using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace LogDigger.Gui.ViewModels.Pages.Replay
{
    /// <summary>
    /// Taken from snoop
    /// </summary>
    public class Injector
    {
        public static string Suffix(IntPtr windowHandle)
        {
            var window = new WindowInfo(windowHandle);
            string bitness = IntPtr.Size == 8 ? "64" : "32";
            string clr = "3.5";
            foreach (var module in window.Modules)
            {
                // a process is valid to snoop if it contains a dependency on PresentationFramework, PresentationCore, or milcore (wpfgfx).
                // this includes the files:
                // PresentationFramework.dll, PresentationFramework.ni.dll
                // PresentationCore.dll, PresentationCore.ni.dll
                // wpfgfx_v0300.dll (WPF 3.0/3.5)
                // wpfgrx_v0400.dll (WPF 4.0)

                // note: sometimes PresentationFramework.dll doesn't show up in the list of modules.
                // so, it makes sense to also check for the unmanaged milcore component (wpfgfx_vxxxx.dll).
                // see for more info: http://snoopwpf.codeplex.com/Thread/View.aspx?ThreadId=236335

                // sometimes the module names aren't always the same case. compare case insensitive.
                // see for more info: http://snoopwpf.codeplex.com/workitem/6090

                if
                (
                    module.szModule.StartsWith("PresentationFramework", StringComparison.OrdinalIgnoreCase) ||
                    module.szModule.StartsWith("PresentationCore", StringComparison.OrdinalIgnoreCase) ||
                    module.szModule.StartsWith("wpfgfx", StringComparison.OrdinalIgnoreCase)
                )
                {
                    if (FileVersionInfo.GetVersionInfo(module.szExePath).FileMajorPart > 3)
                    {
                        clr = "4.0";
                    }
                }
                if (module.szModule.Contains("wow64.dll"))
                {
                    if (FileVersionInfo.GetVersionInfo(module.szExePath).FileMajorPart > 3)
                    {
                        bitness = "32";
                    }
                }
            }
            return bitness + "-" + clr;
        }

        public static void Launch(IntPtr windowHandle, Assembly assembly, string className, string methodName)
        {
            var location = Assembly.GetEntryAssembly().Location;
            var directory = Path.GetDirectoryName(location);
            var file = Path.Combine(directory, @"Injectors\ManagedInjectorLauncher" + Suffix(windowHandle) + ".exe");

            var p = Process.Start(file, windowHandle + " \"" + assembly.Location + "\" \"" + className + "\" \"" + methodName + "\"");
        }
    }
}