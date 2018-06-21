using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using LogDigger.Gui.ViewModels.Core;
using LogDigger.Gui.ViewModels.Filters;

namespace LogDigger.Gui.ViewModels.Pages
{
    public abstract class AFilterPageVm<TItem> : APageVm
    {
        private readonly object _syncTokens = new object();
        private readonly IList<CancellationTokenSource> _tokenSources;
        private IDisposable _cancelSub;
        private IDisposable _refreshSub;

        protected AFilterPageVm(INavigator navigator) : base(navigator)
        {
            _tokenSources = new List<CancellationTokenSource>();
        }

        public ObservableCollection<IFilterItem<TItem>> Filters
        {
            get => GetProperty<ObservableCollection<IFilterItem<TItem>>>();
            set => SetProperty(value);
        }

        protected abstract IEnumerable<IFilterItem<TItem>> BuildFilters();
        
        private void InitFilters()
        {
            _cancelSub?.Dispose();
            _refreshSub?.Dispose();

            Filters = BuildFilters().ToObservableCollection();
            var filterChanges = Observable.Empty<object>();
            foreach (var filter in Filters)
            {
                filterChanges = filterChanges.Merge(filter.WhenFilterChanged());
            }
            filterChanges = filterChanges.Where(x => IsLoading == false);
            _cancelSub = filterChanges.Subscribe(b => CancelAll());
            _refreshSub = filterChanges.Throttle(TimeSpan.FromMilliseconds(400)).Subscribe(b => RefreshAll());
        }

        protected virtual async Task RefreshAll()
        {
            await RefreshFilter();
        }

        protected override void PrepareLoad()
        {
            InitFilters();
        }

        protected async Task RefreshFilter()
        {
            if (IsLoading)
            {
                return;
            }

            UpdatingFilter = true;
            var logEntryVms = new List<TItem>();
            var cancellationTokenSource = new CancellationTokenSource();
            var token = cancellationTokenSource.Token;
            lock (_syncTokens)
            {
                _tokenSources.Add(cancellationTokenSource);
            }
            try
            {
                await Task.Run(() =>
                    {
                        if (AllEntries == null)
                        {
                            return;
                        }
                        IEnumerable<TItem> filteredEntries = new List<TItem>(AllEntries);
                        foreach (var filter in Filters)
                        {
                            filteredEntries = filter.Filter(filteredEntries).ToList();
                        }
                        logEntryVms = GetSortedEntries(filteredEntries).ToList();
                    },
                    token);
            }
            catch (TaskCanceledException)
            {
                // the task has been canceled
            }

            if (!token.IsCancellationRequested)
            {
                FilteredEntries = new ObservableCollection<TItem>(logEntryVms);
                UpdatingFilter = false;
            }
            lock (_syncTokens)
            {
                _tokenSources.Remove(cancellationTokenSource);
            }
        }

        protected abstract IOrderedEnumerable<TItem> GetSortedEntries(IEnumerable<TItem> entries);

        public ObservableCollection<TItem> AllEntries
        {
            get => GetProperty<ObservableCollection<TItem>>();
            set => SetProperty(value);
        }

        public ObservableCollection<TItem> FilteredEntries
        {
            get => GetProperty<ObservableCollection<TItem>>();
            set => SetProperty(value);
        }

        protected void CancelAll()
        {
            lock (_syncTokens)
            {
                foreach (var tokenSource in _tokenSources)
                {
                    tokenSource.Cancel();
                }
            }
        }

        public bool UpdatingFilter
        {
            get => GetProperty<bool>();
            set => SetProperty(value);
        }
    }
}