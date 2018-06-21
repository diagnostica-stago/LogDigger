using System;
using System.Windows.Input;

namespace LogDigger.Gui.ViewModels.Pages.Replay
{
    /// <summary>
    /// Raw KeyEvent arguments.
    /// </summary>
    public class RawKeyEventArgs : EventArgs
    {
        /// <summary>
        /// VKCode of the key.
        /// </summary>
        public int VKCode { get; private set; }

        /// <summary>
        /// WPF Key of the key.
        /// </summary>
        public Key Key { get; private set; }

        /// <summary>
        /// Is the hitted key system key.
        /// </summary>
        public bool IsSysKey { get; private set; }

        /// <summary>
        /// Create raw keyevent arguments.
        /// </summary>
        public RawKeyEventArgs(int virtualKeyCode, bool isSysKey)
        {
            VKCode = virtualKeyCode;
            IsSysKey = isSysKey;
            Key = KeyInterop.KeyFromVirtualKey(virtualKeyCode);
        }
    }
}