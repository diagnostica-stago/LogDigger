using System;

namespace LogDigger.Gui.ViewModels.Core
{
    public class SelectionChangingEventArgs<TItem> : EventArgs
    {
        public SelectionChangingEventArgs(int selectedIndex, TItem selectedItem)
        {
            SelectedIndex = selectedIndex;
            SelectedItem = selectedItem;
        }

        public TItem SelectedItem { get; }

        public int SelectedIndex { get; }
    }
}