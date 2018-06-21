using System.Diagnostics;
using System.Linq;
using System.Management;

namespace LogDigger.Gui.ViewModels.Pages.Replay
{
    public static class ProcessHelper
    {
        public static Process GetProcess(string processName, string assemblyName = null)
        {
            var processesByName = Process.GetProcessesByName(processName);
            if (assemblyName == null)
            {
                return processesByName.FirstOrDefault();
            }
            return processesByName.FirstOrDefault(e => GetCommandLine(e)?.Contains(assemblyName) == true);
        }

        public static string GetCommandLine(this Process p)
        {
            var wmiQuery = $"select CommandLine from Win32_Process where ProcessId='{p.Id}'";
            var searcher = new ManagementObjectSearcher(wmiQuery);
            var retObjectCollection = searcher.Get();
            foreach (var retObject in retObjectCollection)
            {
                return retObject["CommandLine"]?.ToString();
            }
            return null;
        }
    }
}