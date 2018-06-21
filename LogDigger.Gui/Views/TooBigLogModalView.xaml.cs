using LogDigger.Gui.ViewModels.Core;
using LogDigger.Gui.ViewModels.MainWindow;

namespace LogDigger.Gui.Views
{
    /// <summary>
    /// Interaction logic for TooBigLogModalView.xaml
    /// </summary>
    [InjectView(typeof(TooBigLogModalVm))]
    public partial class TooBigLogModalView
    {
        public TooBigLogModalView()
        {
            InitializeComponent();
        }
    }
}
