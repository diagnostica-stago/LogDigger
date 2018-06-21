using System;
using System.Globalization;

namespace LogDigger.Gui.Views.Converters
{
    public class GreaterThanMultiValueConverter : AMarkupMultiValueConverter
    {
        public double Shift { get; set; }

        public override object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var firstVal = values[0] is double fd ? fd : 0;
            var secondVal = values[1] is double sd ? sd : 0;

            return firstVal + Shift > secondVal;
        }

        public override object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}