namespace LogDigger.Gui.ViewModels.Core
{
    public interface INavigator
    {
        void AddPage(IPage newPage);
        void ClosePage(IPage page);
    }
}