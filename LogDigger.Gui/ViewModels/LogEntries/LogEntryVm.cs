using System;
using System.Collections.Generic;
using System.IO;
using LogDigger.Business.Models;
using LogDigger.Gui.ViewModels.Core;
using ReactiveUI;

namespace LogDigger.Gui.ViewModels.LogEntries
{
    public class LogEntryVm : AViewModel
    {
        private readonly LogEntry _logEntry;
        private bool _isSelected;
        private double _shift;
        private ILogContentInfo _contentInfo;

        public LogEntryVm(LogEntry logEntry)
        {
            _logEntry = logEntry;
        }

        public string Content => Entry.Content;

        public DateTime Date => Entry.Date;

        public string FileName => Path.GetFileName(Entry.Parent.Path);

        public string Level => Entry.Level;

        public string Thread => Entry.Thread;

        public bool IsException => Entry.IsException;

        public bool IsSelected
        {
            get { return _isSelected; }
            set { this.RaiseAndSetIfChanged(ref _isSelected, value); }
        }

        public string Logger => Entry.Logger;

        public string Module => Entry.Parent.Module;

        public double Shift
        {
            get { return _shift; }
            set { this.RaiseAndSetIfChanged(ref _shift, value); }
        }

        public string SourceFile => Entry.Parent.Path;

        public string SourceFileName => Path.GetFileNameWithoutExtension(SourceFile);

        public int SourceLineNumber => Entry.SourceLineNumber;

        public ILogContentInfo ContentInfo
        {
            get
            {
                if (_contentInfo == null)
                {
                    _contentInfo = ContentInfoParser.Parse(Entry.Content);
                }
                return _contentInfo;
            }
        }

        public IReadOnlyDictionary<string, object> Data => Entry.Data;

        protected LogEntry Entry
        {
            get { return _logEntry; }
        }

        public T TryGetData<T>(string key)
        {
            Data.TryGetValue(key, out object value);
            return value is T valAsT ? valAsT : default(T);
        }
    }
}