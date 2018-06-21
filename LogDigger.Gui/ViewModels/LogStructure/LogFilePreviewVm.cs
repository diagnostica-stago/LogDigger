using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using LogDigger.Business.Models;
using LogDigger.Gui.ViewModels.Core;
using LogDigger.Gui.ViewModels.LogEntries;

namespace LogDigger.Gui.ViewModels.LogStructure
{
    public class LogFilePreviewVm : AViewModel
    {
        public LogFilePreviewVm(LogFile logFile)
        {
            File = logFile;
        }

        public ParsingState State
        {
            get => GetProperty<ParsingState>();
            set => SetProperty(value);
        }

        public LogFile File
        {
            get => GetProperty<LogFile>();
            set => SetProperty(value);
        }

        public ObservableCollection<LogEntryVm> PreviewEntries
        {
            get => GetProperty<ObservableCollection<LogEntryVm>>();
            set => SetProperty(value);
        }

        public string PreviewText
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }

        public void Load(ILogParser parser)
        {
            var file = File;
            var entries = LoadEntries(file, parser).Result;
            if (entries != null && entries.Any())
            {
                State = ParsingState.Ok;
            }
            else
            {
                State = ParsingState.Failed;
            }

            if (entries != null)
            {
                PreviewEntries = new ObservableCollection<LogEntryVm>(entries);
            }
        }

        private async Task<IEnumerable<LogEntryVm>> LoadEntries(LogFile file, ILogParser parser)
        {
            if (parser?.Format?.Fields == null)
            {
                return null;
            }

            var logFormat = new LogFormat(new List<FieldFormat>(parser.Format.Fields));
            if (file == null)
            {
                return null;
            }
            return await Task.Run(() =>
            {
                try
                {
                    PreviewText = new string(FileUtils.WriteSafeReadAllChars(file.Path, 1000).Select(c => (char)c).ToArray());
                    var entries = parser.ParseEntries(file, PreviewText).Select(x => new LogEntryVm(x));
                    return entries;
                }
                catch (Exception e)
                {
                    // error parsing entries
                    Console.WriteLine(e);
                    return null;
                }
            });
        }
    }
}