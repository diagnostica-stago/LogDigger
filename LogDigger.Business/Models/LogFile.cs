using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LogDigger.Business.Models
{
    public class LogFile
    {
        private readonly string _path;
        private readonly ILogParser _logParser;
        private readonly List<LogEntry> _entries;
        private volatile bool _isLoaded;
        private DateTime? _startDate;

        private object _syncLoad = new object();

        public event EventHandler<NewLinesEventArgs> NewLinesEvent; 

        public LogFile(string path, IParserSelector parserSelector, IModuleClassifier moduleClassifier)
        {
            _path = path;
            _entries = new List<LogEntry>();
            FileName = System.IO.Path.GetFileNameWithoutExtension(_path);
            Module = moduleClassifier.GetModuleForFile(System.IO.Path.GetFileName(_path));
            IsErrorsFile = FileName.EndsWith(".Errors");
            _logParser = parserSelector.GetParser(this);
        }

        public bool IsErrorsFile { get; }

        public long Length => new FileInfo(Path).Length;

        public string Path
        {
            get { return _path; }
        }

        public IList<LogEntry> Entries
        {
            get { return _entries; }
        }

        public DateTime? StartDate => _startDate;

        public string FileName { get; }

        public string Module { get; }

        public LogFile PreLoad()
        {
            var firstLines = FileUtils.WriteSafeReadLines(Path)
                .Take(2)
                .Aggregate(new StringBuilder(), (builder, newStr) => builder.AppendLine(newStr)).ToString();
            if (string.IsNullOrEmpty(firstLines))
            {
                return this;
            }
            var entry = _logParser.ParseText(this, firstLines, 0);
            _startDate = entry?.Date;
            return this;
        }

        public LogFile Load()
        {
            if (_isLoaded)
            {
                return this;
            }
            lock (_syncLoad)
            {
                if (_isLoaded)
                {
                    return this;
                }
                _entries.AddRange(_logParser.ParseEntries(this));
                _isLoaded = true;
            }
            return this;
        }

        public void ActivateFileSystemWatcher()
        {
            var fsw = new FileSystemWatcher(Directory.GetParent(_path).FullName);
            fsw.Filter = System.IO.Path.GetFileName(_path);
            fsw.EnableRaisingEvents = true;
            fsw.Changed += OnFileChanged;
        }

        private void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            var rlr = new ReverseLineReader(_path);
            var lines = new List<LogEntry>();
            using (var enumerator = rlr.GetEnumerator())
            {
                enumerator.MoveNext();
                var line = enumerator.Current;
                var lastLine = Entries.LastOrDefault()?.SourceLine;
                var lineCount = Entries.Count;
                while (!string.Equals(line, lastLine))
                {
                    lines.Add(_logParser.ParseText(this, line, lineCount));
                    lineCount++;
                    enumerator.MoveNext();
                    line = enumerator.Current;
                }
            }
            _entries.AddRange(lines);
            RaiseNewLinesEvent(new NewLinesEventArgs(lines));
        }

        public string[] WriteSafeReadAllLines(string path)
        {
            return WriteSafeReadLines(path).ToArray();
        }

        public IEnumerable<string> WriteSafeReadLines(string path)
        {
            using (var file = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (var sr = new StreamReader(file, Encoding.Default, true))
                {
                    while (!sr.EndOfStream)
                    {
                        yield return sr.ReadLine();
                    }
                }
            }
        }

        protected virtual void RaiseNewLinesEvent(NewLinesEventArgs e)
        {
            NewLinesEvent?.Invoke(this, e);
        }
    }
}