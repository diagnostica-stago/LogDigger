using MaterialDesignThemes.Wpf;

namespace LogDigger.Gui.ViewModels.Core
{
    public static class GlobalMessageQueue
    {
        public static SnackbarMessageQueue Current { get; } = new SnackbarMessageQueue();
    }
}