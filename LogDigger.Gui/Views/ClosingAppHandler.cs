using System;
using System.Windows;
using LogDigger.Business.Services;

namespace LogDigger.Gui.Views
{
    public class ClosingAppHandler : IClosingAppHandler
    {
        public event EventHandler<EventArgs> ClosingApp;

        public ClosingAppHandler()
        {
            Application.Current.Exit += OnExit;
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