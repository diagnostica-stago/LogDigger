using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using LogDigger.Gui.ViewModels.Columns;

namespace LogDigger.Gui.Views.Behaviors
{
    public class AttachedColumnBehavior
    {
        public static readonly DependencyProperty AttachedColumnsProperty =
            DependencyProperty.RegisterAttached("AttachedColumns",
                typeof(IEnumerable),
                typeof(AttachedColumnBehavior),
                new UIPropertyMetadata(null, OnAttachedColumnsPropertyChanged));

        public static readonly DependencyProperty MappedValuesProperty =
            DependencyProperty.RegisterAttached("MappedValues",
                typeof(MappedValueCollection),
                typeof(AttachedColumnBehavior),
                new UIPropertyMetadata(null, OnMappedValuesPropertyChanged));

        public static readonly DependencyProperty HeaderTemplateProperty =
            DependencyProperty.RegisterAttached("HeaderTemplate",
                typeof(DataTemplate),
                typeof(AttachedColumnBehavior),
                new UIPropertyMetadata(null, OnHeaderTemplatePropertyChanged));

        public static readonly DependencyProperty AttachedCellTemplateProperty =
            DependencyProperty.RegisterAttached("AttachedCellTemplate",
                typeof(DataTemplate),
                typeof(AttachedColumnBehavior),
                new UIPropertyMetadata(null, OnCellTemplatePropertyChanged));

        public static readonly DependencyProperty AttachedCellEditingTemplateProperty =
            DependencyProperty.RegisterAttached("AttachedCellEditingTemplate",
                typeof(DataTemplate),
                typeof(DataGrid),
                new UIPropertyMetadata(null, OnCellEditingTemplatePropertyChanged));

        public static readonly DependencyProperty SortingHeaderTemplateProperty = DependencyProperty.RegisterAttached(
            "SortingHeaderTemplate", typeof(DataTemplate), typeof(AttachedColumnBehavior), new PropertyMetadata(default(DataTemplate)));

        public static void SetSortingHeaderTemplate(DependencyObject element, DataTemplate value)
        {
            element.SetValue(SortingHeaderTemplateProperty, value);
        }

        public static DataTemplate GetSortingHeaderTemplate(DependencyObject element)
        {
            return (DataTemplate) element.GetValue(SortingHeaderTemplateProperty);
        }

        private static void OnAttachedColumnsPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            if (!(dependencyObject is DataGrid dataGrid))
            {
                return;
            }

            if (!(e.NewValue is INotifyCollectionChanged columns))
            {
                return;
            }

            columns.CollectionChanged += (sender, args) =>
            {
                if (args.Action == NotifyCollectionChangedAction.Remove)
                {
                    RemoveColumns(dataGrid, args.OldItems);
                }
                else if (args.Action == NotifyCollectionChangedAction.Add)
                {
                    AddColumns(dataGrid, args.NewItems);
                }
                else if (args.Action == NotifyCollectionChangedAction.Reset)
                {
                    RemoveAllColumns(dataGrid);
                }
            };
            dataGrid.Loaded += (sender, args) => AddColumns(dataGrid, GetAttachedColumns(dataGrid));
            var items = dataGrid.ItemsSource as INotifyCollectionChanged;
            if (items != null)
            {
                items.CollectionChanged += (sender, args) =>
                {
                    if (args.Action == NotifyCollectionChangedAction.Remove)
                    {
                        RemoveMappingByRow(dataGrid, args.NewItems);
                    }
                };
            }
        }

        private static void RemoveAllColumns(DataGrid dataGrid)
        {
            var columns = new List<DataGridColumn>(dataGrid.Columns.OfType<CustomBoundColumn>());
            foreach (var col in columns)
            {
                dataGrid.Columns.Remove(col);
            }
        }

        private static void AddColumns(DataGrid dataGrid, IEnumerable columns)
        {
            foreach (var column in columns)
            {
                var desc = column as IColumnDescriptionVm;
                var template = desc?.Template;
                var cellTemplate = dataGrid.FindResource(new DataTemplateKey(template.GetType())) as DataTemplate;
                var dataGridLengthConverter = new DataGridLengthConverter();
                var customBoundColumn = new CustomBoundColumn(desc)
                {
                    Header = column,
                    CellTemplate = cellTemplate,
                    CellEditingTemplate = GetAttachedCellEditingTemplate(dataGrid),
                    // MappedValueCollection = GetMappedValues(dataGrid),
                };
                if (desc.Width != null)
                {
                    customBoundColumn.Width = (DataGridLength) dataGridLengthConverter.ConvertFrom(null, CultureInfo.InvariantCulture, desc.Width);
                }
                else
                {
                    customBoundColumn.Width = new DataGridLength(1, DataGridLengthUnitType.Star);
                }
                if (desc.CanSort)
                {
                    customBoundColumn.HeaderTemplate = GetSortingHeaderTemplate(dataGrid);
                }
                else
                {
                    customBoundColumn.HeaderTemplate = GetHeaderTemplate(dataGrid);
                }
                if (desc.HasOverlook)
                {
                    customBoundColumn.CellStyle = dataGrid.FindResource("OverlookCellStyle") as Style;
                }

                if (desc.InsertBefore)
                {
                    dataGrid.Columns.Insert(dataGrid.FrozenColumnCount, customBoundColumn);
                }
                else
                {
                    dataGrid.Columns.Add(customBoundColumn);
                }
            }
        }

        private static void RemoveColumns(DataGrid dataGrid, IEnumerable columns)
        {
            if (columns == null)
            {
                return;
            }

            foreach (var column in columns)
            {
                DataGridColumn col = dataGrid.Columns.Where(x => x.Header == column).Single();
                GetMappedValues(dataGrid).RemoveByColumn(column);
                dataGrid.Columns.Remove(col);
            }
        }

        private static void RemoveMappingByRow(DataGrid dataGrid, IEnumerable rows)
        {
            foreach (var row in rows)
            {
                GetMappedValues(dataGrid).RemoveByRow(row);
            }
        }

        private static void OnCellTemplatePropertyChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs e)
        {

        }
        private static void OnHeaderTemplatePropertyChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs e)
        {

        }

        private static void OnCellEditingTemplatePropertyChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs e)
        {

        }
        private static void OnMappedValuesPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

        }


        public static IEnumerable GetAttachedColumns(DependencyObject dataGrid)
        {
            return (IEnumerable)dataGrid.GetValue(AttachedColumnsProperty);
        }

        public static void SetAttachedColumns(DependencyObject dataGrid, IEnumerable value)
        {
            dataGrid.SetValue(AttachedColumnsProperty, value);
        }

        public static MappedValueCollection GetMappedValues(DependencyObject dataGrid)
        {
            return (MappedValueCollection)dataGrid.GetValue(MappedValuesProperty);
        }

        public static void SetMappedValues(DependencyObject dataGrid, MappedValueCollection value)
        {
            dataGrid.SetValue(MappedValuesProperty, value);
        }

        public static DataTemplate GetHeaderTemplate(DependencyObject dataGrid)
        {
            return (DataTemplate)dataGrid.GetValue(HeaderTemplateProperty);
        }

        public static void SetHeaderTemplate(DependencyObject dataGrid, DataTemplate value)
        {
            dataGrid.SetValue(HeaderTemplateProperty, value);
        }

        public static DataTemplate GetAttachedCellTemplate(DependencyObject dataGrid)
        {
            return (DataTemplate)dataGrid.GetValue(AttachedCellTemplateProperty);
        }

        public static void SetAttachedCellTemplate(DependencyObject dataGrid, DataTemplate value)
        {
            dataGrid.SetValue(AttachedCellTemplateProperty, value);
        }

        public static DataTemplate GetAttachedCellEditingTemplate(DependencyObject dataGrid)
        {
            return (DataTemplate)dataGrid.GetValue(AttachedCellEditingTemplateProperty);
        }

        public static void SetAttachedCellEditingTemplate(DependencyObject dataGrid, DataTemplate value)
        {
            dataGrid.SetValue(AttachedCellEditingTemplateProperty, value);
        }
    }
}