namespace LogDigger.Gui.ViewModels.Core
{
    public class OpenModalMessage
    {
        public OpenModalMessage(AModalVm modal)
        {
            Modal = modal;
        }

        public AModalVm Modal { get; }
    }
}