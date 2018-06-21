using LogDigger.Gui.ViewModels.Core;
using LogDigger.Gui.ViewModels.Pages.Replay;

namespace LogDigger.Gui.Views
{
    /// <summary>
    /// Interaction logic for ContentDialogView.xaml
    /// </summary>
    [InjectView(typeof(ContentDialogVm))]
    public partial class ContentDialogView
    {
        public ContentDialogView()
        {
            InitializeComponent();
        }
    }
}
