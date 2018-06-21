using System.Windows.Media;
using ReactiveUI;

namespace LogDigger.Gui.ViewModels.Core
{
    public interface IHighlightSearchItemVm : IReactiveObject
    {
        string Text { get; }
        Brush Brush { get; }
    }
}