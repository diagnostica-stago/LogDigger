using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using LogDigger.Business.Models;
using LogDigger.Business.Services;
using LogDigger.Gui.ViewModels.Core;
using Microsoft.Win32;
using ReactiveUI;

namespace LogDigger.Gui.ViewModels.Pages.Summaries
{
    public class TemplatesEditionPageVm : APageVm, ICloseable
    {
        private LogTemplateService _templateService;

        public TemplatesEditionPageVm(INavigator navigator, LogTemplateService templateService) : base(navigator)
        {
            _templateService = templateService;
            SaveCommand = ReactiveCommand.Create(CallSave);
            ImportCommand = ReactiveCommand.Create(CallImport);
            ExportCommand = ReactiveCommand.Create(CallExport);
            AddItemCommand = ReactiveCommand.Create(CallAddItem);
            DeleteItemCommand = ReactiveCommand.Create(CallDeleteItem);
            MoveUpCommand = ReactiveCommand.Create(CallMoveUp);
            MoveDownCommand = ReactiveCommand.Create(CallMoveDown);
            Task.Run(() =>
            {
                var templates = _templateService.GetTemplates();
                Templates = new ObservableCollection<CustomLogTemplateVm>(templates.Select(x => new CustomLogTemplateVm(x)));
            });
        }

        private void CallExport()
        {
            var saveFileDialog = new SaveFileDialog();
            string filePath = string.Empty;
            if (saveFileDialog.ShowDialog() == true)
            {
                filePath = saveFileDialog.FileName;
                Task.Run(() => _templateService.SaveTemplate(Templates.Select(x => x.Template).ToList(), filePath, false));
            }
        }

        private void CallImport()
        {
            var openFileDialog = new OpenFileDialog();
            string filePath = string.Empty;
            if (openFileDialog.ShowDialog() == true)
            {
                filePath = openFileDialog.FileName;
                Task.Run(() =>
                {
                    var result = _templateService.LoadTemplates(filePath);
                    Templates = new ObservableCollection<CustomLogTemplateVm>(result.Select(x => new CustomLogTemplateVm(x)));
                });
            }
        }

        private void CallMoveDown()
        {
            if (SelectedItem != null && Templates.Contains(SelectedItem))
            {
                var currentIndex = Templates.IndexOf(SelectedItem);
                if (currentIndex < Templates.Count - 1)
                {
                    Templates.Move(currentIndex, currentIndex + 1);
                }
            }
        }

        private void CallMoveUp()
        {
            if (SelectedItem != null && Templates.Contains(SelectedItem))
            {
                var currentIndex = Templates.IndexOf(SelectedItem);
                if (currentIndex > 0)
                {
                    Templates.Move(currentIndex, currentIndex - 1);
                }
            }
        }

        private void CallDeleteItem()
        {
            if (SelectedItem != null && Templates.Contains(SelectedItem))
            {
                Templates.Remove(SelectedItem);
            }
        }

        private void CallAddItem()
        {
            Templates.Add(new CustomLogTemplateVm(new CustomLogTemplate()));
        }

        private void CallSave()
        {
            Task.Run(() =>
            {
                _templateService.SaveTemplate(Templates.Select(x => x.Template).ToList());
            });
        }

        public override string Title => "Template edition";

        public ObservableCollection<CustomLogTemplateVm> Templates
        {
            get => GetProperty<ObservableCollection<CustomLogTemplateVm>>();
            set => SetProperty(value);
        }

        public CustomLogTemplateVm SelectedItem
        {
            get => GetProperty<CustomLogTemplateVm>();
            set => SetProperty(value);
        }

        public ICommand ImportCommand { get; }
        public ICommand ExportCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand AddItemCommand { get; }
        public ICommand DeleteItemCommand { get; }
        public ICommand MoveUpCommand { get; }
        public ICommand MoveDownCommand { get; }

        public override Task Reload(IReadOnlyList<LogFile> files)
        {
            return Task.CompletedTask;
        }

        public void Close()
        {
        }
    }
}