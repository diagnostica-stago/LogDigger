using System.Collections.Generic;
using Newtonsoft.Json;

namespace LogDigger.Gui.ViewModels.Columns
{
    [JsonObject(MemberSerialization.OptIn)]
    public class DefaultCellTemplateVm : ACellTemplateVm
    {
        public string Content => TryGet<string>(FieldName);

        public DefaultCellTemplateVm()
        {
        }

        public DefaultCellTemplateVm(string fieldName) : base(fieldName)
        {
        }

        protected override IEnumerable<string> GetEntryDependentProperties()
        {
            yield return nameof(Content);
        }

        public override ACellTemplateVm Clone()
        {
            return new DefaultCellTemplateVm(FieldName);
        }
    }
}