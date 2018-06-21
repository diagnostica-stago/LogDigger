using System;
using System.Text.RegularExpressions;
using LogDigger.Business.Utils;

namespace LogDigger.Gui.ViewModels.Filters
{
    public class ExclusionStringFilterVm<TItem> : AStringFilterVm<TItem>
    {
        public ExclusionStringFilterVm(Func<TItem, string> propertyAccess, string name = null)
            : base(propertyAccess, name)
        {
        }

        protected override bool FilterItem(TItem entry)
        {
            var content = PropertyAccess(entry);
            if (string.IsNullOrEmpty(FilterValue))
            {
                return true;
            }
            if (IsRegex)
            {
                if (content == null)
                {
                    return true;
                }
                return !Regex.Match(content, FilterValue).Success;
            }
            return !(content?.Contains(FilterValue, StringComparison.OrdinalIgnoreCase) ?? false);
        }
    }
}