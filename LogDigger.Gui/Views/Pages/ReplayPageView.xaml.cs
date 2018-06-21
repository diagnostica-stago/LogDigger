using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace LogDigger.Gui.Views.Pages
{
    /// <summary>
    /// Interaction logic for ReplayPageView.xaml
    /// </summary>
    public partial class ReplayPageView
    {
        public ReplayPageView()
        {
            InitializeComponent();
        }

        private void OnDataGridSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selector = sender as Selector;
            var dataGrid = selector as DataGrid;
            if (dataGrid != null && selector.SelectedItem != null && dataGrid.SelectedIndex >= 0)
            {
                dataGrid.ScrollIntoView(selector.SelectedItem);
            }
        }
    }
}
