using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Media;

namespace LogDigger.Gui.Views.Behaviors
{
    public class ClosePopupBehavior : Behavior<FrameworkElement>
    {
        private Popup _popup;
        private List<object> _hitResultsList;

        protected override void OnAttached()
        {
            base.OnAttached();
            _popup = AssociatedObject.Parent as Popup;
            if (_popup != null)
            {
                _popup.Opened += OnPopupOpened;
                _popup.Closed += OnPopupClosed;
            }
        }

        private void OnPopupClosed(object sender, EventArgs e)
        {
            _popup.PreviewMouseMove -= OnRendering;
        }

        private void OnPopupOpened(object sender, EventArgs e)
        {
            _popup.PreviewMouseMove += OnRendering;
        }

        private void OnRendering(object sender, EventArgs e)
        {
            _hitResultsList = new List<object>();
            var root = GetRoot(AssociatedObject) as Visual;
            var pt = Mouse.GetPosition(root as IInputElement);
            VisualTreeHelper.HitTest(root, null, MyHitTestResult, new PointHitTestParameters(pt));

            // Perform actions on the hit test results list.
            if (_hitResultsList == null || _hitResultsList.Count == 0)
            {
                _popup.SetCurrentValue(Popup.IsOpenProperty, false);
            }
        }

        public DependencyObject GetRoot(DependencyObject element)
        {
            var parent = VisualTreeHelper.GetParent(element);
            if (parent == null)
            {
                return element;
            }
            return GetRoot(parent);
        }

        // Return the result of the hit test to the callback.
        public HitTestResultBehavior MyHitTestResult(HitTestResult result)
        {
            // Add the hit test result to the list that will be processed after the enumeration.
            _hitResultsList.Add(result.VisualHit);

            // Set the behavior to return visuals at all z-order levels.
            return HitTestResultBehavior.Continue;
        }
    }
}