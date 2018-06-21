using LogDigger.Gui.ViewModels.Core;

namespace LogDigger.Gui.Views
{
    /// <summary>
    /// Interaction logic for ChangeLogView.xaml
    /// </summary>
    [InjectView(typeof(ChangeLogVm))]
    public partial class ChangeLogView
    {
        public ChangeLogView()
        {
            InitializeComponent();
        }
    }
}
