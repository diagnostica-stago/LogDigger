using System.Windows;

namespace LogDigger.Gui.ViewModels.LogEntries
{
    public class InputContentInfo : ILogContentInfo
    {
        public InputContentInfo(string type, string data, string window)
        {
            Type = type;
            Data = data;
            Window = window;
            var positions = Data.Split(';');
            if (positions.Length == 2)
            {
                double x, y;
                if (double.TryParse(positions[0], out x) && double.TryParse(positions[1], out y))
                {
                    Position = new Point(x, y);
                }
            }
        }

        public Point? Position { get; }

        public string Type { get; }

        public string Data { get; }

        public string Window { get; }
    }
}