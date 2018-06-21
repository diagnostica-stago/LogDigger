using System.Runtime.CompilerServices;
using LogDigger.Gui.Properties;
using LogDigger.Gui.ViewModels.Core;
using ReactiveUI;

namespace LogDigger.Gui.ViewModels.Pages.ViewProfiles
{
    public class ViewCustomization : AViewModel, IViewProfile
    {
        private IViewProfile _currentProfile;

        public ViewCustomization()
        {
            _currentProfile = ViewProfileFactory.CreateProfile();
            _currentProfile?.Init();
        }
        
        private void SetSetting<T>(string settingName, T value, [CallerMemberName] string propertyName = null)
        {
            _currentProfile = null;
            UserSettings.Default.ViewProfile = ViewProfile.Custom.ToString();
            UserSettings.Default[settingName] = value;
            this.RaisePropertyChanged(propertyName);
            this.RaisePropertyChanged(nameof(IsAdvancedView));
            this.RaisePropertyChanged(nameof(IsSimpleView));
        }

        public bool IsIdVisible
        {
            get => _currentProfile?.IsIdVisible ?? UserSettings.Default.IdColumnVisible;
            set => SetSetting(nameof(UserSettings.IdColumnVisible), value);
        }

        public bool ShowSummaryContent
        {
            get => _currentProfile?.ShowSummaryContent ?? UserSettings.Default.ShowSummaryContent;
            set => SetSetting(nameof(UserSettings.ShowSummaryContent), value);
        }

        public bool IsContentVisible
        {
            get => _currentProfile?.IsContentVisible ?? UserSettings.Default.ContentColumnVisible;
            set => SetSetting(nameof(UserSettings.ContentColumnVisible), value);
        }

        public bool IsThreadVisible
        {
            get => _currentProfile?.IsThreadVisible ?? UserSettings.Default.ThreadColumnVisible;
            set => SetSetting(nameof(UserSettings.ThreadColumnVisible), value);
        }

        public bool IsLevelVisible
        {
            get => _currentProfile?.IsLevelVisible ?? UserSettings.Default.LevelColumnVisible;
            set => SetSetting(nameof(UserSettings.LevelColumnVisible), value);
        }

        public bool IsTypeVisible
        {
            get => _currentProfile?.IsTypeVisible ?? UserSettings.Default.TypeColumnVisible;
            set => SetSetting(nameof(UserSettings.TypeColumnVisible), value);
        }

        public void Init()
        {
        }

        public bool IsSimpleView
        {
            get => _currentProfile is SimpleViewProfile;
            set
            {
                if (value)
                {
                    _currentProfile = new SimpleViewProfile();
                    _currentProfile.Init();
                    UserSettings.Default.ViewProfile = ViewProfile.Simple.ToString();
                    this.RaisePropertyChanged(string.Empty);

                }
            }
        }

        public bool IsAdvancedView
        {
            get => _currentProfile is AdvancedViewProfile;
            set
            {
                if (value)
                {
                    _currentProfile = new AdvancedViewProfile(); ;
                    _currentProfile.Init();
                    UserSettings.Default.ViewProfile = ViewProfile.Advanced.ToString();
                    this.RaisePropertyChanged(string.Empty);
                }
            }
        }
    }
}