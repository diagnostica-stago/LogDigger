using System.Collections.Generic;
using System.Reactive.Subjects;
using LogDigger.Gui.ViewModels.Core;
using LogDigger.Gui.ViewModels.LogEntries;
using Newtonsoft.Json;
using ReactiveUI;

namespace LogDigger.Gui.ViewModels.Columns
{
    [JsonObject(MemberSerialization.OptIn)]
    public abstract class ACellTemplateVm : AViewModel
    {
        protected ACellTemplateVm()
        {
            StructureChanged = new Subject<string>();
        }

        protected ACellTemplateVm(string fieldName)
            : this()
        {
            FieldName = fieldName;
        }

        public Subject<string> StructureChanged { get; }

        public LogEntryVm Entry
        {
            get => GetProperty<LogEntryVm>();
            set
            {
                if (SetProperty(value))
                {
                    this.RaisePropertyChanged(nameof(FieldName));
                    foreach (var propName in GetEntryDependentProperties())
                    {
                        this.RaisePropertyChanged(propName);
                    }
                }
            }
        }

        protected virtual IEnumerable<string> GetEntryDependentProperties()
        {
            yield break;
        }

        [JsonProperty]
        public string FieldName
        {
            get => GetProperty<string>();
            set
            {
                if (SetProperty(value))
                {
                    StructureChanged.OnNext(nameof(FieldName));
                }
            }
        }

        public IEnumerable<string> AvailableFields
        {
            get => GetProperty<IEnumerable<string>>();
            set => SetProperty(value);
        }

        protected T TryGet<T>(string fieldName)
        {
            if (fieldName == null)
            {
                return default(T);
            }

            object result = null;
            Entry.Data.TryGetValue(fieldName, out result);
            return result is T castedResult ? castedResult : default(T);
        }

        public abstract ACellTemplateVm Clone();
    }
}