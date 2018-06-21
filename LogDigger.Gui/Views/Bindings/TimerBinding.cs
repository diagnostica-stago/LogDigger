using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;

namespace LogDigger.Gui.Views.Bindings
{
    /// <summary>
    /// Represents a binding that automatically updates its target property.
    /// </summary>
    public sealed class TimerBinding : ABindingDecorator, IDisposable
    {
        private DispatcherTimer _timer;
        private bool _timerStoped;
        private BindingExpression _bindingExpression;

        /// <summary>
        /// Gets or sets the delay between each update.
        /// </summary>
        public int Delay { get; set; }

        /// <summary>
        /// Initialize an instance of <see cref="TimerBinding"/> class.
        /// </summary>
        public TimerBinding()
        {
            Delay = 1;
        }

        /// <summary>
        /// Initialize an instance of <see cref="TimerBinding"/> class from the specified path.
        /// </summary>
        /// <param name="path">The path of the target property.</param>
        public TimerBinding(string path)
        {
            Path = new PropertyPath(path);
        }

        /// <summary>
        /// This basic implementation just sets a binding on the targeted
        /// <see cref="DependencyObject"/> and returns the appropriate
        /// <see cref="BindingExpressionBase"/> instance.<br/>
        /// All this work is delegated to the decorated <see cref="Binding"/>
        /// instance.
        /// </summary>
        /// <returns>
        /// The object value to set on the property where the extension is applied.
        /// In case of a valid binding expression, this is a <see cref="BindingExpressionBase"/>
        /// instance.
        /// </returns>
        /// <param name="serviceProvider">Object that can provide services for the markup
        /// extension.</param>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            DependencyObject elem;
            DependencyProperty prop;
            if (TryGetTargetItems(serviceProvider, out elem, out prop))
            {
                var control = elem as FrameworkElement;
                if (control != null)
                {
                    control.Loaded += OnLoaded;
                    control.Unloaded += OnUnloaded;
                    _bindingExpression = BindingOperations.GetBindingExpression(elem, prop);
                }
            }

            return base.ProvideValue(serviceProvider);
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _timer = new DispatcherTimer(DispatcherPriority.DataBind) { Interval = TimeSpan.FromSeconds(Delay) };
            _timer.Tick += OnTick;
            _timerStoped = false;
            _timer.Start();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            if (_timer != null)
            {
                Dispose();
            }
        }

        private void OnTick(object sender, EventArgs e)
        {
            if (_bindingExpression != null && !_timerStoped)
            {
                var dependencyObject = _bindingExpression.Target;
                var dependencyProperty = _bindingExpression.TargetProperty;
                if (BindingOperations.IsDataBound(dependencyObject, dependencyProperty))
                {
                    Dispose();
                    return;
                }
                _bindingExpression.UpdateTarget();
            }
        }

        public void Dispose()
        {
            _timerStoped = true;
            if (_timer != null)
            {
                _timer.Stop();
                _timer = null;
                _bindingExpression = null;
            }
        }
    }
}