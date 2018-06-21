using System.Windows;
using System.Windows.Controls;

namespace LogDigger.Gui.Views.Controls
{
    public class RefControl : ContentControl
    {
        public static readonly DependencyProperty ChildProperty = DependencyProperty.Register(
            "Child", typeof(FrameworkElement), typeof(RefControl), new PropertyMetadata(default(FrameworkElement), (depObj, args) => ((RefControl)depObj).OnChildChanged(args)));

        private Panel _parentPanel;

        public FrameworkElement Child
        {
            get { return (FrameworkElement) GetValue(ChildProperty); }
            set { SetValue(ChildProperty, value); }
        }

        private void OnChildChanged(DependencyPropertyChangedEventArgs args)
        {
        }

        public RefControl()
        {
            IsVisibleChanged += OnIsVisibleChanged;
        }

        private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (Equals(e.NewValue, true))
            {
                TakeChild();
            }
            else
            {
                ReleaseChild();
            }
        }

        private void TakeChild()
        {
            _parentPanel = Child.Parent as Panel;
            _parentPanel?.Children.Remove(Child);
            Content = Child;
        }

        private void ReleaseChild()
        {
            Content = null;
            _parentPanel?.Children.Add(Child);
        }
    }
}