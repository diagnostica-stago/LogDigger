using System.Collections.Generic;
using Newtonsoft.Json;

namespace LogDigger.Gui.ViewModels.Columns
{
    [JsonObject(MemberSerialization.OptIn)]
    public class HeaderedCellTemplateVm : ACellTemplateVm
    {
        public HeaderedCellTemplateVm()
        {
        }

        public HeaderedCellTemplateVm(string contentFieldName, string headerFieldName)
            : base(contentFieldName)
        {
            HeaderFieldName = headerFieldName;
        }

        [JsonProperty]
        public string HeaderFieldName
        {
            get => GetProperty<string>();
            set
            {
                if (SetProperty(value))
                {
                    StructureChanged.OnNext(nameof(HeaderFieldName));
                }
            }
        }

        public string Content => TryGet<string>(FieldName);
        public string Logger => TryGet<string>(HeaderFieldName);

        protected override IEnumerable<string> GetEntryDependentProperties()
        {
            yield return nameof(Content);
            yield return nameof(Logger);
        }

        public override ACellTemplateVm Clone()
        {
            return new HeaderedCellTemplateVm(FieldName, HeaderFieldName);
        }
    }
}