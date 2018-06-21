using System.Text.RegularExpressions;

namespace LogDigger.Gui.ViewModels.LogEntries
{
    public static class ContentInfoParser
    {
        public static ILogContentInfo Parse(string content)
        {
            if (content == null)
            {
                return new NoContentInfo();
            }

            var match = Regex.Match(content, @"([a-zA-Z0-9]*): (.*) window=\[(.*)\] elem=");
            if (match.Success)
            {
                var eventType = match.Groups[1].Value;
                var eventData = match.Groups[2].Value;
                var window = match.Groups[3].Value;
                return new InputContentInfo(eventType, eventData, window);
            }
            return new NoContentInfo();
        }
    }
}