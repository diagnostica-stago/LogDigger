using System;
using System.Globalization;

namespace LogDigger.Gui.Views.Converters
{
    public class TickToDateTimeConverter : AMarkupConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (value)
            {
                case long longTick:
                    if (longTick <= DateTime.MaxValue.Ticks && longTick >= DateTime.MinValue.Ticks)
                    {
                        return new DateTime(longTick);
                    }
                    break;
                case double doubleTick:
                    if (doubleTick <= DateTime.MaxValue.Ticks && doubleTick >= DateTime.MinValue.Ticks)
                    {
                        return new DateTime((long)doubleTick);
                    }
                    break;
            }
            return default(DateTime);
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime date)
            {
                return date.Ticks;
            }
            return 0.0;
        }
    }
}