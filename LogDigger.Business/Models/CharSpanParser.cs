using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Sprache;

namespace LogDigger.Business.Models
{
    public class CharSpanParser : ILogParser
    {
        private LogFormat _logFormat;

        public LogFormat Format => _logFormat;

        public void SetTemplate(string template)
        {
            _logFormat = LogStructureParser.LogFormat.Parse(template);
        }

        public IEnumerable<LogEntry> ParseEntries(LogFile logFile)
        {
            var allText = FileUtils.WriteSafeReadAllText(logFile.Path);
            return InternalParseEntries(logFile, allText);
        }

        public IEnumerable<LogEntry> ParseEntries(LogFile logFile, int maxChar)
        {
            var allText = new string(FileUtils.WriteSafeReadAllChars(logFile.Path, maxChar).Select(c => (char)c).ToArray());
            return InternalParseEntries(logFile, allText);
        }

        public IEnumerable<LogEntry> ParseEntries(LogFile logFile, string allText)
        {
            return InternalParseEntries(logFile, allText);
        }

        public bool IsParsable(LogFile logFile)
        {
            var allText = new string(FileUtils.WriteSafeReadAllChars(logFile.Path, 10000).Select(c => (char)c).ToArray());
            return InternalParseEntries(logFile, allText, stopOnFirstLine: true, maxChar: 3000).Any();
        }

        /// <summary>
        /// Generate <see cref="LogEntry"/> from a text source.
        /// This methods is very critical for performance, so be careful when refactoring it.
        /// </summary>
        private IEnumerable<LogEntry> InternalParseEntries(LogFile logFile, string allText, bool stopOnFirstLine = false, long maxChar = long.MaxValue)
        {
            var entries = new List<LogEntry>();
            var data = new string[Format.Fields.Count]; // new Dictionary<string, object>();
            var dataPointer = 0;
            using (var formatFieldEnum = Format.Fields.GetEnumerator())
            {
                formatFieldEnum.MoveNext();
                var lineCount = 0;
                var startContent = 0;
                var contentLength = 0;
                var sepPointer = -1;
                var currentFormatField = formatFieldEnum.Current;
                var currentSeparatorCharArray = currentFormatField.Separator.ToCharArray();
                var currentSeparatorPointer = 0;
                var textSpan = allText.AsSpan();
                // iterate over every character, and slice the text to get log parts
                for (int i = 0; i < textSpan.Length; i++)
                {
                    var c = textSpan[i];
                    if (c == '\n')
                    {
                        lineCount++;
                    }
                    if (c == currentSeparatorCharArray[currentSeparatorPointer])
                    {
                        sepPointer++;
                        currentSeparatorPointer++;
                        if (currentSeparatorPointer > currentSeparatorCharArray.Length - 1)
                        {
                            // end of sep => end of content
                            var content = textSpan.Slice(startContent, contentLength).ToString();
                            data[dataPointer] = content;
                            dataPointer++;
                            if (!formatFieldEnum.MoveNext())
                            {
                                // last separator has been parsed
                                // go back to beginning
                                formatFieldEnum.Reset();
                                formatFieldEnum.MoveNext();
                                var entry = GenerateEntry(logFile, lineCount, data);
                                if (entry != null)
                                {
                                    entries.Add(entry);
                                }
                                if (stopOnFirstLine)
                                {
                                    return entries;
                                }
                                dataPointer = 0;
                            }

                            currentFormatField = formatFieldEnum.Current;
                            currentSeparatorCharArray = currentFormatField.Separator.ToCharArray();
                            currentSeparatorPointer = 0;
                            sepPointer = -1;
                            startContent = i + 1;
                            contentLength = 0;
                        }
                    }
                    else
                    {
                        if (sepPointer > -1)
                        {
                            contentLength += sepPointer;
                            sepPointer = -1;
                        }

                        contentLength++;
                    }
                }
            }

            return entries;
        }

        protected virtual LogEntry GenerateEntry(LogFile logFile, int lineCount, string[] dataStr)
        {
            var data = new Dictionary<string, object>();
            for (int i = 0; i < Format.Fields.Count; i++)
            {
                var field = Format.Fields[i];
                var stringData = dataStr[i];
                if (field.Type == "DateTime")
                {
                    if (DateTime.TryParse(stringData.Replace(',', '.'), CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                    {
                        data.Add(field.Name, date);
                    }
                    else
                    {
                        // not parsable
                        return null;
                    }
                }
                else
                {
                    data.Add(field.Name, stringData);
                }
            }
            return CreateEntry(logFile, lineCount, data);
        }

        protected virtual LogEntry CreateEntry(LogFile logFile, int lineCount, Dictionary<string, object> data)
        {
            return new LogEntry(logFile, null, lineCount, false, false, data, date: GetDate(data), content: GetContent(data));
        }

        protected virtual string GetContent(Dictionary<string, object> data)
        {
            return string.Empty;
        }

        protected virtual DateTime GetDate(Dictionary<string, object> data)
        {
            return default(DateTime);
        }

        public LogEntry ParseText(LogFile logFile, string text, int sourceLineNumber)
        {
            return InternalParseEntries(logFile, text, true).FirstOrDefault();
        }
    }
}