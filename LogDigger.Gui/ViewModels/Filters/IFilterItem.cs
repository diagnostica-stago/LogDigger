using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ReactiveUI;

namespace LogDigger.Gui.ViewModels.Filters
{
    public interface IFilterItem<T> : IReactiveObject
    {
        Task InitFilter(IEnumerable<T> list);
        IEnumerable<T> Filter(IEnumerable<T> list);
        IObservable<object> WhenFilterChanged();
    }
}