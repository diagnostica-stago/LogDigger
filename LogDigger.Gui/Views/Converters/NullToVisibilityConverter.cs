using System;
using System.Globalization;
using System.Windows;

namespace LogDigger.Gui.Views.Converters
{
    public class NullToVisibilityConverter : AMarkupConverter
    {
        public NullToVisibilityConverter()
        {
            IsInversed = false;
        }

        public bool Collapse { get; set; }

        public bool IsInversed { get; set; }

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var isVisible = value != null;
            if (IsInversed)
            {
                isVisible = value == null;
            }
            return isVisible ? Visibility.Visible : (Collapse ? Visibility.Collapsed : Visibility.Hidden);
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}