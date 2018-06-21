using Newtonsoft.Json;

namespace LogDigger.Gui.ViewModels.Columns
{
    [JsonObject(MemberSerialization.OptIn)]
    public class LevelCellTemplateVm : ACellTemplateVm
    {
        public LevelCellTemplateVm()
        {
        }

        private LevelCellTemplateVm(string fieldName)
            : base(fieldName)
        {
        }

        public string Level => TryGet<string>(FieldName);

        public override ACellTemplateVm Clone()
        {
            return new LevelCellTemplateVm(FieldName);
        }
    }
}