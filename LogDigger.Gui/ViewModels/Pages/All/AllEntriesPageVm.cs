using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using LogDigger.Business.Models;
using LogDigger.Gui.Properties;
using LogDigger.Gui.ViewModels.Columns;
using LogDigger.Gui.ViewModels.Core;
using LogDigger.Gui.ViewModels.Filters;
using LogDigger.Gui.ViewModels.LogEntries;
using LogDigger.Gui.ViewModels.Pages.ViewProfiles;
using LogDigger.Gui.Views.Pages;
using ReactiveUI;

namespace LogDigger.Gui.ViewModels.Pages.All
{
    public class AllEntriesPageVm : AEntriesPageVm, IPage, IHasHighlights<LogEntryVm>, IItemsHolder<LogEntryVm>
    {
        private readonly INavigator _navigator;
        private readonly ILogParser _parser;
        private IReadOnlyList<LogFile> _files;

        public AllEntriesPageVm(INavigator navigator, ILogParser parser, IReadOnlyList<IColumnDescriptionVm> columns, IModuleClassifier moduleClassifier)
            : base(navigator, columns)
        {
            SortDirection = UserSettings.Default.DateSortDirection;
            IsLoading = true;
            _navigator = navigator;
            _parser = parser;
            ModuleClassifier = moduleClassifier;

            HighlightSearches = BuildHighlights();

            Customization = new ViewCustomization();
        }

        protected virtual ObservableCollection<HighlightSearchItemVm<LogEntryVm>> BuildHighlights()
        {
            return new ObservableCollection<HighlightSearchItemVm<LogEntryVm>>
            {
                new HighlightSearchItemVm<LogEntryVm>(this, "#509e9bc5", x => x.Content),
                new HighlightSearchItemVm<LogEntryVm>(this, "#509eca52", x => x.Content),
                new HighlightSearchItemVm<LogEntryVm>(this, "#50d67e55", x => x.Content)
            };
        }

        public AllEntriesPageVm(List<LogFile> files, INavigator navigator, ILogParser parser, IReadOnlyList<IColumnDescriptionVm> columns, IModuleClassifier moduleClassifier)
            : this(navigator, parser, columns, moduleClassifier)
        {
            _files = files;
        }

        public override async Task Reload(IReadOnlyList<LogFile> files)
        {
            _files = files;
            await Task.Run(CallLoadAll);
            await RefreshAll();
        }

        protected override IEnumerable<IFilterItem<LogEntryVm>> BuildFilters()
        {
            foreach (var column in Columns)
            {
                if (column.FieldFormat.Type == "DateTime")
                {
                    yield return new DateFilterVm<LogEntryVm>(x => x.Data[column.Name] as DateTime? ?? DateTime.MinValue)
                    {
                        Group = column.Name
                    };
                }
                else switch (column.FilterType)
                {
                    case "SimpleListFilter":
                        yield return new SimpleListFilterItemVm<LogEntryVm>(entries => ComputeFilter(entries, e => e.Data[column.Name] as string), e => e.Data[column.Name] as string)
                        {
                            Group = column.Name
                        };
                        break;
                    case "TextFilter":
                        yield return new InclusionStringFilterVm<LogEntryVm>(x => x.Data[column.Name] as string, $"{column.Name} (include)") { Group = column.Name };
                        yield return new ExclusionStringFilterVm<LogEntryVm>(x => x.Data[column.Name] as string, $"{column.Name} (include)") { Group = column.Name };
                        break;
                }
            }

            yield return new HierarchicalListFilterItemVm<LogEntryVm>(ComputeModules, x => x.FileName) { Group = "Modules" };
        }

        private IEnumerable<SelectableVm> ComputeLevels(IEnumerable<LogEntryVm> arg)
        {
            return AllEntries.Select(le => le.Level).Distinct().Select(m => new SelectableVm(m, true)).OrderBy(m => m.Name).ToList();
        }

        private IEnumerable<SelectableVm> ComputeFilter(IEnumerable<LogEntryVm> arg, Func<LogEntryVm, string> selector)
        {
            return AllEntries.Select(selector).Distinct().Select(m => new SelectableVm(m, true)).OrderBy(m => m.Name).ToList();
        }

        private IEnumerable<CompositeSelectableVm> ComputeModules(IEnumerable<LogEntryVm> arg)
        {
            var allFiles = AllEntries.Select(le => le.FileName).Distinct().Select(m => new SelectableVm(m, true)).OrderBy(m => m.Name);
            var allModules = allFiles.GroupBy(x => ModuleClassifier.GetModuleForFile(x.Name)).Select(x => new CompositeSelectableVm(x.Key, true, x.Select(y => y)));
            return allModules.ToList();
        }

        protected override async Task RefreshAll()
        {
            await RefreshFilter();
            await RefreshHighlights();
        }

        public DateFilterVm<LogEntryVm> DateFilter { get; }

        public ObservableCollection<HighlightSearchItemVm<LogEntryVm>> HighlightSearches
        {
            get => GetProperty<ObservableCollection<HighlightSearchItemVm<LogEntryVm>>>();
            set => SetProperty(value);
        }

        public ViewCustomization Customization
        {
            get => GetProperty<ViewCustomization>();
            set => SetProperty(value);
        }

        public string SelectedLevel
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }

        public ObservableCollection<double> HighlightPoints
        {
            get => GetProperty<ObservableCollection<double>>();
            set => SetProperty(value);
        }

        private async Task CallLoadAll()
        {
            IsLoading = true;
            var entries = await Task.Run(() =>
            {
                var total = _files.Count;
                var result = new List<LogEntryVm>();
                int current = 0;
                foreach (var logFile in _files)
                {
                    result.AddRange(logFile.Entries.Select(BuildLogEntry));
                    SetProgress("Building UI", current, total);
                    current++;
                    // uncomment to activate auto refresh
                    // logFile.ActivateFileSystemWatcher();
                    // logFile.NewLinesEvent += OnNewLines;
                }
                return result;
            }).ConfigureAwait(true);

            await Task.Run(() =>
            {
                AllEntries = new ObservableCollection<LogEntryVm>(GetSortedEntries(entries));
                FilteredEntries = AllEntries;
            });

            var currentFilters = 0;
            var totalFilters = Filters.Count;
            SetProgress("Generating filters", currentFilters, totalFilters);
            var filterTasks = new List<Task>();
            foreach (var filter in Filters)
            {
                var task = filter.InitFilter(AllEntries);
                filterTasks.Add(task);
                currentFilters++;
                SetProgress("Generating filters", currentFilters, totalFilters);
            }
            await Task.WhenAll(filterTasks.ToArray());

            IsLoading = false;
        }

        private IOrderedEnumerable<T> GetSortedEntries<T, TFirstSort, TSecondSort>(IEnumerable<T> entries, Func<T, TFirstSort> sort, Func<T, TSecondSort> thenSort)
        {
            if (SortDirection == ListSortDirection.Descending)
            {
                return entries.OrderByDescending(sort).ThenByDescending(thenSort);
            }
            return entries.OrderBy(sort).ThenBy(thenSort);
        }

        protected override IOrderedEnumerable<LogEntryVm> GetSortedEntries(IEnumerable<LogEntryVm> entries)
        {
            return GetSortedEntries(entries, e => e.Date, e => e.SourceLineNumber);
        }
        
        // keep this, for when we implement live update
        private void OnNewLines(object sender, NewLinesEventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (IsLoading == false)
                {
                    foreach (var logEntry in GetSortedEntries(e.Lines, entry => entry.Date, entry => entry.SourceLineNumber))
                    {
                        var logEntryVm = BuildLogEntry(logEntry);
                        // var allActiveFileNames = new HashSet<string>(AllModules.SelectMany(x => x.Children).Where(m => m.IsActive ?? false).Select(x => x.Name));
                        // var allActiveLevels = new HashSet<string>(AllLevels.Where(m => m.IsActive ?? false).Select(x => x.Name));
                        // if (Filter(logEntryVm, allActiveFileNames, allActiveLevels))
                        {
                            if (SortDirection == ListSortDirection.Descending)
                            {
                                FilteredEntries.Insert(0, logEntryVm);
                            }
                            else
                            {
                                FilteredEntries.Add(logEntryVm);
                            }
                        }
                    }
                }
            }));
        }

        protected virtual LogEntryVm BuildLogEntry(LogEntry logEntry)
        {
            return new LogEntryVm(logEntry);
        }

        private Task RefreshHighlights()
        {
            var tasks = new List<Task>();
            foreach (var highlightSearch in HighlightSearches)
            {
                tasks.Add(highlightSearch.RefreshHighlights());
            }
            return Task.Run(() => Task.WaitAll(tasks.ToArray()));
        }

        public ObservableCollection<LogEntryVm> Items => FilteredEntries;

        public void SetSelectedItem(int selectedPoint)
        {
            if (FilteredEntries.Any())
            {
                var item = FilteredEntries[selectedPoint];
                RaiseSelectionChanged(new SelectionChangingEventArgs<LogEntryVm>(selectedPoint, item));
            }
        }

        public override string Title => "ALL";

        public ListSortDirection SortDirection
        {
            get => GetProperty<ListSortDirection>();
            set
            {
                if (SetProperty(value))
                {
                    RefreshSort();
                }
            }
        }

        public ICommand SortCommand => ReactiveCommand.CreateFromTask(CallSort);

        public ICommand FilterFromThisDateCommand => ReactiveCommand.CreateFromTask(CallFilterFromThisDate);

        public ICommand FilterToThisDateCommand => ReactiveCommand.CreateFromTask(CallFilterToThisDate);

        public ICommand FilterRangeCommand => ReactiveCommand.Create(CallFilterRange);

        protected IModuleClassifier ModuleClassifier { get; }

        private void CallFilterRange()
        {
            var selectedEntries = SelectedItems?.Cast<LogEntryVm>();
            var dateFilter = Filters.OfType<DateFilterVm<LogEntryVm>>().FirstOrDefault();
            if (dateFilter != null && SelectedItems != null)
            {
                var lowerTicks = selectedEntries?.FirstOrDefault()?.Date.Ticks;
                if (lowerTicks != null)
                {
                    dateFilter.LowerDateTick = lowerTicks.Value;
                }
                var upperTicks = selectedEntries?.LastOrDefault()?.Date.Ticks;
                if (upperTicks != null)
                {
                    dateFilter.UpperDateTick = upperTicks.Value;
                }
            }
        }

        private Task CallFilterFromThisDate()
        {
            var dateFilter = Filters.OfType<DateFilterVm<LogEntryVm>>().FirstOrDefault();
            if (dateFilter != null)
            {
                dateFilter.LowerDateTick = SelectedItem.Date.Ticks;
            }
            return Task.CompletedTask;
        }

        private Task CallFilterToThisDate()
        {
            var dateFilter = Filters.OfType<DateFilterVm<LogEntryVm>>().FirstOrDefault();
            if (dateFilter != null)
            {
                dateFilter.UpperDateTick = SelectedItem.Date.Ticks;
            }
            return Task.CompletedTask;
        }

        private async Task CallSort()
        {
            SortDirection = SortDirection == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending;
            await RefreshSort();
        }

        private async Task RefreshSort()
        {
            if (AllEntries == null || FilteredEntries == null)
            {
                return;
            }

            var newAllEntries = new ObservableCollection<LogEntryVm>();
            var newFilteredEntries = new ObservableCollection<LogEntryVm>();
            await Task.Run(() =>
            {
                newAllEntries = GetSortedEntries(AllEntries).ToObservableCollection();
                newFilteredEntries = GetSortedEntries(FilteredEntries).ToObservableCollection();
            });
            AllEntries = newAllEntries;
            FilteredEntries = newFilteredEntries;
        }
    }
}
