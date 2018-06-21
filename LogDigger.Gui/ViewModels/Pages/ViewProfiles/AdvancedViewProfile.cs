namespace LogDigger.Gui.ViewModels.Pages.ViewProfiles
{
    public class AdvancedViewProfile : IViewProfile
    {
        public bool IsIdVisible
        {
            get { return true; }
        }

        public bool IsContentVisible
        {
            get { return true; }
        }

        public bool IsThreadVisible
        {
            get { return false; }
        }

        public bool IsLevelVisible
        {
            get { return true; }
        }

        public bool IsTypeVisible
        {
            get { return false; }
        }

        public bool ShowSummaryContent
        {
            get { return false; }
        }

        public void Init()
        {
        }
    }
}