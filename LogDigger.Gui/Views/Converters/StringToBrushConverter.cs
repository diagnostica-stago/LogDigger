using System;
using System.Globalization;
using System.Windows.Media;

namespace LogDigger.Gui.Views.Converters
{
    public class StringToBrushConverter : AMarkupConverter
    {
        public double MutiplyBy { get; set; }

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var str = value as string;
            var brushConverter = new BrushConverter();
            Brush brush;
            try
            {
                brush = brushConverter.ConvertFromString(str) as Brush;
            }
            catch (Exception)
            {
                brush = null;
            }

            return brush ?? Brushes.Black;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}