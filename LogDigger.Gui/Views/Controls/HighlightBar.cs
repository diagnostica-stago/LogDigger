using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LogDigger.Gui.Views.Controls
{
    public class HighlightBar : Control
    {
        private List<double> _positions;

        public static readonly DependencyProperty HighlightsProperty = DependencyProperty.Register(
            "Highlights", typeof(IEnumerable<double>), typeof(HighlightBar), new PropertyMetadata(default(IEnumerable<double>), (o, args) => ((HighlightBar)o).OnHighlightPointsChanged(args)));
        
        public HighlightBar()
        {
            IsHitTestVisible = false;
            _positions = new List<double>();
        }

        public IEnumerable<double> Highlights
        {
            get { return (IEnumerable<double>) GetValue(HighlightsProperty); }
            set { SetValue(HighlightsProperty, value); }
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            SetPositions(new List<double>());
            var hlCopy = Highlights?.ToList();
            Task.Run(() => UpdatePositions(hlCopy));
            base.OnRenderSizeChanged(sizeInfo);
        }

        private void OnHighlightPointsChanged(DependencyPropertyChangedEventArgs args)
        {
            var hlCopy = Highlights?.ToList();
            Task.Run(() => UpdatePositions(hlCopy));
        }

        private void UpdatePositions(IEnumerable<double> newPositions)
        {
            var result = new List<double>();

            if (newPositions != null)
            {
                int previousPos = 0;
                foreach (var highlight in newPositions)
                {
                    var posY = (int)Math.Floor(highlight * ActualHeight);
                    if (posY == previousPos || posY == previousPos + 1)
                    {
                        continue;
                    }
                    previousPos = posY;
                    result.Add(posY);
                }
            }

            SetPositions(result);
        }

        private void SetPositions(List<double> newPositions)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                _positions = newPositions;
                InvalidateVisual();
            }));
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            var pen = new Pen();
            foreach (var position in _positions)
            {
                var location = new Point(0, position);
                drawingContext.DrawRectangle(Foreground, pen, new Rect(location, new Size(ActualWidth, 2)));
            }
            base.OnRender(drawingContext);
        }
    }
}