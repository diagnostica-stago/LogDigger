namespace LogDigger.Gui.ViewModels.Pages.ViewProfiles
{
    public interface IViewProfile
    {
        bool IsIdVisible { get; }

        bool IsContentVisible { get; }

        bool IsThreadVisible { get; }

        bool IsLevelVisible { get; }

        bool IsTypeVisible { get; }

        bool ShowSummaryContent { get; }

        void Init();
    }
}