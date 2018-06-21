using System;
using System.Collections.Generic;

namespace LogDigger.Gui.ViewModels.Filters
{
    public abstract class AStringFilterVm<TItem> : AFilterItemVm<TItem>
    {
        protected AStringFilterVm(Func<TItem, string> propertyAccess, string name = null)
             : base(name)
        {
            PropertyAccess = propertyAccess;
        }

        public string FilterValue
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }

        public bool IsRegex
        {
            get => GetProperty<bool>();
            set => SetProperty(value);
        }

        protected Func<TItem, string> PropertyAccess { get; }

        public override IEnumerable<TItem> Filter(IEnumerable<TItem> list)
        {
            if (string.IsNullOrEmpty(FilterValue))
            {
                return list;
            }

            return base.Filter(list);
        }
    }
}