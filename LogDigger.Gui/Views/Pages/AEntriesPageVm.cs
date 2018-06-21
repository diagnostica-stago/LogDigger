using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using LogDigger.Gui.Properties;
using LogDigger.Gui.ViewModels.Columns;
using LogDigger.Gui.ViewModels.Core;
using LogDigger.Gui.ViewModels.LogEntries;
using LogDigger.Gui.ViewModels.Pages;
using ReactiveUI;

namespace LogDigger.Gui.Views.Pages
{
    public abstract class AEntriesPageVm : AFilterPageVm<LogEntryVm>
    {
        protected AEntriesPageVm(INavigator navigator, IReadOnlyList<IColumnDescriptionVm> columns) : base(navigator)
        {
            Columns = columns;
        }

        public LogEntryVm SelectedItem
        {
            get => GetProperty<LogEntryVm>();
            set => SetProperty(value);
        }

        public IEnumerable SelectedItems
        {
            get => GetProperty<IEnumerable>();
            set => SetProperty(value);
        }

        public ICommand OpenFileCommand => ReactiveCommand.CreateFromTask(CallOpenFile);

        public IReadOnlyList<IColumnDescriptionVm> Columns { get; }

        public event EventHandler<SelectionChangingEventArgs<LogEntryVm>> SelectionChanged;

        private Task CallOpenFile()
        {
            var cmd = UserSettings.Default.TextReaderPath;
            var args = UserSettings.Default.TextReaderArgs.Replace("%f", $"\"{SelectedItem.SourceFile}\"").Replace("%ln", SelectedItem.SourceLineNumber.ToString());
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

        protected virtual void RaiseSelectionChanged(SelectionChangingEventArgs<LogEntryVm> e)
        {
            SelectionChanged?.Invoke(this, e);
        }
    }
}