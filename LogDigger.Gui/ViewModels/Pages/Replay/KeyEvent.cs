namespace LogDigger.Gui.ViewModels.Pages.Replay
{
    public enum KeyEvent : int
    {
        /// <summary>
        /// None
        /// </summary>
        None = 0,

        /// <summary>
        /// Wm Key down
        /// </summary>
        WmKeydown = 256,

        /// <summary>
        /// Wm key up
        /// </summary>
        WmKeyup = 257,

        /// <summary>
        /// Wm sys key up
        /// </summary>
        WmSyskeyup = 261,

        /// <summary>
        /// Wm sys key down
        /// </summary>
        WmSyskeydown = 260
    }
}