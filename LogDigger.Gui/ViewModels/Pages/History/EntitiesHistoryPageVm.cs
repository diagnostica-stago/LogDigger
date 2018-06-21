using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using LogDigger.Business.Models;
using LogDigger.Gui.ViewModels.Core;
using LogDigger.Gui.ViewModels.Filters;
using LogDigger.Gui.ViewModels.LogEntries;
using LogDigger.Gui.ViewModels.Pages.All;
using ReactiveUI;
using SimpleLogger;
using Sprache;

namespace LogDigger.Gui.ViewModels.Pages.History
{
    public class EntitiesHistoryPageVm : APageVm, ICloseable
    {
        private IReadOnlyList<LogFile> _files;
        private IReadOnlyList<LogEntry> _dbEntries;
        private IReadOnlyList<Entity> _entities;

        public EntitiesHistoryPageVm(INavigator navigator, AllEntriesPageVm allEntriesPageVm) : base(navigator)
        {
            EntityGroups = new ObservableCollection<EntityGroup>();
            allEntriesPageVm.WhenAnyValue(x => x.SelectedItem).ObserveOnDispatcher().Subscribe(OnSelectedItemChanged);
            this.WhenAnyValue(x => x.Filter).Throttle(TimeSpan.FromMilliseconds(300)).Subscribe(OnFilter);
            DateFilter = new DateFilterVm<LogEntry>(x => x.Date)
            {
                MinimumDateTick = 0,
                MaximumDateTick = long.MaxValue,
                LowerDateTick = 0,
                UpperDateTick = long.MaxValue
            };
            DateFilter.WhenFilterChanged().Subscribe(x => OnFilterChanged());
        }

        public override string Icon => "Database";

        private void OnFilterChanged()
        {
            Task.Run(() => LoadEntries(_dbEntries));
        }

        public string Filter
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }

        public DateFilterVm<LogEntry> DateFilter { get; }

        public ObservableCollection<EntityGroup> EntityGroups
        {
            get => GetProperty<ObservableCollection<EntityGroup>>();
            set => SetProperty(value);
        }

        public override string Title
        {
            get { return "Entities history"; }
        }

        private void OnFilter(string filter)
        {
            Task.Run(() => FilterEntities(_entities, filter));
        }

        private void FilterEntities(IReadOnlyList<Entity> entities, string filter)
        {
            if (entities == null)
            {
                return;
            }
            var filteredEntities = entities.Where(x => x.Match(filter));
            EntityGroups = new ObservableCollection<EntityGroup>(filteredEntities.GroupBy(x => x.Type).Select(x => new EntityGroup(x.Key, x)).OrderBy(x => x.Type));
        }

        private void OnSelectedItemChanged(LogEntryVm entry)
        {
            if (entry == null)
            {
                return;
            }

            IsLoading = true;
            DateFilter.MaximumDateTick = entry.Date.Ticks;
            // Task.Run(() => LoadEntries(_dbEntries, entry.Date));
            IsLoading = false;
        }

        public override async Task Reload(IReadOnlyList<LogFile> files)
        {
            IsLoading = true;
            _files = files;
            await Task.Run(() => LoadAll(files));
            IsLoading = false;
        }

        private void LoadAll(IReadOnlyList<LogFile> files)
        {
            _dbEntries = files.SelectMany(x => x.Entries).Where(x => x.Content.Contains("db-")).OrderBy(x => x.Date).ToList();
            DateFilter.InitFilter(_dbEntries);
            LoadEntries(_dbEntries);
        }

        private void LoadEntries(IReadOnlyList<LogEntry> dbEntries)
        {
            var entities = new ConcurrentDictionary<string, Entity>();
            var entries = DateFilter.Filter(dbEntries);
            foreach (var logEntry in entries)
            {
                try
                {
                    var dbEvent = EntityParser.DbEvent.Parse(logEntry.Content);
                    var entity = dbEvent.Entity;
                    entity.EntitiesLookup = entities;
                    if (dbEvent.Operation == Operation.Create || dbEvent.Operation == Operation.Update)
                    {
                        entities.AddOrUpdate(entity.HashId, entity, (key, oldValue) => entity);
                    }
                    else if (dbEvent.Operation == Operation.Delete)
                    {
                        entities.TryRemove(entity.HashId, out entity);
                    }
                }
                catch (Exception)
                {
                    Logger.Log(Logger.Level.Warning, $"DbEvent parse error on content: {logEntry.Content}");
                }
            }
            _entities = entities.Values.ToList();
            FilterEntities(_entities, Filter);
        }

        public void Close()
        {
        }
    }
}