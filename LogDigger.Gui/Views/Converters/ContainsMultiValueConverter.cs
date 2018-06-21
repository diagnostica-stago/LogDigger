using System;
using System.Globalization;
using System.Windows.Data;

namespace LogDigger.Gui.Views.Converters
{
    public class ContainsMultiValueConverter : AMarkupMultiValueConverter
    {
        public IValueConverter InnerConverter { get; set; }

        public override object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var result = false;
            var stringToSearch = values[0] as string;
            for (int i = 1; i < values.Length; i++)
            {
                var dataString = values[i] as string;
                if (string.IsNullOrEmpty(stringToSearch) || string.IsNullOrEmpty(dataString))
                {
                    result = false;
                    break;
                }
                if (dataString.ToLowerInvariant().Contains(stringToSearch.ToLowerInvariant()))
                {
                    result = true;
                    break;
                }
            }
            return InnerConverter?.Convert(result, targetType, parameter, culture) ?? result;
        }

        public override object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}