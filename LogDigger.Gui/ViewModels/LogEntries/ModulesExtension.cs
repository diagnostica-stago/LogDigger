using System.IO;
using LogDigger.Business.Models;

namespace LogDigger.Gui.ViewModels.LogEntries
{
    public static class ModulesExtension
    {
        public static string GetModule(this LogEntry entry)
        {
            return entry.Parent.Module;
        }
    }
}