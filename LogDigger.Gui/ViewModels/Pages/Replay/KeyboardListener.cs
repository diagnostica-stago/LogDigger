using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace LogDigger.Gui.ViewModels.Pages.Replay
{
    /// <summary>
    /// Listens keyboard globally.
    /// Code taken from: http://stackoverflow.com/questions/1639331/using-global-keyboard-hook-wh-keyboard-ll-in-wpf-c-sharp
    /// <remarks>Uses WH_KEYBOARD_LL.</remarks>
    /// </summary>
    public class KeyboardListener : IDisposable
    {
        /// <summary>
        /// Creates global keyboard listener.
        /// </summary>
        public KeyboardListener()
        {
            // We have to store the HookCallback, so that it is not garbage collected runtime
            _hookedLowLevelKeyboardProc = LowLevelKeyboardProc;

            // Set the hook
            _hookId = InterceptKeys.SetHook(_hookedLowLevelKeyboardProc);

            // Assign the asynchronous callback event
            _hookedKeyboardCallbackAsync = KeyboardListenerKeyboardCallbackAsync;
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="KeyboardListener"/> class.
        /// Destroys global keyboard listener.
        /// </summary>
        ~KeyboardListener()
        {
            Dispose(false);
        }

        /// <summary>
        /// Fired when any of the keys is pressed down.
        /// </summary>
        public event EventHandler<RawKeyEventArgs> KeyDown;

        /// <summary>
        /// Fired when any of the keys is released.
        /// </summary>
        public event EventHandler<RawKeyEventArgs> KeyUp;

        #region Inner workings

        /// <summary>
        /// Hook ID
        /// </summary>
        private readonly IntPtr _hookId = IntPtr.Zero;

        /// <summary>
        /// Event to be invoked asynchronously (BeginInvoke) each time key is pressed.
        /// </summary>
        private readonly KeyboardCallbackAsync _hookedKeyboardCallbackAsync;

        /// <summary>
        /// Contains the hooked callback in runtime.
        /// </summary>
        private readonly LowLevelKeyboardProc _hookedLowLevelKeyboardProc;

        /// <summary>
        /// Actual callback hook.
        /// <remarks>Calls asynchronously the asyncCallback.</remarks>
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1305:FieldNamesMustNotUseHungarianNotation", Justification = "Reviewed. Suppression is OK here.")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private IntPtr LowLevelKeyboardProc(int nCode, UIntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                if (wParam.ToUInt32() == (int)KeyEvent.WmKeydown || wParam.ToUInt32() == (int)KeyEvent.WmKeyup || wParam.ToUInt32() == (int)KeyEvent.WmSyskeydown || wParam.ToUInt32() == (int)KeyEvent.WmSyskeyup)
                {
                    _hookedKeyboardCallbackAsync.BeginInvoke((KeyEvent)wParam.ToUInt32(), Marshal.ReadInt32(lParam), null, null);
                }
            }

            return NativeMethods.CallNextHookEx(_hookId, nCode, wParam, lParam);
        }

        /// <summary>
        /// HookCallbackAsync procedure that calls accordingly the KeyDown or KeyUp events.
        /// </summary>
        /// <param name="keyEvent">Keyboard event</param>
        /// <param name="virtualKeyCode">The VKCode</param>
        private void KeyboardListenerKeyboardCallbackAsync(KeyEvent keyEvent, int virtualKeyCode)
        {
            switch (keyEvent)
            {
                // KeyDown events
                case KeyEvent.WmKeydown:
                    if (KeyDown != null)
                    {
                        KeyDown(this, new RawKeyEventArgs(virtualKeyCode, false));
                    }
                    break;
                case KeyEvent.WmSyskeydown:
                    if (KeyDown != null)
                    {
                        KeyDown(this, new RawKeyEventArgs(virtualKeyCode, true));
                    }
                    break;

                // KeyUp events
                case KeyEvent.WmKeyup:
                    if (KeyUp != null)
                    {
                        KeyUp(this, new RawKeyEventArgs(virtualKeyCode, false));
                    }
                    break;
                case KeyEvent.WmSyskeyup:
                    if (KeyUp != null)
                    {
                        KeyUp(this, new RawKeyEventArgs(virtualKeyCode, true));
                    }
                    break;
            }
        }

        /// <summary>
        /// Asynchronous callback hook.
        /// </summary>
        private delegate void KeyboardCallbackAsync(KeyEvent keyEvent, int vkCode);

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Disposes the hook.
        /// <remarks>This call is required as it calls the UnhookWindowsHookEx.</remarks>
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // free managed resources
            }
            NativeMethods.UnhookWindowsHookEx(_hookId);
        }

        #endregion
    }
}