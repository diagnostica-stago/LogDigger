using LogDigger.Gui.ViewModels.Core;
using LogDigger.Gui.ViewModels.Pages.Summaries;

namespace LogDigger.Gui.Views.Pages
{
    /// <summary>
    /// Interaction logic for TemplatesEditionPageView.xaml
    /// </summary>
    [InjectView(typeof(TemplatesEditionPageVm))]
    public partial class TemplatesEditionPageView
    {
        public TemplatesEditionPageView()
        {
            InitializeComponent();
        }
    }
}
