using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using LogDigger.Gui.ViewModels.Core;
using LogDigger.Gui.ViewModels.LogEntries;

namespace LogDigger.Gui.ViewModels.Filters
{
    /// <summary>
    /// Experimentation => not used yet.
    /// => to move in a branch
    /// </summary>
    public class EntryFilterVm : AViewModel
    {
        public event EventHandler FilterChanged;
        
        public void SetModulesFromString(IEnumerable<string> availableModules)
        {
            Modules = availableModules.Select(m => new SelectableVm(m, true)).ToList();
            var obs = Observable.FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>(h => Modules[0].PropertyChanged += h, h => Modules[0].PropertyChanged -= h);
            foreach (var moduleVm in Modules.Skip(1))
            {
                var newObs = Observable.FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>(h => moduleVm.PropertyChanged += h, h => moduleVm.PropertyChanged -= h);
                obs = obs.Merge(newObs);
            }
            obs.Throttle(TimeSpan.FromMilliseconds(1000)).Select(e => e.EventArgs).Subscribe(OnModulePropertyChanged);
        }

        private void OnModulePropertyChanged(PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsActive")
            {
                RaiseFilterChanged();
            }
        }

        public List<SelectableVm> Modules
        {
            get => GetProperty<List<SelectableVm>>();
            set => SetProperty(value);
        }

        public List<string> SelectedModuleNames
        {
            get { return Modules.Where(m => m.IsActive ?? true).Select(m => m.Name).ToList(); }
        }

        protected virtual void RaiseFilterChanged()
        {
            Task.Factory.StartNew(
                () =>
                {
                    FilterChanged?.Invoke(this, EventArgs.Empty);
                });
        }

        public bool Match(LogEntryVm logEntryVm)
        {
            if (Modules == null || !Modules.Any())
            {
                return true;
            }
            return ActiveModuleNames.Contains(logEntryVm.Module);
        }

        public IEnumerable<string> ActiveModuleNames
        {
            get { return Modules.Where(m => m.IsActive ?? true).Select(m => m.Name); }
        }
    }
}