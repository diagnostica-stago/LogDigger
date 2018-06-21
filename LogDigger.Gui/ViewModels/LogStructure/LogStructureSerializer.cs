using Newtonsoft.Json;

namespace LogDigger.Gui.ViewModels.LogStructure
{
    public static class LogStructureSerializer
    {
        public static string Serialize(ILogStructureVm structure)
        {
            var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
            return JsonConvert.SerializeObject(structure, Formatting.Indented, settings);
        }

        public static LogStructureVm Deserialize(string json)
        {
            var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
            return JsonConvert.DeserializeObject<LogStructureVm>(json, settings);
        }
    }
}