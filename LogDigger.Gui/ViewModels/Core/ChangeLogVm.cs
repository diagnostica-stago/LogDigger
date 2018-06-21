using System.IO;
using System.Threading.Tasks;

namespace LogDigger.Gui.ViewModels.Core
{
    public class ChangeLogVm : AModalVm
    {
        public ChangeLogVm()
        {
            Task.Run(() => Init());
        }

        private void Init()
        {
            Log = File.ReadAllText(Path.Combine(AppUtils.AppLocation, "changelog.md"));
        }

        public string Log
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }
    }
}