using LogDigger.Gui.ViewModels.Core;
using LogDigger.Gui.ViewModels.Pages.History;

namespace LogDigger.Gui.Views.Pages
{
    [InjectView(typeof(EntitiesHistoryPageVm))]
    public partial class EntitiesHistoryPageView
    {
        public EntitiesHistoryPageView()
        {
            InitializeComponent();
        }
    }
}
