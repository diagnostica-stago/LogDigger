using System;
using System.Collections.Generic;

namespace LogDigger.Business.Models
{
    public class NewLinesEventArgs : EventArgs
    {
        public NewLinesEventArgs(List<LogEntry> lines)
        {
            Lines = lines;
        }

        public List<LogEntry> Lines { get; }
    }
}