using System;
using System.Collections.Generic;
using LogDigger.Business.Models;
using LogDigger.Gui.ViewModels.Core;
using Newtonsoft.Json;
using ReactiveUI;

namespace LogDigger.Gui.ViewModels.Columns
{
    [JsonObject(MemberSerialization.OptIn)]
    public class ColumnVm : AViewModel, IColumnDescriptionVm
    {
        public ColumnVm(FieldFormat fieldFormat, string fieldName, IEnumerable<string> availableFields)
        {
            FieldFormat = fieldFormat;
            AvailableFields = availableFields;
            TemplateType = typeof(DefaultCellTemplateVm);
            Name = fieldName;
            IsVisible = true;
        }

        [JsonProperty]
        public string Width
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }

        [JsonProperty]
        public string Name
        {
            get => GetProperty<string>();
            set
            {
                if (SetProperty(value))
                {
                    Template.FieldName = Name;
                }
            }
        }

        [JsonProperty]
        public bool IsVisible
        {
            get => GetProperty<bool>();
            set => SetProperty(value);
        }

        [JsonProperty]
        public bool HasOverlook
        {
            get => GetProperty<bool>();
            set => SetProperty(value);
        }

        [JsonProperty]
        public string FilterType
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }
        
        public IEnumerable<string> FilterTypes { get; } = new []{ "None", "TextFilter", "SimpleListFilter" };

        public Type TemplateType
        {
            get => GetProperty<Type>();
            set
            {
                if (SetProperty(value))
                {
                    Template = value.GetConstructor(Type.EmptyTypes).Invoke(null) as ACellTemplateVm;
                    Template.StructureChanged.Subscribe(x => this.RaisePropertyChanged(nameof(Template)));
                    Template.FieldName = Name;
                    Template.AvailableFields = AvailableFields;
                }
            }
        }

        public TTemplate GenerateTemplate<TTemplate>()
            where TTemplate : ACellTemplateVm
        {
            TemplateType = typeof(TTemplate);
            return (TTemplate)Template;
        }

        [JsonProperty(TypeNameHandling = TypeNameHandling.All)]
        public ACellTemplateVm Template
        {
            get => GetProperty<ACellTemplateVm>();
            private set => SetProperty(value);
        }

        public IEnumerable<Type> AvailableTypes
        {
            get
            {
                yield return typeof(DateCellTemplateVm);
                yield return typeof(DefaultCellTemplateVm);
                yield return typeof(HeaderedCellTemplateVm);
            }
        }

        [JsonProperty]
        public FieldFormat FieldFormat { get; }

        [JsonProperty]
        public bool CanSort
        {
            get => GetProperty<bool>();
            set => SetProperty(value);
        }

        [JsonProperty]
        public bool InsertBefore
        {
            get => GetProperty<bool>();
            set => SetProperty(value);
        }

        public IEnumerable<string> AvailableFields
        {
            get => GetProperty<IEnumerable<string>>();
            set => SetProperty(value);
        }
    }
}