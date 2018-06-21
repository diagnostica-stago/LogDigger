using System;
using System.ComponentModel;
using System.Reactive.Linq;

namespace LogDigger.Gui.ViewModels.Core
{
    public static class ViewModelExtensions
    {
        public static IObservable<PropertyChangedEventArgs> WhenAnyPropertyChanged(this INotifyPropertyChanged inpc)
        {
            return Observable
                .FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>(
                    h => inpc.PropertyChanged += h,
                    h => inpc.PropertyChanged -= h)
                .Select(x => x.EventArgs);
        }
    }
}