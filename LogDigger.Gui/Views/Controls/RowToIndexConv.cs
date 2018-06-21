using System;
using System.Globalization;
using System.Windows.Controls;
using LogDigger.Gui.Views.Converters;

namespace LogDigger.Gui.Views.Controls
{
    public class RowToIndexConv : AMarkupConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var row = value as DataGridRow;
            
            if (row == null)
            {
                return 0;
            }

            return row.GetIndex() + 1;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
