using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;

namespace LogDigger.Gui.Views.Controls
{
    public class LoadingPanel : ContentControl
    {
        public static readonly DependencyProperty IsLoadingProperty = DependencyProperty.Register("IsLoading", typeof(bool), typeof(LoadingPanel), new PropertyMetadata(default(bool)));

        public static readonly DependencyProperty ProgressInfoProperty = DependencyProperty.Register("ProgressInfo", typeof(string), typeof(LoadingPanel), new PropertyMetadata(default(string)));

        public string ProgressInfo
        {
            get { return (string) GetValue(ProgressInfoProperty); }
            set { SetValue(ProgressInfoProperty, value); }
        }

        public bool IsLoading
        {
            get { return (bool) GetValue(IsLoadingProperty); }
            set { SetValue(IsLoadingProperty, value); }
        }

        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline", Justification = "Necessary initialization for custom control styling in WPF")]
        static LoadingPanel()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(LoadingPanel), new FrameworkPropertyMetadata(typeof(LoadingPanel)));
        }
    }
}