using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace LogDigger.Gui.Views.Controls
{
    /// <summary>
    /// Copy of the class in the .NET Framework (System.Windows.Controls) to avoid binding errors
    /// Converts DataGridHeadersVisibility to Visibility based on the given parameter.
    /// </summary> 
    [Localizability(LocalizationCategory.NeverLocalize)]
    public sealed class DataGridHeadersVisibilityToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Convert DataGridHeadersVisibility to Visibility
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var visible = false;

            if (value is DataGridHeadersVisibility && parameter is DataGridHeadersVisibility)
            {
                var valueAsDataGridHeadersVisibility = (DataGridHeadersVisibility)value;
                var parameterAsDataGridHeadersVisibility = (DataGridHeadersVisibility)parameter;

                switch (valueAsDataGridHeadersVisibility)
                {
                    case DataGridHeadersVisibility.All:
                        visible = true;
                        break;
                    case DataGridHeadersVisibility.Column:
                        visible = parameterAsDataGridHeadersVisibility == DataGridHeadersVisibility.Column ||
                                    parameterAsDataGridHeadersVisibility == DataGridHeadersVisibility.None;
                        break;
                    case DataGridHeadersVisibility.Row:
                        visible = parameterAsDataGridHeadersVisibility == DataGridHeadersVisibility.Row ||
                                    parameterAsDataGridHeadersVisibility == DataGridHeadersVisibility.None;
                        break;
                }
            }

            if (targetType == typeof(Visibility))
            {
                return visible ? Visibility.Visible : Visibility.Collapsed;
            }
            else
            {
                return DependencyProperty.UnsetValue;
            }
        }

        /// <summary>
        ///     Not implemented
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
