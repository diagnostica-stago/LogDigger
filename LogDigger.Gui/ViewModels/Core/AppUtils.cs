using System.IO;
using System.Reflection;

namespace LogDigger.Gui.ViewModels.Core
{
    public static class AppUtils
    {
        public static readonly string AppLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public static readonly string LogFileName = $"{Assembly.GetEntryAssembly().GetName().Name}.log";
        public static readonly string LogFilePath = Path.Combine(AppLocation, LogFileName);
    }
}