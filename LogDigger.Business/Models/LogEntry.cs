using System;
using System.Collections.Generic;
using System.Reflection;

namespace LogDigger.Business.Models
{
    public class LogEntry
    {
        public LogEntry(LogFile parent,
            string sourceLine,
            int sourceLineNumber,
            bool isClosed = false,
            bool isException = false,
            IReadOnlyDictionary<string, object> data = null,
            DateTime date = new DateTime(),
            string content = null,
            string level = null,
            string logger = null,
            string thread = null,
            string exception = null)
        {
            Parent = parent;
            // TODO: optim memoire. a remettre si on utilise la maj temps reel
            SourceLine = null; // sourceLine;
            SourceLineNumber = sourceLineNumber;
            IsClosed = isClosed;
            IsException = isException;
            Date = date;
            Content = !string.IsNullOrEmpty(exception) ? $"{content}{Environment.NewLine}{exception}" : content;
            Level = level;
            Logger = logger;
            Thread = thread;
            Data = data ?? new Dictionary<string, object>();
        }

        public IReadOnlyDictionary<string, object> Data { get; }

        public DateTime Date { get; set; }

        public string SourceLine { get; }

        public LogFile Parent { get; }

        public string Level { get; }

        public int SourceLineNumber { get; }

        public string Logger { get; }

        public string Thread { get; }

        public string Content { get; set; }

        public bool IsClosed { get; set; }

        public bool IsException { get; }

        public string Exception { get; }

        public T TryGetData<T>(string key)
        {
            Data.TryGetValue(key, out object value);
            return value is T valAsT ? valAsT : default(T);
        }
    }
}