using System;

namespace LogDigger.Gui.ViewModels.Filters
{
    public class FilterItemVm<TParam, TItem> : AFilterItemVm<TParam, TItem>, IFilterItem<TItem>
    {
        private readonly Func<TParam> _paramBuilder;
        private readonly Func<TParam, TItem, bool> _filterPredicate;

        public FilterItemVm(Func<TParam> paramBuilder, Func<TParam, TItem, bool> filterPredicate)
        {
            _paramBuilder = paramBuilder;
            _filterPredicate = filterPredicate;
        }

        protected override TParam RetrieveParam()
        {
            return _paramBuilder();
        }

        protected override bool FilterItem(TItem entry, TParam param)
        {
            return _filterPredicate(param, entry);
        }
    }

    public class FilterItemVm<TItem> : FilterItemVm<object, TItem>
    {

        public FilterItemVm(Func<TItem, bool> filterPredicate)
            : base(() => null, (o, item) => filterPredicate(item))
        {
        }
    }
}