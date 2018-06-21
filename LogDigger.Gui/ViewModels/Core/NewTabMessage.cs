namespace LogDigger.Gui.ViewModels.Core
{
    public class NewTabMessage
    {
        public NewTabMessage(object newTab)
        {
            NewTab = newTab;
        }

        public object NewTab { get; private set; }
    }
}