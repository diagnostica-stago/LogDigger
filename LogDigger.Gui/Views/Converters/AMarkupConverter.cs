using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace LogDigger.Gui.Views.Converters
{
    /// <summary>
    /// Base classe for MarkupConverter in Gui.Fx.
    /// </summary>
    public abstract class AMarkupConverter : MarkupExtension, IValueConverter
    {
        #region Overrides of MarkupExtension

        public override object ProvideValue(System.IServiceProvider serviceProvider)
        {
            return this;
        }

        #endregion
        #region Implementation of IValueConverter

        public abstract object Convert(object value, Type targetType, object parameter, CultureInfo culture);

        public abstract object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture);

        #endregion
    }
}