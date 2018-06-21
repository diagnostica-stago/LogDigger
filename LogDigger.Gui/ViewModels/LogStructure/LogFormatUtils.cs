using System.Collections.Generic;
using System.Linq;
using LogDigger.Business.Models;
using LogDigger.Gui.ViewModels.Columns;

namespace LogDigger.Gui.ViewModels.LogStructure
{
    public static class LogFormatUtils
    {
        public static ColumnVm GenerateCellTemplateBuilder(FieldFormat field, IReadOnlyList<FieldFormat> logFormatFields)
        {
            var colum = new ColumnVm(field, field.Name, logFormatFields.Select(x => x.Name));
            switch (field.Type)
            {
                case "DateTime":
                    colum.GenerateTemplate<DateCellTemplateVm>();
                    break;
                case "Level":
                    colum.GenerateTemplate<LevelCellTemplateVm>();
                    break;
            }
            return colum;
        }
    }
}