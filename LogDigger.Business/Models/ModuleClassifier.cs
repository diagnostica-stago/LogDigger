using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace LogDigger.Business.Models
{
    public class ModuleClassifier : IModuleClassifier
    {
        private readonly ConcurrentDictionary<string, string> _moduleForFile = new ConcurrentDictionary<string, string>();

        protected virtual IList<string> KnownModules { get; } = new List<string>();

        public string GetModuleForFile(string fileName)
        {
            return _moduleForFile.GetOrAdd(fileName, Update);
        }

        private string Update(string arg)
        {
            return KnownModules.FirstOrDefault(m => arg.Contains("." + m + ".")) ?? "[unknown]";
        }
    }
}