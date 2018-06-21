using System;
using System.Windows;
using ICSharpCode.AvalonEdit;

namespace LogDigger.Gui.Views.Controls
{
    public class TextEditorEx : TextEditor
    {
        private int _oldSelectionStart;
        private int _oldSelectionLength;

        public TextEditorEx()
        {
            Loaded += OnThisLoaded;
            Unloaded += OnThisUnloaded;
            this.Initialized += OnThisInitialized;
            this.DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
        }

        private void OnThisInitialized(object sender, EventArgs e)
        {
        }

        private void OnThisUnloaded(object sender, RoutedEventArgs e)
        {
            _oldSelectionStart = SelectionStart;
            _oldSelectionLength = SelectionLength;
        }

        private void OnThisLoaded(object sender, RoutedEventArgs e)
        {
            this.Select(_oldSelectionStart, _oldSelectionLength);
        }
    }
}