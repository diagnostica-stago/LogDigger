using Newtonsoft.Json;

namespace LogDigger.Business.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    public class FieldFormat
    {
        public FieldFormat(string name, string type, string separator)
        {
            Name = name;
            Type = type;
            Separator = separator;
        }

        [JsonProperty]
        public string Name { get; }
        [JsonProperty]
        public string Type { get; }
        [JsonProperty]
        public string Separator { get; }
    }
}