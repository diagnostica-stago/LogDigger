using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using LogDigger.Gui.Properties;
using LogDigger.Gui.ViewModels.Core;
using ReactiveUI;

namespace LogDigger.Gui.ViewModels.Settings
{
    public class SettingsVm : AModalVm
    {
        private readonly IModalHandler _modalHandler;

        public SettingsVm(IModalHandler modalHandler)
        {
            _modalHandler = modalHandler;
            NotepadPath = UserSettings.Default.TextReaderPath;
            AllSortDirections = Enum.GetValues(typeof(ListSortDirection)).Cast<ListSortDirection>().ToList();
            OpenLogFileCommand = ReactiveCommand.CreateFromTask(CallOpenLogFile);
            OpenParentFolderCommand = ReactiveCommand.Create(CallOpenParentFolder);
            CopyLogPathCommand = ReactiveCommand.Create(CallCopyLogPath);
            SubmitBugReportCommand = ReactiveCommand.Create(CallSubmitBugReport);
            ShowChangeLogCommand = ReactiveCommand.Create(CallShowChangeLog);
        }

        private void CallShowChangeLog()
        {
            _modalHandler.CloseCurrent();
            _modalHandler.OpenModal(new ChangeLogVm());
        }

        private void CallOpenParentFolder()
        {
            Process.Start(Directory.GetParent(AppUtils.LogFilePath).FullName); ;
        }

        private void CallCopyLogPath()
        {
            Clipboard.SetText(AppUtils.LogFilePath);
        }

        private void CallSubmitBugReport()
        {
            Process.Start("http://frgit01.stago.grp/yardinj/LogDigger/issues"); ;
        }

        private Task CallOpenLogFile()
        {
            var cmd = UserSettings.Default.TextReaderPath;
            var args = UserSettings.Default.TextReaderArgs.Replace("%f", $"\"{AppUtils.LogFilePath}\"").Replace("%ln", string.Empty);
            if (!File.Exists(cmd))
            {
                GlobalMessageQueue.Current.Enqueue($@"The path ""{cmd}"" does not exist on your computer. Change it in the settings menu.");
            }
            else
            {
                Process.Start(cmd, args);
            }
            return Task.CompletedTask;
        }

        public ListSortDirection DateSortDirection
        {
            get { return UserSettings.Default.DateSortDirection; }
            set
            {
                UserSettings.Default.DateSortDirection = value;
                UserSettings.Default.Save();
            }
        }

        public string NotepadPath
        {
            get { return UserSettings.Default.TextReaderPath; }
            set
            {
                UserSettings.Default.TextReaderPath = value;
                UserSettings.Default.Save();
            }
        }

        public IList<ListSortDirection> AllSortDirections { get; }

        public ICommand OpenLogFileCommand { get; }

        public ICommand OpenParentFolderCommand { get; }

        public ICommand CopyLogPathCommand { get; }

        public ICommand SubmitBugReportCommand { get; }

        public ICommand ShowChangeLogCommand { get; }
    }
}