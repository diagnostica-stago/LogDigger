using System;
using System.Collections;
using System.Globalization;
using System.Linq;

namespace LogDigger.Gui.Views.Converters
{
    public class HasMultipleItemsConverter : AMarkupConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var ienum = value as IEnumerable;
            var ienumObj = ienum?.Cast<object>();
            return ienumObj?.Count() > 1;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}