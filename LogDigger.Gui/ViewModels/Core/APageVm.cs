using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime;
using System.Threading.Tasks;
using System.Windows.Input;
using LogDigger.Business.Models;
using ReactiveUI;
using SimpleLogger;

namespace LogDigger.Gui.ViewModels.Core
{
    public abstract class APageVm : AViewModel, IPage
    {
        protected APageVm(INavigator navigator)
        {
            Navigator = navigator;
        }

        public bool IsLoading
        {
            get => GetProperty<bool>();
            set => SetProperty(value);
        }

        public string ProgressInfo
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }

        public abstract string Title { get; }

        public virtual string Icon => "ViewList";

        public virtual bool CanClosePage => false;

        public ICommand ClosePageCommand => ReactiveCommand.Create(CallClosePage);

        protected INavigator Navigator { get; }

        private void CallClosePage()
        {
            Navigator.ClosePage(this);
        }

        protected void SetProgress(string info, int current = 0, int total = 0)
        {
            var progressInfo = total == 0 ? info : $"{info} {(int)(current / (double)total * 100)}%";
            if (!string.Equals(progressInfo, ProgressInfo))
            {
                ProgressInfo = progressInfo;
            }
        }

        public virtual IReadOnlyList<LogFile> FilterFiles(IReadOnlyList<LogFile> files)
        {
            return new List<LogFile>(files.ToList());
        }

        public abstract Task Reload(IReadOnlyList<LogFile> files);

        public virtual void Activate()
        {
        }

        public virtual void Deactivate()
        {
        }

        public async Task ReloadAll(IReadOnlyList<LogFile> files)
        {
            IsLoading = true;
            SetProgress("Reading logs", 0, 1);
            var filteredFiles = FilterFiles(files);
            // load related files
            await Task.Run(() =>
            {
                var progress = 0;
                var total = filteredFiles.Count;
                Parallel.ForEach(filteredFiles, f =>
                {
                    f.Load();
                    SetProgress("Reading logs", progress, total);
                    progress++;
                });
            });
            SetProgress("Building UI");
            PrepareLoad();
            await Reload(filteredFiles);
            IsLoading = false;
        }

        protected virtual void PrepareLoad()
        {
        }
    }
}