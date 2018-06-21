using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LogDigger.Gui.Views.Controls
{
    public class ScrollViewerEx : ScrollViewer
    {
        public bool HandleMouseWheel { get; set; } = true;

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            if (!HandleMouseWheel)
            {
                return;
            }
            if (ComputedHorizontalScrollBarVisibility == Visibility.Collapsed &&
                ComputedVerticalScrollBarVisibility == Visibility.Collapsed)
            {
                return;
            }

            base.OnMouseWheel(e);
        }
    }
}