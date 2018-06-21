using System;
using System.Reactive.Concurrency;
using System.Windows.Threading;

namespace LogDigger.Gui.ViewModels.Core
{
    public class GuiScheduler : IGuiScheduler
    {
        public IDisposable Schedule<TState>(TState state, Func<IScheduler, TState, IDisposable> action)
        {
            IDisposable disposable = null;
            Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Normal, new Action(() => disposable = action(this, state)));
            return disposable;
        }

        public IDisposable Schedule<TState>(TState state, TimeSpan dueTime, Func<IScheduler, TState, IDisposable> action)
        {
            IDisposable disposable = null;
            Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Normal, new Action(() => disposable = action(this, state)));
            return disposable;
        }

        public IDisposable Schedule<TState>(TState state, DateTimeOffset dueTime, Func<IScheduler, TState, IDisposable> action)
        {
            IDisposable disposable = null;
            Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Normal, new Action(() => disposable = action(this, state)));
            return disposable;
        }

        public DateTimeOffset Now => DateTimeOffset.Now;
    }
}