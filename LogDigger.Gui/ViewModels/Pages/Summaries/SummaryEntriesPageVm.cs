using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using LogDigger.Business.Models;
using LogDigger.Business.Services;
using LogDigger.Gui.ViewModels.Core;
using LogDigger.Gui.ViewModels.Filters;
using ReactiveUI;

namespace LogDigger.Gui.ViewModels.Pages.Summaries
{
    public class SummaryEntriesPageVm : AFilterPageVm<ALogSummaryVm>, IHasHighlights<ALogSummaryVm>, IItemsHolder<ALogSummaryVm>
    {
        private IReadOnlyList<LogFile> _files;
        private LogTemplateService _templateService;

        public SummaryEntriesPageVm(INavigator navigator)
            : base(navigator)
        {
            _templateService = new LogTemplateService();
            _templateService.TemplatesUpdated += OnTemplatesUpdated;
            OpenTemplateEditionCommand = ReactiveCommand.Create(CallOpenTemplateEdition);
            HighlightSearches = new ObservableCollection<HighlightSearchItemVm<ALogSummaryVm>>();
            HighlightSearches.Add(new HighlightSearchItemVm<ALogSummaryVm>(this, "#509e9bc5", x => x.Details));
            HighlightSearches.Add(new HighlightSearchItemVm<ALogSummaryVm>(this, "#509eca52", x => x.Details));
            HighlightSearches.Add(new HighlightSearchItemVm<ALogSummaryVm>(this, "#50d67e55", x => x.Details));
        }


        public override string Title => "Summary";

        public override string Icon => "ViewList";

        public ICommand OpenTemplateEditionCommand { get; }

        public ObservableCollection<CustomLogTemplateVm> Templates
        {
            get => GetProperty<ObservableCollection<CustomLogTemplateVm>>();
            set => SetProperty(value);
        }

        private void OnTemplatesUpdated(object sender, EventArgs e)
        {
            Reload(_files);
        }

        private void CallOpenTemplateEdition()
        {
            Navigator.AddPage(new TemplatesEditionPageVm(Navigator, _templateService));
        }

        public override async Task Reload(IReadOnlyList<LogFile> files)
        {
            IsLoading = true;
            _files = files;
            var templates = _templateService.GetTemplates();
            Templates = new ObservableCollection<CustomLogTemplateVm>(templates.Select(x => new CustomLogTemplateVm(x)));
            await Task.Run(() =>
            {
                var entries = files.SelectMany(x => x.Entries).ToList();
                var totalEntries = entries.Count;
                var currentEntry = 0;
                var groups = new ConcurrentDictionary<object, ConcurrentBag<LogEntry>>();

                foreach (var entry in entries)
                {
                    groups.AddOrUpdate(GroupBy(entry)
                        , (id) =>
                        {
                            currentEntry++;
                            SetProgress("Building UI", currentEntry, totalEntries);
                            return new ConcurrentBag<LogEntry> { entry };
                        }, (id, list) =>
                        {
                            list.Add(entry);
                            currentEntry++;
                            SetProgress("Building UI", currentEntry, totalEntries);
                            return list;
                        });
                }
                var summaryEntries = new List<ALogSummaryVm>();
                var current = 0;
                var total = groups.Count;
                foreach (var idGroup in groups)
                {
                    foreach (var entry in idGroup.Value)
                    {
                        var summary = LogSummaryFactory.BuildLogSummary(templates, entry);
                        if (summary != null)
                        {
                            summary.LookForExtraInformation(idGroup.Value);
                            summaryEntries.Add(summary);
                        }
                    }
                    SetProgress("Building summary view", current, total);
                    current++;
                }
                
                AllEntries = new ObservableCollection<ALogSummaryVm>(summaryEntries.OrderBy(x => x.Date));
            });
            foreach (var filter in Filters)
            {
                filter.InitFilter(AllEntries);
            }
            IsLoading = false;
            await RefreshAll();
        }

        protected virtual object GroupBy(LogEntry entry)
        {
            return entry;
        }

        public ObservableCollection<HighlightSearchItemVm<ALogSummaryVm>> HighlightSearches
        {
            get => GetProperty<ObservableCollection<HighlightSearchItemVm<ALogSummaryVm>>>();
            set => SetProperty(value);
        }

        public ObservableCollection<ALogSummaryVm> Items => AllEntries;

        public void SetSelectedItem(int index)
        {
            if (AllEntries?.Any() ?? false)
            {
                var item = AllEntries[index];
                RaiseSelectionChanged(new SelectionChangingEventArgs<ALogSummaryVm>(index, item));
            }
        }

        public event EventHandler<SelectionChangingEventArgs<ALogSummaryVm>> SelectionChanged;

        protected virtual void RaiseSelectionChanged(SelectionChangingEventArgs<ALogSummaryVm> e)
        {
            SelectionChanged?.Invoke(this, e);
        }

        protected override IEnumerable<IFilterItem<ALogSummaryVm>> BuildFilters()
        {
            yield return new InclusionStringFilterVm<ALogSummaryVm>(x => x.Details, "Details (include)") { Group = "Details" };
            yield return new ExclusionStringFilterVm<ALogSummaryVm>(x => x.Details, "Details (exclude)") { Group = "Details" };
            yield return new DateFilterVm<ALogSummaryVm>(x => x.Date) { Group = "Date" };
            yield return new SimpleListFilterItemVm<ALogSummaryVm>(ComputeTypes, x => x.TypeName) { Group = "Type" };
        }

        private IEnumerable<SelectableVm> ComputeTypes(IEnumerable<ALogSummaryVm> summaries)
        {
            return summaries.GroupBy(x => x.TypeName).Select(x => new SelectableVm(x.Key, true));
        }

        protected override IOrderedEnumerable<ALogSummaryVm> GetSortedEntries(IEnumerable<ALogSummaryVm> entries)
        {
            return entries.OrderBy(x => x.Date);
        }
    }
}
