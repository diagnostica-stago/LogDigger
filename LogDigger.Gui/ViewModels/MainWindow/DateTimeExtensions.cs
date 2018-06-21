using System;

namespace LogDigger.Gui.ViewModels.MainWindow
{
    public static class DateTimeExtensions
    {
        public static DateTime KeepUntilMinutes(this DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, 0);
        }

    }
}