using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interactivity;
using LogDigger.Gui.Views.Pages;

namespace LogDigger.Gui.Views.Behaviors
{
    public enum OverlookMouseBehavior
    {
        OnMouseOver,
        OnMouseClick,
        OnMouseOverAndCtrl
    }

    public class ShowAdornerOnMouseOverBehavior : Behavior<FrameworkElement>
    {
        private static HashSet<ShowAdornerOnMouseOverBehavior> _openedAdorner =
            new HashSet<ShowAdornerOnMouseOverBehavior>();

        public static readonly DependencyProperty AdornerTemplateProperty = DependencyProperty.Register(
            "AdornerTemplate", typeof(DataTemplate), typeof(ShowAdornerOnMouseOverBehavior),
            new PropertyMetadata(default(DataTemplate)));

        public static readonly DependencyProperty ConditionProperty = DependencyProperty.Register(
            "Condition", typeof(bool), typeof(ShowAdornerOnMouseOverBehavior),
            new PropertyMetadata(true, (obj, args) => ((ShowAdornerOnMouseOverBehavior) obj).OnConditionChanged()));

        public static readonly DependencyProperty AdornedElementProperty = DependencyProperty.Register(
            "AdornedElement", typeof(FrameworkElement), typeof(ShowAdornerOnMouseOverBehavior),
            new PropertyMetadata(default(FrameworkElement)));

        public static readonly DependencyProperty GroupProperty = DependencyProperty.RegisterAttached(
            "Group", typeof(string), typeof(ShowAdornerOnMouseOverBehavior),
            new FrameworkPropertyMetadata(default(string), FrameworkPropertyMetadataOptions.Inherits));

        public static readonly DependencyProperty MouseEventProperty = DependencyProperty.Register(
            "MouseEvent", typeof(OverlookMouseBehavior), typeof(ShowAdornerOnMouseOverBehavior),
            new PropertyMetadata(OverlookMouseBehavior.OnMouseOver));

        public static readonly DependencyProperty IsAdornerLockedProperty = DependencyProperty.RegisterAttached(
            "IsAdornerLocked", typeof(bool), typeof(ShowAdornerOnMouseOverBehavior),
            new FrameworkPropertyMetadata(default(bool), FrameworkPropertyMetadataOptions.Inherits));

        public static void SetIsAdornerLocked(DependencyObject element, bool value)
        {
            element.SetValue(IsAdornerLockedProperty, value);
        }

        public static bool GetIsAdornerLocked(DependencyObject element)
        {
            return (bool) element.GetValue(IsAdornerLockedProperty);
        }

        public OverlookMouseBehavior MouseEvent
        {
            get { return (OverlookMouseBehavior) GetValue(MouseEventProperty); }
            set { SetValue(MouseEventProperty, value); }
        }

        public static void SetGroup(DependencyObject element, string value)
        {
            element.SetValue(GroupProperty, value);
        }

        public static string GetGroup(DependencyObject element)
        {
            return (string) element.GetValue(GroupProperty);
        }

        public FrameworkElement AdornedElement
        {
            get { return (FrameworkElement) GetValue(AdornedElementProperty); }
            set { SetValue(AdornedElementProperty, value); }
        }

        public bool Condition
        {
            get { return (bool) GetValue(ConditionProperty); }
            set { SetValue(ConditionProperty, value); }
        }

        private bool IsAnyAdornerOpened =>
            _openedAdorner.Any(x => GetGroup(x.AssociatedObject) == GetGroup(AssociatedObject));

        private ControlAdorner _adorner;

        public DataTemplate AdornerTemplate
        {
            get { return (DataTemplate) GetValue(AdornerTemplateProperty); }
            set { SetValue(AdornerTemplateProperty, value); }
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Loaded += OnLoaded;
            AssociatedObject.Unloaded += OnUnloaded;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            if (_openedAdorner.Contains(this))
            {
                _openedAdorner.Remove(this);
            }
        }

        private void OnConditionChanged()
        {
            if (Condition)
            {
                EnableToolTip();
            }
            else
            {
                DisableToolTip();
            }
        }

        private void DisableToolTip()
        {
            ToolTipService.SetIsEnabled(AssociatedObject, false);
        }

        private void TryEnableToolTip()
        {
            if (Condition)
            {
                EnableToolTip();
            }
        }

        private void EnableToolTip()
        {
            ToolTipService.SetPlacement(AssociatedObject, PlacementMode.Top);
            ToolTipService.SetIsEnabled(AssociatedObject, true);
            AssociatedObject.ToolTip = @"Hold ""LeftCtrl"" or click inside to see the full content";
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _adorner = new ControlAdorner(AssociatedObject, AdornerTemplate.LoadContent() as FrameworkElement,
                AssociatedObject?.DataContext, AdornerProperties.GetAdornerContext(this));
            _adorner.Margin = new Thickness(-6, 0, -6, 0);
            HideAdorner();
            Application.Current.MainWindow.PreviewMouseLeftButtonDown += OnPreviewMouseDown;
            Application.Current.MainWindow.PreviewMouseLeftButtonUp += OnPreviewMouseUp;
            Application.Current.MainWindow.PreviewMouseMove += OnPreviewMouseMove;
            if (MouseEvent == OverlookMouseBehavior.OnMouseOverAndCtrl)
            {
                Application.Current.MainWindow.PreviewKeyDown += OnPreviewKeyDown;
            }
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.PreviewMouseLeftButtonDown -= OnPreviewMouseDown;
            Application.Current.MainWindow.PreviewMouseLeftButtonUp -= OnPreviewMouseUp;
            Application.Current.MainWindow.PreviewMouseMove -= OnPreviewMouseMove;
            if (MouseEvent == OverlookMouseBehavior.OnMouseOverAndCtrl)
            {
                Application.Current.MainWindow.PreviewKeyDown -= OnPreviewKeyDown;
            }

            if (_openedAdorner.Contains(this))
            {
                _openedAdorner.Remove(this);
            }
        }

        private bool IsInBound(FrameworkElement reference)
        {
            var posFromAssociatedObject = Mouse.GetPosition(reference);
            return posFromAssociatedObject.X > 0
                   && posFromAssociatedObject.X < reference.ActualWidth
                   && posFromAssociatedObject.Y > 0
                   && posFromAssociatedObject.Y < reference.ActualHeight;
        }

        private void OnPreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!Condition)
            {
                return;
            }
            if (!IsInBound(_adorner) && !GetIsAdornerLocked(_adorner))
            {
                HideAdorner();
            }
            if (AssociatedObject.IsVisible
                && IsInBound(AssociatedObject)
                && !IsAnyAdornerOpened)
            {
                ShowAdorner();
                SetIsAdornerLocked(_adorner, true);
            }
        }

        private void OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!Condition)
            {
                return;
            }

            if (IsInBound(_adorner))
            {
                SetIsAdornerLocked(_adorner, true);
            }
            else
            {
                HideAdorner();
                SetIsAdornerLocked(_adorner, false);
            }

            if (AssociatedObject.IsVisible
                && IsInBound(AssociatedObject)
                && !IsAnyAdornerOpened)
            {
                ShowAdorner();
                SetIsAdornerLocked(_adorner, true);
            }
        }

        private void OnPreviewMouseMove(object sender, MouseEventArgs e)
        {
            HandleKeyDownOrMouseMove();
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            HandleKeyDownOrMouseMove();
        }

        private void HandleKeyDownOrMouseMove()
        {
            if (!Condition)
            {
                return;
            }

            if (AssociatedObject.IsVisible
                && IsInBound(AssociatedObject)
                && Keyboard.IsKeyDown(Key.LeftCtrl)
                && !IsAnyAdornerOpened)
            {
                ShowAdorner();
            }
            else
            {
                if (!IsInBound(_adorner) && (!GetIsAdornerLocked(_adorner) || Keyboard.IsKeyDown(Key.LeftCtrl)))
                {
                    HideAdorner();
                    SetIsAdornerLocked(_adorner, false);
                }
            }
        }

        private void HideAdorner()
        {
            _adorner.Hide();
            if (_openedAdorner.Contains(this))
            {
                _openedAdorner.Remove(this);
            }
            TryEnableToolTip();
        }

        private void ShowAdorner()
        {
            if (_adorner.Visibility != Visibility.Visible)
            {
                _adorner.Show();
                _openedAdorner.Add(this);
                DisableToolTip();
            }
        }
    }
}