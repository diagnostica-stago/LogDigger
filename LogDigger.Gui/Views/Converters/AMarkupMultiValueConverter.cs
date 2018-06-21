using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace LogDigger.Gui.Views.Converters
{
    /// <summary>
    /// A markup converter to do multibinding
    /// </summary>
    public abstract class AMarkupMultiValueConverter : MarkupExtension, IMultiValueConverter
    {
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }

        public abstract object Convert(object[] values, Type targetType, object parameter, CultureInfo culture);

        public abstract object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture);
    }
}