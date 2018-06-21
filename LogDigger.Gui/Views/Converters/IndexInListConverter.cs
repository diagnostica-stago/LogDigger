using System;
using System.Collections.Generic;
using System.Globalization;

namespace LogDigger.Gui.Views.Converters
{
    public class IndexInListConverter : AMarkupMultiValueConverter
    {
        public override object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var item = values[0] as string;
            var list = values[1] as IList<string>;
            var itemWidth = (double)parameter;
            return list.IndexOf(item) * itemWidth;
        }

        public override object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}