using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Interactivity;
using ICSharpCode.AvalonEdit;

namespace LogDigger.Gui.Views.Behaviors
{
    public sealed class AvalonEditBehaviour : Behavior<TextEditor>
    {
        public static readonly DependencyProperty GiveMeTheTextProperty =
            DependencyProperty.Register("GiveMeTheText", typeof(string), typeof(AvalonEditBehaviour),
                new FrameworkPropertyMetadata(default(string), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, PropertyChangedCallback));

        public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register(
            "IsReadOnly", typeof(bool), typeof(AvalonEditBehaviour), new PropertyMetadata(true));

        private bool _updatingSource;

        public bool IsReadOnly
        {
            get { return (bool) GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }

        public string GiveMeTheText
        {
            get { return (string)GetValue(GiveMeTheTextProperty); }
            set { SetValue(GiveMeTheTextProperty, value); }
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            if (AssociatedObject != null)
            {
                AssociatedObject.Loaded += OnAssociatedObjectLoaded;
                AssociatedObject.LostFocus += OnLostFocus;
                AssociatedObject.TextChanged += AssociatedObjectOnTextChanged;
            }
        }

        private void OnLostFocus(object sender, RoutedEventArgs e)
        {
            AssociatedObject.Select(0, 0);
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            if (AssociatedObject != null)
            {
                AssociatedObject.Loaded -= OnAssociatedObjectLoaded;
                AssociatedObject.LostFocus -= OnLostFocus;
                AssociatedObject.TextChanged -= AssociatedObjectOnTextChanged;
            }
        }

        private void OnAssociatedObjectLoaded(object sender, RoutedEventArgs e)
        {
            var editor = AssociatedObject;
            var oldSelectionStart = editor.SelectionStart;
            var oldSelectionLength = editor.SelectionLength;
            var caretOffset = editor.CaretOffset;
            editor.Document.Text = GiveMeTheText ?? string.Empty;
            editor.CaretOffset = caretOffset;
            editor.Select(oldSelectionStart, oldSelectionLength);
        }

        private void AssociatedObjectOnTextChanged(object sender, EventArgs eventArgs)
        {
            var textEditor = sender as TextEditor;
            var be = BindingOperations.GetBinding(this, GiveMeTheTextProperty);
            if (!IsReadOnly && textEditor?.Document != null)
            {
                _updatingSource = true;
                SetCurrentValue(GiveMeTheTextProperty, textEditor.Document.Text);
                _updatingSource = false;
            }
        }

        private static void PropertyChangedCallback(
            DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var behavior = dependencyObject as AvalonEditBehaviour;
            if (behavior._updatingSource)
            {
                return;
            }
            var editor = behavior?.AssociatedObject;
            if (editor?.Document != null)
            {
                var caretOffset = editor.CaretOffset;
                editor.Document.Text = dependencyPropertyChangedEventArgs.NewValue?.ToString() ?? string.Empty;
                if (caretOffset < editor.Document.Text.Length)
                {
                    editor.CaretOffset = caretOffset;
                }
            }
        }
    }
}