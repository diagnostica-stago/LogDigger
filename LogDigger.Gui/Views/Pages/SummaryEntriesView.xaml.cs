using System;
using System.Windows;
using System.Windows.Controls;
using LogDigger.Gui.ViewModels.Core;
using LogDigger.Gui.ViewModels.Pages.Summaries;
using MahApps.Metro.Controls;

namespace LogDigger.Gui.Views.Pages
{
    /// <summary>
    /// Interaction logic for SummaryEntriesView.xaml
    /// </summary>
    [InjectView(typeof(SummaryEntriesPageVm))]
    public partial class SummaryEntriesView
    {
        public SummaryEntriesView()
        {
            DataContextChanged += OnDataContextChanged;
            InitializeComponent();
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var page = e.NewValue as IItemsHolder<ALogSummaryVm>;
            if (page != null)
            {
                page.SelectionChanged += OnSelectionChanged;
            }
        }

        private void OnSelectionChanged(object sender, SelectionChangingEventArgs<ALogSummaryVm> e)
        {
            // Auto scroll
            Dispatcher.BeginInvoke(new Action(() =>
            {
                xamlDataGrid.SelectedItem = e.SelectedItem;
                var scrollViewer = xamlDataGrid.FindChild<ScrollViewer>("DG_ScrollViewer");
                var itemsCount = xamlDataGrid.Items.Count;
                if (itemsCount > 0)
                {
                    var ratio = e.SelectedIndex / (double) itemsCount;
                    var itemPosition = ratio * (scrollViewer.ScrollableHeight + scrollViewer.ViewportHeight);
                    var currentPosition = scrollViewer.VerticalOffset;
                    if (currentPosition < itemPosition &&
                        itemPosition < (currentPosition + scrollViewer.ViewportHeight))
                    {
                        // no need to scroll
                    }
                    else if (currentPosition < itemPosition)
                    {
                        scrollViewer.ScrollToVerticalOffset(
                            itemPosition - scrollViewer.ViewportHeight + xamlDataGrid.RowHeight);
                    }
                    else
                    {
                        scrollViewer.ScrollToVerticalOffset(itemPosition);
                    }
                }
            }));
        }
    }
}
