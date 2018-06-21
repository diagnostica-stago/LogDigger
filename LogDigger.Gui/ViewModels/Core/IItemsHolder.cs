using System;
using System.Collections.ObjectModel;

namespace LogDigger.Gui.ViewModels.Core
{
    public interface IItemsHolder<TItem> 
    {
        event EventHandler<SelectionChangingEventArgs<TItem>> SelectionChanged;
        bool IsLoading { get; }
        ObservableCollection<TItem> Items { get; }
        void SetSelectedItem(int index);
    }
}