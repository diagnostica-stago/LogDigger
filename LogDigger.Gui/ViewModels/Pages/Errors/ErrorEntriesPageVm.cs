using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using LogDigger.Business.Models;
using LogDigger.Gui.ViewModels.Columns;
using LogDigger.Gui.ViewModels.Core;
using LogDigger.Gui.ViewModels.Filters;
using LogDigger.Gui.ViewModels.LogEntries;
using LogDigger.Gui.Views.Pages;

namespace LogDigger.Gui.ViewModels.Pages.Errors
{
    public class ErrorEntriesPageVm : AEntriesPageVm, IPage
    {
        private List<LogFile> _files;

        public ErrorEntriesPageVm(INavigator navigator, IReadOnlyList<IColumnDescriptionVm> columns)
            : base(navigator, columns)
        {
            IsLoading = true;
        }

        public ErrorEntriesPageVm(List<LogFile> files, INavigator navigator, IReadOnlyList<IColumnDescriptionVm> columns)
            : this(navigator, columns)
        {
            _files = files;
        }

        public override string Title => "ERRORS";

        public override string Icon => "Stackoverflow";

        public override IReadOnlyList<LogFile> FilterFiles(IReadOnlyList<LogFile> files)
        {
            return files.Where(f => f.IsErrorsFile).ToList();
        }

        public override async Task Reload(IReadOnlyList<LogFile> files)
        {
            _files = files.Where(f => f.IsErrorsFile).ToList();
            await CallLoadErrors();
            await RefreshAll();
        }

        public ObservableCollection<LogEntryVm> ErrorsEntries
        {
            get { return GetProperty<ObservableCollection<LogEntryVm>>(); }
            private set { SetProperty(value); }
        }

        private async Task CallLoadErrors()
        {
            IsLoading = true;
            var entries = await Task.Run(
                    () => _files.Where(f => f.IsErrorsFile)
                        .SelectMany(lf => lf.Entries)
                        .Where(le => le.IsException)
                        .Select(le => new LogEntryVm(le))
                        .ToList())
                .ConfigureAwait(true);
            AllEntries = new ObservableCollection<LogEntryVm>(GetSortedEntries(entries));
            IsLoading = false;
        }

        protected override IEnumerable<IFilterItem<LogEntryVm>> BuildFilters()
        {
            yield break;
        }

        protected override IOrderedEnumerable<LogEntryVm> GetSortedEntries(IEnumerable<LogEntryVm> entries)
        {
            return entries.OrderByDescending(x => x.Data["date"]);
        }
    }
}
