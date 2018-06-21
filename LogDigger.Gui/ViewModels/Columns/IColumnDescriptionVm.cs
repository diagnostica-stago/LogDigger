using System.Collections.Generic;
using LogDigger.Business.Models;
using ReactiveUI;

namespace LogDigger.Gui.ViewModels.Columns
{
    public interface IColumnDescriptionVm : IReactiveObject
    {
        string Width { get; }
        string Name { get; }
        ACellTemplateVm Template { get; }
        bool HasOverlook { get; set; }
        bool IsVisible { get; set; }
        string FilterType { get; set; }
        IEnumerable<string> FilterTypes { get; }
        FieldFormat FieldFormat { get; }
        bool CanSort { get; set; }
        bool InsertBefore { get; set; }
    }
}