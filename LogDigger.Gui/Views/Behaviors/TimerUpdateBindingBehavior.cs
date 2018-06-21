using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Interactivity;
using System.Windows.Threading;

namespace LogDigger.Gui.Views.Behaviors
{
    public class TimerUpdateBindingBehavior : Behavior<DependencyObject>, IDisposable
    {
        private DispatcherTimer _timer;
        private bool _timerStoped;

        public DependencyProperty DependencyProperty { get; set; }
        public int Delay { get; set; }

        protected override void OnAttached()
        {
            base.OnAttached();
            _timer = new DispatcherTimer(DispatcherPriority.DataBind) { Interval = TimeSpan.FromMilliseconds(Delay) };
            _timer.Tick += OnTick;
            _timerStoped = false;
            _timer.Start();
        }

        private void OnTick(object sender, EventArgs e)
        {
            if (!_timerStoped)
            {
                var bindingExpression = BindingOperations.GetBindingExpression(AssociatedObject, DependencyProperty);
                if (bindingExpression != null)
                {
                    bindingExpression?.UpdateTarget();
                }
                else
                {
                    var mbe = BindingOperations.GetMultiBindingExpression(AssociatedObject, DependencyProperty);
                    mbe?.UpdateTarget();
                }
            }
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            if (_timer != null)
            {
                Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _timerStoped = true;
                if (_timer != null)
                {
                    _timer.Stop();
                    _timer = null;
                }
            }
        }
    }
}