using System;
using System.Globalization;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using LogDigger.Gui.ViewModels.Columns;
using LogDigger.Gui.ViewModels.LogEntries;
using ReactiveUI;

namespace LogDigger.Gui.Views.Behaviors
{
    public class CustomBoundColumn : DataGridTemplateColumn//DataGridBoundColumn
    {
        private readonly IColumnDescriptionVm _columnDescription;

        public CustomBoundColumn(IColumnDescriptionVm columnDescription)
        {
            _columnDescription = columnDescription;
            _columnDescription
                .WhenAnyValue(x => x.IsVisible)
                .ObserveOnDispatcher()
                .Subscribe(isVisible => Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed);
            _columnDescription
                .WhenAnyValue(x => x.Template)
                .ObserveOnDispatcher()
                .Subscribe(t =>
                {
                    if (DataGridOwner == null)
                    {
                        return;
                    }

                    var cellTemplate = DataGridOwner.FindResource(new DataTemplateKey(t.GetType())) as DataTemplate;
                    CellTemplate = cellTemplate;
                });
            _columnDescription
                .WhenAnyValue(x => x.HasOverlook)
                .ObserveOnDispatcher()
                .Subscribe(val =>
                {
                    if (DataGridOwner == null)
                    {
                        return;
                    }

                    if (val)
                    {
                        CellStyle = DataGridOwner.FindResource("OverlookCellStyle") as Style;
                    }
                    else
                    {
                        CellStyle = DataGridOwner.FindResource("LogDataGridCellStyle") as Style;
                    }
                });
        }

        public new DataTemplate CellTemplate { get; set; }
        public new DataTemplate CellEditingTemplate { get; set; }
        public MappedValueCollection MappedValueCollection { get; set; }

        protected override FrameworkElement GenerateElement(DataGridCell cell, object dataItem)
        {
//            var content = new ContentControl();
//            var bindingPath = (cell.Column.Header as IColumnDescription)?.Binding;
//            var binding = new Binding { Source = dataItem, Path = new PropertyPath(bindingPath) };
//            content.ContentTemplate = cell.IsEditing ? CellEditingTemplate : CellTemplate;
//            content.SetBinding(ContentControl.ContentProperty, binding);
//            return content;

            base.GenerateElement(cell, dataItem);
            ChooseCellTemplateAndSelector(false, out var template, out var templateSelector);
            if (template == null && templateSelector == null)
            {
                return null;
            }
            var contentPresenter = new ContentPresenter();
            var columnDesc = cell.Column.Header as IColumnDescriptionVm;
            BindingOperations.SetBinding(contentPresenter, ContentPresenter.ContentProperty, new Binding { Converter = new CustomBoundColumnConverter(() => columnDesc.Template.Clone()) });
            contentPresenter.ContentTemplate = template;
            contentPresenter.ContentTemplateSelector = templateSelector;
            return contentPresenter;
        }

        protected override FrameworkElement GenerateEditingElement(DataGridCell cell, object dataItem)
        {
            return GenerateElement(cell, dataItem);
        }

        private void ChooseCellTemplateAndSelector(bool isEditing, out DataTemplate template, out DataTemplateSelector templateSelector)
        {
            template = null;
            templateSelector = null;
            if (isEditing)
            {
                template = CellEditingTemplate;
                templateSelector = CellEditingTemplateSelector;
            }

            if (template != null || templateSelector != null)
            {
                return;
            }
            template = CellTemplate;
            templateSelector = CellTemplateSelector;
        }
    }

    public class CustomBoundColumnConverter : IValueConverter
    {
        private readonly Func<ACellTemplateVm> _cellTemplateContainer;

        public CustomBoundColumnConverter(Func<ACellTemplateVm> cellTemplateContainer)
        {
            _cellTemplateContainer = cellTemplateContainer;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var template = _cellTemplateContainer.Invoke();
            template.Entry = value as LogEntryVm;
            return template;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}