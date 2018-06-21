using System.Collections.ObjectModel;
using LogDigger.Gui.ViewModels.Core;

namespace LogDigger.Gui.ViewModels.Pages
{
    public interface IHasHighlights<TItem>
    {
        ObservableCollection<HighlightSearchItemVm<TItem>> HighlightSearches { get; }
    }
}