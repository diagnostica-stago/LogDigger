using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace LogDigger.Gui.ViewModels.Columns
{
    [JsonObject(MemberSerialization.OptIn)]
    public class DateCellTemplateVm : ACellTemplateVm
    {
        public DateTime Date => TryGet<DateTime>(FieldName);

        public DateCellTemplateVm()
        {
        }

        public DateCellTemplateVm(string fieldName) : base(fieldName)
        {
        }

        protected override IEnumerable<string> GetEntryDependentProperties()
        {
            yield return nameof(Date);
        }

        public override ACellTemplateVm Clone()
        {
            return new DateCellTemplateVm(FieldName);
        }
    }
}