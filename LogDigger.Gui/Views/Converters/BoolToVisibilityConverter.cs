using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace LogDigger.Gui.Views.Converters
{
    /// <summary>
    /// Converter bool to wpf Visibility
    /// </summary>
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BoolToVisibilityConverter : AMarkupConverter
    {
        public BoolToVisibilityConverter()
        {
            IsInversed = false;
            Collapse = true;
        }

        public bool Collapse { get; set; }

        public bool IsInversed { get; set; }

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool boolValue = (bool)value;
            if (IsInversed)
            {
                boolValue = !boolValue;
            }
            return boolValue ? Visibility.Visible : (Collapse ? Visibility.Collapsed : Visibility.Hidden);
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}