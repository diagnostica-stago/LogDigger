using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LogDigger.Gui.ViewModels.Core;

namespace LogDigger.Gui.ViewModels.Filters
{
    public abstract class AFilterItemVm<TParam, TItem> : AViewModel, IFilterItem<TItem>
    {
        protected AFilterItemVm(string name = null)
        {
            Name = name;
        }

        public string Name { get; }

        public string Group
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }

        public virtual Task InitFilter(IEnumerable<TItem> list)
        {
            return Task.CompletedTask;
        }

        public virtual IEnumerable<TItem> Filter(IEnumerable<TItem> list)
        {
            var param = RetrieveParam();
            var items = new List<TItem>();
            Parallel.ForEach(list, () => new List<TItem>(),
                (entry, state, result) =>
                {
                    if (FilterItem(entry, param))
                    {
                        result.Add(entry);
                    }
                    return result;
                },
                lst =>
                {
                    lock (items)
                    {
                        items.AddRange(lst);
                    }
                });
            return items;
        }

        public virtual IObservable<object> WhenFilterChanged()
        {
            return this.WhenAnyPropertyChanged();
        }

        protected abstract TParam RetrieveParam();

        protected abstract bool FilterItem(TItem entry, TParam param);
    }

    public abstract class AFilterItemVm<TItem> : AFilterItemVm<object, TItem>
    {
        protected AFilterItemVm(string name = null)
            : base(name)
        {
        }

        protected override object RetrieveParam()
        {
            return null;
        }

        protected sealed override bool FilterItem(TItem entry, object param)
        {
            return FilterItem(entry);
        }

        protected abstract bool FilterItem(TItem entry);
    }
}