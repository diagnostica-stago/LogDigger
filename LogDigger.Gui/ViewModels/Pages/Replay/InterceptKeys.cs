using System;
using System.Diagnostics;

namespace LogDigger.Gui.ViewModels.Pages.Replay
{
    /// <summary>
    /// Winapi Key interception helper class.
    /// </summary>
    internal static class InterceptKeys
    {
        private const int WhKeyboardLl = 13;

        public static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return NativeMethods.SetWindowsHookEx(WhKeyboardLl, proc, NativeMethods.GetModuleHandle(curModule.ModuleName), 0);
            }
        }
    }
}