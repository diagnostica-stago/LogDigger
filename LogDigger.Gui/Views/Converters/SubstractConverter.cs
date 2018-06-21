using System;
using System.Globalization;

namespace LogDigger.Gui.Views.Converters
{
    public class SubstractConverter : AMarkupConverter
    {
        public double Number { get; set; }

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var doubleVal = value is double ? (double?) value : null;
            if (doubleVal.HasValue)
            {
                return doubleVal - Number;
            }
            return value;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}