using System;
using System.Globalization;
using System.Windows.Data;

namespace LogDigger.Gui.Views.Converters
{
    [ValueConversion(typeof(string), typeof(string))]
    public class StringToUpperCaseConverter : AMarkupConverter
    {
        public StringConvertion Convertion { get; set; }

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var stringValue = value as string;
            if (string.IsNullOrWhiteSpace(stringValue))
            {
                return null;
            }
            switch (Convertion)
            {
                case StringConvertion.All:
                    return stringValue.ToUpper(CultureInfo.CurrentCulture);
                case StringConvertion.FirstCharOnly:
                    return string.Concat(stringValue.Substring(0, 1).ToUpper(CultureInfo.CurrentCulture), stringValue.Substring(1, stringValue.Length - 1));
                default:
                    return string.Empty;
            }
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}