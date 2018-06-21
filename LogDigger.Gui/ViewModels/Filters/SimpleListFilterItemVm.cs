using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using LogDigger.Gui.ViewModels.Core;
using ReactiveUI;

namespace LogDigger.Gui.ViewModels.Filters
{
    public class HierarchicalListFilterItemVm<TItem> : SimpleListFilterItemVm<TItem>
    {
        public HierarchicalListFilterItemVm(Func<IEnumerable<TItem>, IEnumerable<CompositeSelectableVm>> initializer, Func<TItem, string> propertyAccessor) 
            : base(initializer, propertyAccessor)
        {
        }

        protected override List<string> RetrieveParam()
        {
            return Selectors.Cast<CompositeSelectableVm>().SelectMany(x => x.Children).Where(x => x.IsActive == true).Select(x => x.Name).ToList();
        }
    }

    public class SimpleListFilterItemVm<TItem> : AFilterItemVm<List<string>, TItem>
    {
        private readonly Func<TItem, string> _propertyAccessor;
        private readonly Func<IEnumerable<TItem>, IEnumerable<SelectableVm>> _initializer;

        public SimpleListFilterItemVm(Func<IEnumerable<TItem>, IEnumerable<SelectableVm>> initializer, Func<TItem, string> propertyAccessor)
        {
            _initializer = initializer;
            _propertyAccessor = propertyAccessor;
            Selectors = new ReactiveList<SelectableVm>();
            Selectors.ChangeTrackingEnabled = true;
            Selectors.ItemChanged.Subscribe(x => this.RaisePropertyChanged(nameof(AllSelected)));
        }

        public ReactiveList<SelectableVm> Selectors
        {
            get => GetProperty<ReactiveList<SelectableVm>>();
            set => SetProperty(value);
        }

        public bool? AllSelected
        {
            get
            {
                var activeModuleCount = 0;
                foreach (var module in Selectors)
                {
                    if (module.IsActive ?? false)
                    {
                        activeModuleCount++;
                    }
                }
                return activeModuleCount == Selectors.Count
                    ? true
                    : activeModuleCount == 0
                        ? false
                        : (bool?)null;
            }
            set
            {
                if (SetProperty(value) && value.HasValue)
                {
                    foreach (var module in Selectors)
                    {
                        module.IsActive = value.Value;
                    }
                }
            }
        }

        public override async Task InitFilter(IEnumerable<TItem> list)
        {
            var items = await Task.Run(() => _initializer(list));
            await Dispatch(() =>
            {
                Selectors.Clear();
                foreach (var item in items)
                {
                    Selectors.Add(item);
                }
            });
        }

        public override IObservable<object> WhenFilterChanged()
        {
            return Selectors.ItemChanged.Cast<object>().Merge(Selectors.WhenAnyPropertyChanged());
        }

        protected override List<string> RetrieveParam()
        {
            return Selectors.Where(x => x.IsActive == true).Select(x => x.Name).ToList();
        }

        protected override bool FilterItem(TItem entry, List<string> param)
        {
            return param.Contains(_propertyAccessor(entry));
        }
    }
}