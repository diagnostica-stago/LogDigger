using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace LogDigger.Business.Models
{
    public enum LogStyle
    {
        DateFirst,
        IdFirstWithLogger,
        IdFirstWithoutLogger,
        Unknown
    }

    public class LogParser : ILogParser
    {
        private static readonly string[] Separators = new[] { "<#>" };
        private static readonly string Separator = Separators[0];
        
        public string[] ParseLine(string line)
        {
            var parts = new List<string>();
            using (var sepEnumerator = Separator.GetEnumerator())
            {
                sepEnumerator.MoveNext();
                var firstSepChar = sepEnumerator.Current;
                var sepCharFound = false;
                using (var reader = line.GetEnumerator())
                {
                    var currentString = new StringBuilder();
                    var sepCharBuffer = new StringBuilder();
                    while (reader.MoveNext())
                    {
                        var ch = reader.Current;
                        if (ch == sepEnumerator.Current
                            && (sepCharFound || sepEnumerator.Current == firstSepChar))
                        {
                            sepCharFound = true;
                            sepCharBuffer.Append(ch);
                            if (!sepEnumerator.MoveNext())
                            {
                                // end of section
                                parts.Add(currentString.ToString());
                                currentString.Clear();
                                sepCharFound = false;
                                sepEnumerator.Reset();
                                sepEnumerator.MoveNext();
                                sepCharBuffer.Clear();
                            }
                        }
                        else
                        {
                            currentString.Append(ch);
                            currentString.Append(sepCharBuffer);
                            sepCharFound = false;
                            sepEnumerator.Reset();
                            sepEnumerator.MoveNext();
                        }
                    }
                }
            }
            if (!parts.Any())
            {
                parts.Add(line);
            }
            return parts.ToArray();
        }

        public LogEntry ParseText(LogFile parent, string text, int sourceLineNumber)
        {
            var isClosed = false;
            var isException = false;
            var id = "[unknown]";
            var content = string.Empty;
            var level = string.Empty;
            var logger = string.Empty;
            var date = DateTime.MinValue;
            var thread = string.Empty;
            var exception = string.Empty;
            var info = text.Split(Separators, StringSplitOptions.None);

            var logStyle = ComputeLogStyle(info);

            var posDico = new Dictionary<string, int>();
            switch (logStyle)
            {
                case LogStyle.IdFirstWithLogger:
                    posDico = new Dictionary<string, int>
                    {
                        [nameof(id)] = 0,
                        [nameof(content)] = 5,
                        [nameof(logger)] = 4,
                        [nameof(level)] = 3,
                        [nameof(date)] = 1,
                        [nameof(thread)] = 2,
                        [nameof(exception)] = 6,
                    };
                    break;
                case LogStyle.DateFirst:
                    posDico = new Dictionary<string, int>
                    {
                        [nameof(id)] = 5,
                        [nameof(content)] = 4,
                        [nameof(logger)] = 3,
                        [nameof(level)] = 2,
                        [nameof(date)] = 0,
                        [nameof(thread)] = 1,
                        [nameof(exception)] = 6,
                    };
                    break;
                case LogStyle.IdFirstWithoutLogger:
                    posDico = new Dictionary<string, int>
                    {
                        [nameof(id)] = 0,
                        [nameof(content)] = 4,
                        [nameof(level)] = 3,
                        [nameof(date)] = 1,
                        [nameof(thread)] = 2,
                        [nameof(exception)] = 5,
                    };
                    break;
                default:
                    break;
            }

            try
            {
                if (text.StartsWith("==> FATAL"))
                {
                    isClosed = false;
                    isException = true;
                    id = "[unknown]";
                }
                else
                {
                    isClosed = true;
                    if (logStyle == LogStyle.Unknown)
                    {
                        id = "[unknown]";
                        content = text;
                    }
                    else
                    {
                        // id = info[0];
                        var dateStr = TryGet(info, posDico, nameof(date)).Replace(',', '.');
                        level = TryGet(info, posDico, nameof(level)).Trim(' ');
                        DateTime.TryParse(dateStr, CultureInfo.InvariantCulture, DateTimeStyles.None, out date);
                        logger = TryGet(info, posDico, nameof(logger));
                        content = TryGet(info, posDico, nameof(content));
                        thread = TryGet(info, posDico, nameof(thread));
                        if (!string.IsNullOrEmpty(content))
                        {
                            content = content.Substring(2, content.Length - 4);
                        }
                        id = TryGet(info, posDico, nameof(id));
                    }
                }
            }
            catch (Exception e)
            {
                content = text;
                id = "[unknown]";
                isClosed = true;
            }
            return new LogEntry(parent, text, sourceLineNumber, isClosed, isException, null, date, content: content, level: level, logger: logger, thread: thread);
        }

        public LogFormat Format => null;

        private string TryGet(string[] info, Dictionary<string, int> posDico, string key)
        {
            int pos;
            if (posDico.TryGetValue(key, out pos) && info.Length > posDico[key])
            {
                return info[posDico[key]];
            }
            return string.Empty;
        } 

        private LogStyle ComputeLogStyle(string[] info)
        {
            var dateStr = info[0].Replace(',', '.');
            DateTime date;
            var dateParsed = DateTime.TryParse(dateStr, CultureInfo.InvariantCulture, DateTimeStyles.None, out date);
            if (dateParsed)
            {
                return LogStyle.DateFirst;
            }
            if (info.Length == 7)
            {
                return LogStyle.IdFirstWithLogger;
            }
            if (info.Length == 6)
            {
                return LogStyle.IdFirstWithoutLogger;
            }
            return LogStyle.Unknown;
        }


        public void Append(LogEntry entry, string line)
        {
            var date = DateTime.MinValue;
            if (entry.IsClosed)
            {
                throw new Exception("Cannot append line to a closed entry.");
            }
            if (string.Equals(line, "-------------------"))
            {
                entry.IsClosed = true;
                return;
            }
            var values = line.Split(new[] { " : " }, StringSplitOptions.None);
            var key = values[0];
            var value = values.Length > 1 ? values[1] : string.Empty;
            if (key.StartsWith(" DATE"))
            {
                value = value.Replace(',', '.');
                DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out date);
                entry.Date = date;
            }
            else if (key.StartsWith(" THREAD ID"))
            {
                // todo
            }
            else if (key.StartsWith(" MESSAGE"))
            {
                // todo
                //_content = value;
            }
            else if (key.StartsWith(" EXCEPTION"))
            {
                // todo
            }
            else
            {
                var newLine = line.Trim(' ');
                if (!string.IsNullOrEmpty(newLine))
                {
                    entry.Content += newLine + Environment.NewLine;
                }
            }
        }

        public void SetTemplate(string template)
        {
        }

        public IEnumerable<LogEntry> ParseEntries(LogFile logFile)
        {
            var i = 0;
            var entries = new List<LogEntry>();
            LogEntry logEntry = null;
            foreach (var line in FileUtils.WriteSafeReadAllLines(logFile.Path))
            {
                if (string.IsNullOrEmpty(line) || string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }
                if (logEntry?.IsClosed ?? true)
                {
                    logEntry = ParseText(logFile, line, i);
                    entries.Add(logEntry);
                }
                else
                {
                    Append(logEntry, line);
                }
                i++;
            }

            return entries;
        }

        public IEnumerable<LogEntry> ParseEntries(LogFile logFile, int maxChar)
        {
            yield break;
        }

        public IEnumerable<LogEntry> ParseEntries(LogFile logFile, string allText)
        {
            yield break;
        }
    }
}
