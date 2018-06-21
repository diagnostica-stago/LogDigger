using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using LogDigger.Gui.ViewModels.Core;
using LogDigger.Gui.ViewModels.LogEntries;
using MahApps.Metro.Controls;

namespace LogDigger.Gui.Views.Pages
{
    public partial class EntriesView
    {
        public EntriesView()
        {
            DataContextChanged += OnDataContextChanged;
            InitializeComponent();
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var allPageVm = e.NewValue as AEntriesPageVm;
            if (allPageVm != null)
            {
                allPageVm.SelectionChanged += OnSelectionChanged;
            }
        }

        private void OnSelectionChanged(object sender, SelectionChangingEventArgs<LogEntryVm> e)
        {
            // Auto scroll
            Dispatcher.BeginInvoke(new Action(() =>
            {
                xamlDataGrid.SelectedItem = e.SelectedItem;
                var scrollViewer = xamlDataGrid.FindChild<ScrollViewer>("DG_ScrollViewer");
                var ratio = e.SelectedIndex / (double) xamlDataGrid.Items.Count;
                var itemPosition = ratio * (scrollViewer.ScrollableHeight + scrollViewer.ViewportHeight);
                var currentPosition = scrollViewer.VerticalOffset;
                if (currentPosition < itemPosition && itemPosition < (currentPosition + scrollViewer.ViewportHeight))
                {
                    // no need to scroll
                }
                else if (currentPosition < itemPosition)
                {
                    scrollViewer.ScrollToVerticalOffset(itemPosition - scrollViewer.ViewportHeight + xamlDataGrid.RowHeight);
                }
                else
                {
                    scrollViewer.ScrollToVerticalOffset(itemPosition);
                }
            }));
        }

        private void OnDataGridRequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
        {
            e.Handled = true;
        }

        private void OnSelectedCellChanged(object sender, SelectedCellsChangedEventArgs e)
        {
        }
    }
}
