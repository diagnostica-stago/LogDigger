using System;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;

namespace LogDigger.Gui.Views.Core
{
    public class ColorizeAvalonEdit : DocumentColorizingTransformer
    {
        private readonly string _text;
        private readonly Brush _brush;

        public ColorizeAvalonEdit(string text, Brush brush)
        {
            _text = text;
            _brush = brush;
        }

        public Brush TargetBrush
        {
            get { return _brush; }
        }

        protected override void ColorizeLine(DocumentLine line)
        {
            if (string.IsNullOrEmpty(_text))
            {
                return;
            }
            int lineStartOffset = line.Offset;
            string text = CurrentContext.Document.GetText(line);
            int start = 0;
            int index;
            int endOffset = _text.Length;
            while ((index = text.IndexOf(_text, start, StringComparison.OrdinalIgnoreCase)) >= 0)
            {
                base.ChangeLinePart(
                    lineStartOffset + index, // startOffset
                    lineStartOffset + index + endOffset, // endOffset
                    (VisualLineElement element) => {
                        // This lambda gets called once for every VisualLineElement
                        // between the specified offsets.
                        Typeface tf = element.TextRunProperties.Typeface;
                        // Replace the typeface with a modified version of
                        // the same typeface
                        element.TextRunProperties.SetBackgroundBrush(_brush);
                    });
                start = index + 1; // search for next occurrence
            }
        }
    }
}
