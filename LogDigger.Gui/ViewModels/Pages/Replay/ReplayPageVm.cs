using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using LogDigger.Business.Models;
using LogDigger.Gui.ViewModels.Core;
using LogDigger.Gui.ViewModels.LogEntries;
using ReactiveUI;
using SimpleLogger;

namespace LogDigger.Gui.ViewModels.Pages.Replay
{
    public class ReplayPageVm : APageVm
    {
        private static string StartAssemblyName = @"Stago.Mhp.TechCore.StartAssembly";
        private static string EmulatorGuiName = @"Stago.Mhp.Fps.Emulator.Gui.Host";
        private static string MainGuiName = @"Stago.Mhp.Instr.PsMh.Gui";

        private IList<LogEntryVm> _logEntries;
        private LogEntryVm _selectedItem;
        private bool _mustStop;
        private IReadOnlyList<LogFile> _files;
        private CancellationTokenSource _cancellationTokenSource;
        private Process _mainWindowProc;
        private Process _emulatorProc;
        private readonly LogPlayer _player;
        private bool _leftControlDown;
        private KeyboardListener _keyboardListener;

        public ReplayPageVm(INavigator navigator)
            : base(navigator)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _player = new LogPlayer();
            Speed = 1.0;
        }

        public override string Title => "Replay";

        public override string Icon => "Play";

        public string Error
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }
        
        public ICommand ReplayCommand => ReactiveCommand.CreateFromTask(CallReplay, this.WhenAnyValue(x => x.CanReplay).ObserveOnDispatcher());

        public ICommand StopCommand => ReactiveCommand.CreateFromTask(CallStop, this.WhenAnyValue(x => x.CanStop).ObserveOnDispatcher());

        public ICommand AttachToProcessCommand => ReactiveCommand.CreateFromTask(CallAttachToProcess, this.WhenAnyValue(x => x.CanAttach).ObserveOnDispatcher());

        public bool CanAttach => !CanReplay;

        public bool CanReplay => _emulatorProc != null && _mainWindowProc != null;

        public bool CanStop => Replaying;

        public override void Activate()
        {
            KeyboardListener.KeyDown += OnGlobalKeyDown;
            KeyboardListener.KeyUp += OnGlobalKeyUp;
        }

        public override void Deactivate()
        {
            KeyboardListener.KeyDown -= OnGlobalKeyDown;
            KeyboardListener.KeyUp -= OnGlobalKeyUp;
        }

        internal bool IsRunAsAdmin()
        {
            var id = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(id);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        private Task CallAttachToProcess()
        {
            return Task.Run(() =>
            {
                if (!IsRunAsAdmin())
                {
                    Logger.Log("Cannot attach if not admin.");
                    GlobalMessageQueue.Current.Enqueue("Restart LogDigger as admin to do that.");
                    return;
                }
                if (_mainWindowProc != null)
                {
                    _mainWindowProc.Exited -= OnMainWindowProcExited;
                }
                if (_emulatorProc != null)
                {
                    _emulatorProc.Exited -= OnEmulatorProcExited;
                }

                _mainWindowProc = ProcessHelper.GetProcess(StartAssemblyName, MainGuiName);
                _emulatorProc = ProcessHelper.GetProcess(EmulatorGuiName);

                if (_mainWindowProc != null)
                {
                    _mainWindowProc.EnableRaisingEvents = true;
                    _mainWindowProc.Exited += OnMainWindowProcExited;
                }
                else
                {
                    GlobalMessageQueue.Current.Enqueue($"Cannot attach to {StartAssemblyName}/{MainGuiName}.");
                }

                if (_emulatorProc != null)
                {
                    _emulatorProc.EnableRaisingEvents = true;
                    _emulatorProc.Exited += OnEmulatorProcExited;
                }
                else
                {
                    GlobalMessageQueue.Current.Enqueue($"Cannot attach to {EmulatorGuiName}.");
                }
                
                this.RaisePropertyChanged(nameof(CanReplay));
                this.RaisePropertyChanged(nameof(CanAttach));
            });
        }

        private void OnEmulatorProcExited(object sender, EventArgs e)
        {
            _emulatorProc.Exited -= OnEmulatorProcExited;
            _emulatorProc = null;

            this.RaisePropertyChanged(nameof(CanReplay));
            this.RaisePropertyChanged(nameof(CanAttach));
        }

        private void OnMainWindowProcExited(object sender, EventArgs e)
        {
            _mainWindowProc.Exited -= OnMainWindowProcExited;
            _mainWindowProc = null;

            this.RaisePropertyChanged(nameof(CanReplay));
            this.RaisePropertyChanged(nameof(CanAttach));
        }

        private Task CallStop()
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();
            _mustStop = true;
            return Task.CompletedTask;
        }

        private void SkipDelay()
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();
        }

        private async Task CallReplay()
        {

            Replaying = true;
            this.RaisePropertyChanged(nameof(CanStop));
            var entries = SelectedItem == null ? LogEntries : LogEntries.SkipWhile(x => x != SelectedItem).ToList();
            for (int i = 0; i < entries.Count; i++)
            {
                var logEntry = entries[i];
                if (_mustStop)
                {
                    _mustStop = false;
                    break;
                }
                var nextLogEntry = i + 1 < entries.Count ? entries[i + 1] : null;
                // skip the event if it has been promoted
                if (nextLogEntry?.IsPromotedEventOf(logEntry) ?? false)
                {
                    i++;
                    logEntry = nextLogEntry;
                    nextLogEntry = i + 1 < entries.Count ? entries[i + 1] : null;
                }

                SelectedItem = logEntry;

                var process = logEntry.FileName.Contains(MainGuiName) ? _mainWindowProc : _emulatorProc;
                await _player.Play(process, logEntry, entries);

                if (nextLogEntry != null)
                {
                    var gap = TimeSpan.FromTicks((long)((nextLogEntry.Date - logEntry.Date).Ticks * (1 / Speed)));
                    NextEventTime = DateTime.Now + gap;
                    NextEventType = (nextLogEntry.ContentInfo as InputContentInfo)?.Type;
                    try
                    {
                        await Task.Delay(gap, _cancellationTokenSource.Token);
                    }
                    catch (TaskCanceledException)
                    {
                        // ok
                    }
                }
            }
            Replaying = false;
            this.RaisePropertyChanged(nameof(CanStop));
            _leftControlDown = false;
        }

        public DateTime NextEventTime
        {
            get { return GetProperty<DateTime>(); }
            set
            {
                if (SetProperty(value))
                {
                    this.RaisePropertyChanged(nameof(WaitingTime));
                }
            }
        }

        public string NextEventType
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public TimeSpan WaitingTime => NextEventTime - DateTime.Now;

        private void OnGlobalKeyUp(object sender, RawKeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl)
            {
                _leftControlDown = false;
            }
        }

        private void OnGlobalKeyDown(object sender, RawKeyEventArgs e)
        {
            Debug.WriteLine(e.Key);
            if (e.Key == Key.LeftCtrl)
            {
                _leftControlDown = true;
            }
            if (e.Key == Key.Cancel || (_leftControlDown && e.Key == Key.P))
            {
                CallStop();
            }
            if (_leftControlDown && (e.Key == Key.Add || e.Key == Key.Right))
            {
                if (Replaying)
                {
                    SkipDelay();
                }
                else
                {
                    CallReplay();
                }
            }
        }

        public override IReadOnlyList<LogFile> FilterFiles(IReadOnlyList<LogFile> files)
        {
            return files.Where(x => Regex.IsMatch(x.Path, @".*\.Inputs\.log.*")).ToList();
        }

        public override async Task Reload(IReadOnlyList<LogFile> files)
        {
            _files = files;
            await Task.Run(CallLoadAll);
        }

        private Task CallLoadAll()
        {
            LogEntries = _files.SelectMany(f => f.Entries).Select(e => new LogEntryVm(e)).OrderBy(x => x.Date).ToList();
            return Task.CompletedTask;
        }

        public IList<LogEntryVm> LogEntries
        {
            get { return _logEntries; }
            set { UpdateCurrentProperty(ref _logEntries, value); }
        }

        public LogEntryVm SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                if (UpdateCurrentProperty(ref _selectedItem, value))
                {
                    // _selectedItem.DrawOnWindow(WindowVm.MainWindow);
                }
            }
        }

        public double Speed
        {
            get { return GetProperty<double>(); }
            set { SetProperty(value); }
        }

        public IList<double> AvailableSpeeds => new List<double> { 0.25, 0.5, 0.75, 1.0, 1.5, 2.0, 3.0 };

        public bool Replaying
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public KeyboardListener KeyboardListener
        {
            get
            {
                if (_keyboardListener == null)
                {
                    _keyboardListener = new KeyboardListener();
                }
                return _keyboardListener;
            }
        }

        public string[] WriteSafeReadAllLines(string path)
        {
            using (var csv = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var sr = new StreamReader(csv))
            {
                var file = new List<string>();
                while (!sr.EndOfStream)
                {
                    file.Add(sr.ReadLine());
                }

                return file.ToArray();
            }
        }
    }
}