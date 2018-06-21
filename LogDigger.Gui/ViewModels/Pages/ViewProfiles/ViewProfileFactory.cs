using System;
using LogDigger.Gui.Properties;

namespace LogDigger.Gui.ViewModels.Pages.ViewProfiles
{
    public class ViewProfileFactory
    {
        public static IViewProfile CreateProfile()
        {
            var setting = UserSettings.Default.ViewProfile;
            ViewProfile result;
            if (Enum.TryParse(setting, true, out result))
            {
                switch (result)
                {
                    case ViewProfile.Custom:
                        return null;
                    case ViewProfile.Simple:
                        return new SimpleViewProfile();
                    case ViewProfile.Advanced:
                        return new AdvancedViewProfile();
                }
            }
            return null;
        }
    }
}