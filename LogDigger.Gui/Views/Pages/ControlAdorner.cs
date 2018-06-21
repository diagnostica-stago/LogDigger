using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace LogDigger.Gui.Views.Pages
{
    public class ControlAdorner : Adorner
    {
        private FrameworkElement _child;
        private readonly AdornerLayer _adornerLayer;

        public ControlAdorner(UIElement adornedElement, FrameworkElement child, object dataContext, object adornerContext)
            : base(adornedElement)
        {
            Child = child;
            Child.DataContext = dataContext;
            AdornerProperties.SetAdornerContext(Child, adornerContext);
            _adornerLayer = AdornerLayer.GetAdornerLayer(AdornedElement);
            _adornerLayer.Add(this);
            IsHitTestVisible = true;
        }

        protected override int VisualChildrenCount
        {
            get { return 1; }
        }

        public FrameworkElement Child
        {
            get
            {
                return _child;
            }
            set
            {
                if (_child != null)
                {
                    RemoveVisualChild(_child);
                }
                _child = value;
                if (_child != null)
                {
                    AddVisualChild(_child);
                }
            }
        }

        protected override Visual GetVisualChild(int index)
        {
            if (index != 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            return _child;
        }

        protected override Size MeasureOverride(Size constraint)
        {
            _child.Measure(constraint);
            return _child.DesiredSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            _child.Arrange(new Rect(new Point(_child.Margin.Left, _child.Margin.Top), finalSize));
            return new Size(_child.ActualWidth, _child.ActualHeight);
        }

        public void Show()
        {
            _adornerLayer.Update(AdornedElement);
            Visibility = Visibility.Visible;
        }

        public void Hide()
        {
            Visibility = Visibility.Collapsed;
        }
    }

    public static class AdornerProperties
    {
        public static readonly DependencyProperty AdornerContextProperty = DependencyProperty.RegisterAttached("AdornerContext", typeof(object), typeof(AdornerProperties), new FrameworkPropertyMetadata(default(object), FrameworkPropertyMetadataOptions.Inherits));

        public static void SetAdornerContext(DependencyObject element, object value)
        {
            element.SetValue(AdornerContextProperty, value);
        }

        public static object GetAdornerContext(DependencyObject element)
        {
            return (object) element.GetValue(AdornerContextProperty);
        }
    }
}
