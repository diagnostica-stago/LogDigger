using System;
using System.ComponentModel;
using System.Globalization;

namespace LogDigger.Gui.Views.Converters
{
    public class BoolToSortDirectionConverter : AMarkupConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var sortDirVal = (ListSortDirection)value;
            return sortDirVal == ListSortDirection.Ascending;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var boolVal = (bool)value;
            return boolVal ? ListSortDirection.Ascending : ListSortDirection.Descending;
        }
    }
}