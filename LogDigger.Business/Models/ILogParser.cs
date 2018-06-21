using System.Collections.Generic;

namespace LogDigger.Business.Models
{
    public interface ILogParser
    {
        void SetTemplate(string template);
        LogEntry ParseText(LogFile logFile, string text, int sourceLineNumber);
        LogFormat Format { get; }
        IEnumerable<LogEntry> ParseEntries(LogFile logFile);
        IEnumerable<LogEntry> ParseEntries(LogFile logFile, int maxChar);
        IEnumerable<LogEntry> ParseEntries(LogFile logFile, string allText);
    }
}