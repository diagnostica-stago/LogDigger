using System.Collections.Generic;
using System.Threading.Tasks;
using LogDigger.Business.Models;

namespace LogDigger.Gui.ViewModels.Core
{
    public interface IPage : IActivable
    {
        string Title { get; }

        IReadOnlyList<LogFile> FilterFiles(IReadOnlyList<LogFile> files);
        
        Task Reload(IReadOnlyList<LogFile> files);

        Task ReloadAll(IReadOnlyList<LogFile> files);
    }
}