using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using ReactiveUI;

namespace LogDigger.Gui.ViewModels.Core
{
    /// <summary>
    /// Base class for all localized ViewModel classes in the application.
    /// It provides support for property change notifications 
    /// and has a DisplayName property.  This class is abstract.
    /// </summary>
    public abstract class AViewModel : ReactiveObject
    {
        private ConcurrentDictionary<string, object> _properties;

        private string _displayName;
        private bool _disposed = false;
        private bool _isDisposed;
        /// <summary>
        /// Returns true if the object is disposed
        /// </summary>
        protected bool IsDisposed
        {
            get { return _isDisposed; }
            private set { UpdateCurrentProperty(ref _isDisposed, value); }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Returns the user-friendly name of this object.
        /// Child classes can set this property to a new value,
        /// or override it to determine the value on-demand.
        /// </summary>
        public string DisplayName
        {
            get { return _displayName; }
            protected set { UpdateCurrentProperty(ref _displayName, value); }
        }

        public ConcurrentDictionary<string, object> Properties
        {
            get
            {
                if (_properties == null)
                {
                    _properties = new ConcurrentDictionary<string, object>();
                }
                return _properties;
            }
        }

        protected Task<TResult> Run<TResult>(Func<TResult> func)
        {
            return Task.Run(func);
        }

        protected T GetProperty<T>([CallerMemberName] string expressionName = "")
        {
            return (T)Properties.GetOrAdd(expressionName, x => default(T));
        }

        protected virtual bool SetProperty<T>(T value, Action<T> action = null, [CallerMemberName] string expressionName = "")
        {
            Properties.AddOrUpdate(expressionName, (key) => value, (key, oldValue) => value);
            action?.Invoke(value);
            this.RaisePropertyChanged(expressionName);
            return true;
        }

        /// <summary>
        /// Update value and raise PropertyChanged if newValue != refPRoperty Value        
        /// </summary>
        /// <typeparam name="T">Type of property</typeparam>
        /// <param name="refProperty">The property by reference</param>
        /// <param name="newValue">The new value</param>
        /// <param name="propertyName">name of property : Do not set this parameter</param>
        /// <returns>True if the property has changed, false if not.</returns>
        [SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", Justification = "We need a ref to factorize this code")]
        [SuppressMessage("Microsoft.Design", "CA1026", Justification = "Optional parameters needed for CallerMemberinfo")]
        protected bool UpdateCurrentProperty<T>(ref T refProperty,  T newValue, [CallerMemberName] string propertyName = "")
        {
            if (Equals(refProperty, newValue))
            {
                return false;
            }
            refProperty = newValue;
            this.RaisePropertyChanged(propertyName);
            return true;
        }

        // Dispose() calls Dispose(true)

        // NOTE: Leave out the finalizer altogether if this class doesn't 
        // own unmanaged resources itself, but leave the other methods
        // exactly as they are. 
        ~AViewModel()
        {
            // Finalizer calls Dispose(false)
            Dispose(false);
        }

        // The bulk of the clean-up code is implemented in Dispose(bool)
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
            }

            _disposed = true;
        }

        protected DispatcherOperation Dispatch(Action action)
        {
            return Application.Current.Dispatcher.BeginInvoke(new Action(action));
        }

        public void Cleanup()
        {
            IsDisposed = true;
        }
    }
}