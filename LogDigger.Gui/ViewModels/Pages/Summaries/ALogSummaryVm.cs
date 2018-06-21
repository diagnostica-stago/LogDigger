using System;
using System.Collections.Generic;
using LogDigger.Business.Models;
using LogDigger.Gui.ViewModels.Core;
using LogDigger.Gui.ViewModels.LogEntries;

namespace LogDigger.Gui.ViewModels.Pages.Summaries
{
    public abstract class ALogSummaryVm : AViewModel
    {
        public LogEntry SourceEntry { get; }

        protected ALogSummaryVm(LogEntry sourceEntry)
        {
            SourceEntry = sourceEntry;
        }

        public virtual void LookForExtraInformation(IEnumerable<LogEntry> entries)
        {
        }

        public abstract string Details { get; }

        public abstract LogType LogType { get; }

        public DateTime Date => SourceEntry.Date;

        public virtual string TypeColor => "Black";

        public virtual string TypeName => LogType.ToString();
    }
}