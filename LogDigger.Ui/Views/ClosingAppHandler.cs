using System;
using System.Windows;
using LogDigger.Views;

namespace LogDigger.Services
{
    public class ClosingAppHandler : IClosingAppHandler
    {
        public event EventHandler<EventArgs> ClosingApp;

        public ClosingAppHandler()
        {
            App.Current.Exit += OnExit;
        }

        private void OnExit(object sender, ExitEventArgs e)
        {
            RaiseClosingApp();
        }

        protected virtual void RaiseClosingApp()
        {
            ClosingApp?.Invoke(this, EventArgs.Empty);
        }
    }
}