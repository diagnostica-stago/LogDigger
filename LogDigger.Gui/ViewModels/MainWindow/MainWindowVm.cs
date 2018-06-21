using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Deployment.Application;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Threading.Tasks;
using System.Windows.Input;
using Dragablz;
using LogDigger.Business.Models;
using LogDigger.Business.Services;
using LogDigger.Gui.ViewModels.Core;
using LogDigger.Gui.ViewModels.LogStructure;
using LogDigger.Gui.ViewModels.Pages.All;
using LogDigger.Gui.ViewModels.Settings;
using MaterialDesignThemes.Wpf;
using ReactiveUI;

namespace LogDigger.Gui.ViewModels.MainWindow
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// </summary>
    public class MainWindowVm : AViewModel, INavigator
    {
        private readonly ILogExtractionService _logService;
        private readonly IModalHandler _modalHandler;
        private readonly ILogParser _parser;
        private List<LogFile> _files;

        public MainWindowVm(ILogExtractionService logService, IModalHandler modalHandler, ILogParser parser, ILogStructureVm logStructure, IModuleClassifier moduleClassifier)
        {
            _logService = logService;
            _modalHandler = modalHandler;
            _parser = parser;
            ModuleClassifier = moduleClassifier;
            LogPath = string.Empty;
            LogStructure = logStructure;
            ResetPages();
        }

        public void ResetPages()
        {
            Pages = BuildPages();
            SelectedPage = Pages[0];
        }

        protected virtual ObservableCollection<IPage> BuildPages()
        {
            return new ObservableCollection<IPage>
            {
                new AllEntriesPageVm(this, Parser, LogStructure.Columns, ModuleClassifier),
                // new SummaryEntriesPageVm(this) // todo: do not work properly yet in a generic context
            };
        }

        public void ResetAndLoadPages()
        {
            ResetPages();
            CallLoad();
        }

        public ICommand LoadCommand => ReactiveCommand.Create(CallLoad);

        public ICommand ParseCommand => ReactiveCommand.Create(CallParse);

        public ICommand RefreshCommand => ReactiveCommand.Create(CallRefresh);

        public ICommand OpenSettingsCommand => ReactiveCommand.Create(CallOpenSettings);

        public ICommand OpenNewSessionCommand => ReactiveCommand.Create(CallOpenNewSession);


        public ObservableCollection<IPage> Pages
        {
            get { return GetProperty<ObservableCollection<IPage>>(); }
            set { SetProperty(value); }
        }

        public ItemActionCallback CloseItemCallback => OnItemClosed;

        private void OnItemClosed(ItemActionCallbackArgs<TabablzControl> args)
        {
            var closeable = args.DragablzItem.Content as ICloseable;
            closeable?.Close();
        }

        public bool IsLoading
        {
            get { return GetProperty<bool>(); }
            set
            {
                if (SetProperty(value))
                {
                    this.RaisePropertyChanged(nameof(ShowFileForm));
                }
            }
        }

        public string ProgressState
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }

        public bool Loaded
        {
            get { return GetProperty<bool>(); }
            set
            {
                if (SetProperty(value))
                {
                    this.RaisePropertyChanged(nameof(ShowFileForm));
                    this.RaisePropertyChanged(nameof(CurrentDocument));
                    this.RaisePropertyChanged(nameof(Title));
                }
            }
        }

        public string LogPath
        {
            get { return GetProperty<string>(); }
            set
            {
                if (SetProperty(value))
                {
                    this.RaisePropertyChanged(nameof(CurrentDocument));
                    this.RaisePropertyChanged(nameof(Title));
                }
            }
        }

        public string Error
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public bool ShowFileForm => !Loaded && !IsLoading && !ShowLogStructure;

        public string Title => $"Log Digger{VersionString}{CurrentFileName}";

        public string CurrentDocument => Loaded ? LogPath : string.Empty;

        public string CurrentFileName => Loaded ? $" - {Path.GetFileName(LogPath)}" : string.Empty;

        public string VersionString => ApplicationDeployment.IsNetworkDeployed ? $" ({ApplicationDeployment.CurrentDeployment.CurrentVersion})" : string.Empty;

        public IPage SelectedPage
        {
            get { return GetProperty<IPage>(); }
            set
            {
                var oldPage = SelectedPage;
                if (SetProperty(value))
                {
                    oldPage?.Deactivate();
                    SelectedPage?.Activate();
                }
            }
        }

        public bool ShowLogStructure
        {
            get => GetProperty<bool>();
            set
            {
                if (SetProperty(value))
                {
                    this.RaisePropertyChanged(nameof(ShowFileForm));
                }
            }
        }

        private void CallOpenNewSession()
        {
            Loaded = false;
        }

        private async void CallOpenSettings()
        {
            var settingVm = new SettingsVm(_modalHandler);
            await _modalHandler.OpenModal(settingVm);
        }

        private async void CallLoad()
        {
            IsLoading = true;
            Loaded = false;
            ProgressState = "Fetching files";
            var result = await Task.Run(() => _logService.FetchLogFiles(LogPath));
            if (!result.HasError)
            {
                IsLoading = false;
                ProgressState = "Reading logs";
                _files = result.Result;
                if (!SkipLogStructure)
                {
                    ShowLogStructure = true;

                    LogStructure.ParseCommand = ParseCommand;
                    LogStructure.Files.AddRange(_files.Select(x => new LogFilePreviewVm(x)));
                    LogStructure.Editing = true;
                }
                else
                {
                    CallParse();
                }
            }
        }

        public virtual bool SkipLogStructure => false;

        public ILogStructureVm LogStructure
        {
            get => GetProperty<ILogStructureVm>();
            private set => SetProperty(value);
        }

        protected ILogParser Parser => _parser;

        protected IModuleClassifier ModuleClassifier { get; }

        private async void CallParse()
        {
            ShowLogStructure = false;
            IsLoading = false;
            Loaded = true;
            ProgressState = "Reading logs";
            await Task.Run(() => _files.ForEach(x => x.PreLoad()));
            var dates = _files.Where(x => x.StartDate != null).GroupBy(x => x.StartDate.Value.KeepUntilMinutes())
                .ToDictionary(x => x.Key, x => x.ToList());
            var totalSize = _files.Sum(lf => lf.Length);
            if (totalSize > 1024 * 1024 * 100)
            {
                //show the dialog
                var tooBigLogModalVm = new TooBigLogModalVm(totalSize, dates);
                var takeAll = (bool) await _modalHandler.OpenModal(tooBigLogModalVm);
                ;
                if (!takeAll)
                {
                    var filesCopy = _files.ToList();
                    foreach (var file in filesCopy)
                    {
                        if (!file.Path.EndsWith(".log"))
                        {
                            _files.Remove(file);
                        }
                    }
                }
            }

            var reloadTasks = new List<Task>();

            GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;
            foreach (var page in Pages)
            {
                reloadTasks.Add(page.ReloadAll(_files));
            }

            await Task.WhenAll(reloadTasks);

            IsLoading = false;
        }

        private void ClosingEventHandler(object sender, DialogOpenedEventArgs eventargs)
        {
        }

        private void CallRefresh()
        {
            CallParse();
        }

        public void AddPage(IPage newPage)
        {
            MessageBus.Current.SendMessage(new NewTabMessage(newPage));

            // Pages.Add(newPage);
            SelectedPage = newPage;
        }

        public void ClosePage(IPage page)
        {
            if (SelectedPage == page)
            {
                SelectedPage = Pages[Pages.Count - 2];
            }
            Pages.Remove(page);
        }
    }
}
