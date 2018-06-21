using LogDigger.Gui.ViewModels.Core;

namespace LogDigger.Gui.ViewModels.Pages.Replay
{
    public class ContentDialogVm : AModalVm
    {
        public string Content { get; }

        public ContentDialogVm(string content)
        {
            Content = content;
        }
    }
}