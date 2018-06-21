using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace LogDigger.Gui.Views.Controls
{
    public class DeferLoadingControl : ContentControl
    {
        public DeferLoadingControl()
        {
            Visibility = Visibility.Collapsed;
            Loaded += OnThisLoadedChanged;
        }

        private async void OnThisLoadedChanged(object sender, RoutedEventArgs routedEventArgs)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(300));
            Visibility = Visibility.Visible;
        }
    }

    public class DeferredContent : ContentPresenter
    {
        public DataTemplate DeferredContentTemplate
        {
            get { return (DataTemplate)GetValue(DeferredContentTemplateProperty); }
            set { SetValue(DeferredContentTemplateProperty, value); }
        }

        public static readonly DependencyProperty DeferredContentTemplateProperty =
            DependencyProperty.Register("DeferredContentTemplate",
            typeof(DataTemplate), typeof(DeferredContent), null);

        public DeferredContent()
        {
            Loaded += HandleLoaded;
        }

        private void HandleLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= HandleLoaded;
            // await Task.Delay(TimeSpan.FromMilliseconds(150));
            Application.Current.Dispatcher.BeginInvoke(new Action(ShowDeferredContent));
        }

        public void ShowDeferredContent()
        {
            if (DeferredContentTemplate != null)
            {
                Content = DeferredContentTemplate.LoadContent();
            }
        }
    }
}