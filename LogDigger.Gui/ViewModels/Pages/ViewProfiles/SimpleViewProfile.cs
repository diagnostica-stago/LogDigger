namespace LogDigger.Gui.ViewModels.Pages.ViewProfiles
{
    public class SimpleViewProfile : IViewProfile
    {
        public bool IsIdVisible
        {
            get { return false; }
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
            get { return false; }
        }

        public bool IsTypeVisible
        {
            get { return true; }
        }

        public bool ShowSummaryContent
        {
            get { return true; }
        }

        public void Init()
        {
        }
    }
}