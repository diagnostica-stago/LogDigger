using LogDigger.Gui.ViewModels.Core;
using LogDigger.Gui.ViewModels.Settings;

namespace LogDigger.Gui.Views
{
    /// <summary>
    /// Interaction logic for SettingsView.xaml
    /// </summary>
    [InjectView(typeof(SettingsVm))]
    public partial class SettingsView
    {
        public SettingsView()
        {
            InitializeComponent();
        }
    }
}
