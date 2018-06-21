using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Interactivity;
using System.Windows.Media;
using ICSharpCode.AvalonEdit;
using LogDigger.Gui.ViewModels.Core;
using LogDigger.Gui.Views.Core;
using ReactiveUI;

namespace LogDigger.Gui.Views.Behaviors
{
    public sealed class AvalonHighlightBehavior : Behavior<TextEditor>
    {
        public static readonly DependencyProperty HighlightedTextProperty = DependencyProperty.Register(
            "HighlightedText", typeof(string), typeof(AvalonHighlightBehavior), new PropertyMetadata(default(string), OnHighlightedTextChanged));

        public static readonly DependencyProperty BrushProperty = DependencyProperty.Register(
            "Brush", typeof(Brush), typeof(AvalonHighlightBehavior), new PropertyMetadata(default(Brush)));

        public static readonly DependencyProperty HighlightSearchesProperty = DependencyProperty.Register(
            "HighlightSearches", typeof(IEnumerable<IHighlightSearchItemVm>), typeof(AvalonHighlightBehavior), new PropertyMetadata(default(IEnumerable<IHighlightSearchItemVm>), (o, args) => ((AvalonHighlightBehavior)o).OnHighlightSearchesChanged()));
        
        public IEnumerable<IHighlightSearchItemVm> HighlightSearches
        {
            get { return (IEnumerable<IHighlightSearchItemVm>) GetValue(HighlightSearchesProperty); }
            set { SetValue(HighlightSearchesProperty, value); }
        }

        public Brush Brush
        {
            get { return (Brush) GetValue(BrushProperty); }
            set { SetValue(BrushProperty, value); }
        }

        public string HighlightedText
        {
            get { return (string)GetValue(HighlightedTextProperty); }
            set { SetValue(HighlightedTextProperty, value); }
        }
        
        protected override void OnAttached()
        {
            base.OnAttached();
            if (AssociatedObject != null)
            {
                AssociatedObject.TextChanged += AssociatedObjectOnTextChanged;
                AssociatedObject.Loaded += AssociatedObjectLoaded;
            }
        }

        private void AssociatedObjectLoaded(object sender, RoutedEventArgs e)
        {
            OnHighlightSearchesChanged();
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            if (AssociatedObject != null)
            {
                AssociatedObject.TextChanged -= AssociatedObjectOnTextChanged;
            }
        }

        private void AssociatedObjectOnTextChanged(object sender, EventArgs eventArgs)
        {
            InitTransformers();
        }
        
        private static void OnHighlightedTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var behavior = d as AvalonHighlightBehavior;
            if (behavior.AssociatedObject != null)
            {
                var editor = behavior.AssociatedObject as TextEditor;
                var lineTransformers = editor.TextArea.TextView.LineTransformers;
                // replace any transformer using this brush
                var transformer = lineTransformers.OfType<ColorizeAvalonEdit>().FirstOrDefault(cae => Equals(cae.TargetBrush, behavior.Brush));
                if (transformer != null)
                {
                    lineTransformers.Remove(transformer);
                }
                if (!string.IsNullOrEmpty((string)e.NewValue))
                {
                    lineTransformers.Add(new ColorizeAvalonEdit(e.NewValue.ToString(), behavior.Brush));
                }
            }
        }

        private void OnHighlightSearchesChanged()
        {
            if (AssociatedObject != null && HighlightSearches != null)
            {
                foreach (var hs in HighlightSearches)
                {
                    hs.WhenAnyValue(x => x.Text).Select(x => hs).Subscribe(OnTextChanged);
                }
                InitTransformers();
            }
        }

        private void InitTransformers()
        {
            var editor = AssociatedObject as TextEditor;
            var lineTransformers = editor.TextArea.TextView.LineTransformers;
            // replace any transformer using this brush
            var transformers = lineTransformers.OfType<ColorizeAvalonEdit>().ToList();
            foreach (var transformer in transformers)
            {
                lineTransformers.Remove(transformer);
            }
            if (HighlightSearches != null)
            {
                foreach (var hs in HighlightSearches)
                {
                    if (!string.IsNullOrEmpty(hs.Text))
                    {
                        lineTransformers.Add(new ColorizeAvalonEdit(hs.Text, hs.Brush));
                    }
                }
            }
        }

        private void OnTextChanged(IHighlightSearchItemVm hs)
        {
            var lineTransformers = AssociatedObject.TextArea.TextView.LineTransformers;
            var transformer = lineTransformers.OfType<ColorizeAvalonEdit>().FirstOrDefault(cae => Equals(cae.TargetBrush, hs.Brush));
            if (transformer != null)
            {
                lineTransformers.Remove(transformer);
            }
            if (!string.IsNullOrEmpty(hs.Text))
            {
                lineTransformers.Add(new ColorizeAvalonEdit(hs.Text, hs.Brush));
            }
        }
    }
}
