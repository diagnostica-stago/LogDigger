using System.Collections.ObjectModel;
using System.Windows.Input;
using LogDigger.Business.Models;
using LogDigger.Gui.ViewModels.Columns;
using ReactiveUI;

namespace LogDigger.Gui.ViewModels.LogStructure
{
    public interface ILogStructureVm
    {
        ReactiveList<IColumnDescriptionVm> Columns { get; }
        ICommand ParseCommand { get; set; }
        ReactiveList<LogFilePreviewVm> Files { get; }
        LogFormat Format { get; }
        ILogParser Parser { get; set; }
        bool Editing { get; set; }
    }
}