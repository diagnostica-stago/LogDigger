using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LogDigger.Gui.Views.Controls.SequenceDiagram
{
    public interface IDiagram
    {
        IEnumerable<IDiagramLine> Lines { get; }
    }

    public interface IDiagramLine
    {
        string Name { get; }
    }

    public interface IDiagramItem
    {
        string Name { get; }
        string LinkToLine { get; }
    }


    public class SequenceDiagramControl : Canvas
    {
        private List<FrameworkElement> _lineHeaders;

        public static readonly DependencyProperty DiagramProperty = DependencyProperty.Register(
            "Diagram", typeof(IDiagram), typeof(SequenceDiagramControl), new PropertyMetadata(default(IDiagram), (o, args) => ((SequenceDiagramControl)o).OnDiagramChanged()));
        
        public static readonly DependencyProperty LineHeaderTemplateProperty = DependencyProperty.Register(
            "LineHeaderTemplate", typeof(DataTemplate), typeof(SequenceDiagramControl), new PropertyMetadata(default(DataTemplate)));

        public SequenceDiagramControl()
        {
            _lineHeaders = new List<FrameworkElement>();
        }

        public DataTemplate LineHeaderTemplate
        {
            get { return (DataTemplate) GetValue(LineHeaderTemplateProperty); }
            set { SetValue(LineHeaderTemplateProperty, value); }
        }

        public IDiagram Diagram
        {
            get { return (IDiagram) GetValue(DiagramProperty); }
            set { SetValue(DiagramProperty, value); }
        }

        private void OnDiagramChanged()
        {
            foreach (var diagramLine in Diagram.Lines)
            {
                var lineHeader = LineHeaderTemplate.LoadContent() as FrameworkElement;
                lineHeader.DataContext = diagramLine;
                _lineHeaders.Add(lineHeader);
                AddVisualChild(lineHeader);
            }
            InvalidateMeasure();
        }

        protected override int VisualChildrenCount
        {
            get { return _lineHeaders.Count; }
        }

        protected override Visual GetVisualChild(int index)
        {
            return _lineHeaders[index];
        }

        protected override Size MeasureOverride(Size constraint)
        {
            var lineHeadersCount = _lineHeaders.Count;
            foreach (var elem in _lineHeaders)
            {
                elem.Measure(new Size(constraint.Width / lineHeadersCount, constraint.Height));
            }
            return base.MeasureOverride(constraint);
        }

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            double x = 0;
            var lineHeadersCount = _lineHeaders.Count;
            foreach (var elem in _lineHeaders)
            {
                var width = arrangeSize.Width / lineHeadersCount;
                var rect = new Rect(new Point(x, 0), new Size(width, elem.DesiredSize.Height));
                elem.Arrange(rect);
                x += width;
            }
            return base.ArrangeOverride(arrangeSize);
        }
    }
}