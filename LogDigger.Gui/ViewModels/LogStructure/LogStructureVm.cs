using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using LogDigger.Business.Models;
using LogDigger.Gui.ViewModels.Columns;
using LogDigger.Gui.ViewModels.Core;
using LogDigger.Gui.ViewModels.LogEntries;
using Newtonsoft.Json;
using ReactiveUI;
using Sprache;

namespace LogDigger.Gui.ViewModels.LogStructure
{
    [JsonObject(MemberSerialization.OptIn)]
    public class LogStructureVm : AViewModel, IActivable, ILogStructureVm
    {
        private LogFormat _logFormat;
        private IDisposable _onColumnsChanged;

        public LogStructureVm()
        {
            Columns = new ReactiveList<IColumnDescriptionVm> { ChangeTrackingEnabled = true };

            this.WhenAnyValue(x => x.StringTemplate)
                .Throttle(TimeSpan.FromMilliseconds(300))
                .ObserveOnDispatcher()
                .Subscribe(template => GenerateColumns());

            Files = new ReactiveList<LogFilePreviewVm> { ChangeTrackingEnabled = true };
            Files.ItemChanged.Subscribe(item =>
            {
                this.RaisePropertyChanged(nameof(TotalFilesCount));
                this.RaisePropertyChanged(nameof(ParsedFilesCount));
            });
        }

        private void RegisterColumnsChanged()
        {
            _onColumnsChanged?.Dispose();

            _onColumnsChanged = Observable.Empty<object>()
                .Merge(Columns.Changed)
                .Merge(Columns.ItemChanged)
                // .Merge(this.WhenAnyValue(x => x.SelectedFile))
                .Throttle(TimeSpan.FromMilliseconds(450))
                .ObserveOnDispatcher()
                .Subscribe(template => TryLoadEntries());
        }

        public LogStructureVm(ILogParser parser)
            : this()
        {
            Parser = parser;
        }

        public ICommand ParseCommand
        {
            get => GetProperty<ICommand>();
            set => SetProperty(value);
        }

        [JsonProperty]
        public ReactiveList<IColumnDescriptionVm> Columns
        {
            get => GetProperty<ReactiveList<IColumnDescriptionVm>>();
            private set
            {
                if (SetProperty(value))
                {
                    Columns.ChangeTrackingEnabled = true;
                    RegisterColumnsChanged();
                }
            }
        }

        [JsonProperty]
        public string StringTemplate
        {
            get => GetProperty<string>();
            set
            {
                if (SetProperty(value) && Editing)
                {
                    _logFormat = LogStructureParser.LogFormat.Parse(value);
                    Parser.SetTemplate(value);
                }
            }
        }

        public bool Editing { get; set; }

        public LogFilePreviewVm SelectedFile
        {
            get => GetProperty<LogFilePreviewVm>();
            set => SetProperty(value);
        }

        public ReactiveList<LogFilePreviewVm> Files { get; }

        [JsonProperty]
        public LogFormat Format
        {
            get { return _logFormat; }
            set { _logFormat = value; }
        }

        public ILogParser Parser { get; set; }

        public int TotalFilesCount => Files.Count;

        public int ParsedFilesCount => Files.Count(x => x.State == ParsingState.Ok);

        public void GenerateColumns()
        {
            if (Parser?.Format == null)
            {
                return;
            }

            var logFormat = new LogFormat(new List<FieldFormat>(Parser.Format.Fields));
            Columns.Clear();
            foreach (var field in logFormat.Fields)
            {
                Columns.Add(LogFormatUtils.GenerateCellTemplateBuilder(field, logFormat.Fields));
            }
        }

        private void TryLoadEntries()
        {
            Task.Run(() =>
            {
                if (Files == null)
                {
                    return;
                }

                foreach (var filePreview in Files)
                {
                    Task.Run(() => filePreview.Load(Parser));
                }
            });
        }

        private async Task<IEnumerable<LogEntryVm>> LoadEntries(LogFile file)
        {
            if (Parser?.Format?.Fields == null)
            {
                return null;
            }

            var logFormat = new LogFormat(new List<FieldFormat>(Parser.Format.Fields));
            if (file == null)
            {
                return null;
            }
            return await Task.Run(() =>
            {
                try
                {
                    var entries = Parser.ParseEntries(file, 1000).Select(x => new LogEntryVm(x));
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

        public void Activate()
        {
        }

        public void Deactivate()
        {
        }
    }
}
