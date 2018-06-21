using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Threading;
using LogDigger.Gui.ViewModels.Core;
using ReactiveUI;

namespace LogDigger.Gui.Views.Controls
{
    public class FxDataGridColumnVm : AViewModel
    {
        public FxDataGridColumnVm(string header, string binding, int index)
        {
            Index = index;
            Binding = binding;
            Header = header;
        }

        public string Header { get; private set; }

        public string Binding { get; private set; }

        public int Index
        {
            get => GetProperty<int>();
            set
            {
                if (SetProperty(value))
                {
                    this.RaisePropertyChanged(nameof(FormatedIndex));
                }
            }
        }

        public string FormatedIndex
        {
            get { return Index <= 9 ? Index.ToString("0#", CultureInfo.InvariantCulture) : Index.ToString(CultureInfo.InvariantCulture); }
        }

        public int DisplayIndex { get; set; }
    }

    /// <summary>
    /// ListBox with Detail for each item
    /// The control have an attached property (IsDetailsSelector) 
    /// and if this property is set to true on a control of type Selector 
    /// in the items then we can manage a detail for each items of the selector.
    /// </summary>
    public class FxDataGrid : DataGridEx
    {
        // Fields //////////////////////////////////////////////////////////////

        public static readonly DependencyProperty IsShowDetailsControlProperty = DependencyProperty.RegisterAttached("IsShowDetailsControl", typeof(bool), typeof(FxDataGrid), new PropertyMetadata(false));

        public static readonly DependencyProperty ColumnsSourceProperty = DependencyProperty.Register("ColumnsSource", typeof(IEnumerable<FxDataGridColumnVm>), typeof(FxDataGrid), new PropertyMetadata(default(IEnumerable<FxDataGridColumnVm>), (o, args) => ((FxDataGrid)o).ColumnsSourceChanged(args)));

        public static readonly DependencyProperty ColumnHeaderSortProperty = DependencyProperty.Register("ColumnHeaderSort", typeof(string), typeof(FxDataGrid), new PropertyMetadata(default(string)));

        private const int MaxDeep = 30;

        private object _currentDetailsItem;

        // Constructors ////////////////////////////////////////////////////////

        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline", Justification = "Necessary initialization for custom control styling in WPF")]
        static FxDataGrid()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FxDataGrid), new FrameworkPropertyMetadata(typeof(FxDataGrid)));
        }

        public FxDataGrid()
        {
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
            Sorting += SortHandler;
            Language = XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag);
        }

        protected override void OnSorting(DataGridSortingEventArgs eventArgs)
        {
            eventArgs.Handled = false;
            SortHandler(this, eventArgs);
        }

        private void SortHandler(object sender, DataGridSortingEventArgs e)
        {
            var column = e.Column;
            if (column != null)
            {
                ColumnHeaderSort = (string)column.Header;
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.PreviewMouseDown += OnMainWindowPreviewMouseDown;
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.PreviewMouseDown -= OnMainWindowPreviewMouseDown;
        }

        public IEnumerable<FxDataGridColumnVm> ColumnsSource
        {
            get { return (IEnumerable<FxDataGridColumnVm>)GetValue(ColumnsSourceProperty); }
            set { SetValue(ColumnsSourceProperty, value); }
        }
        
        public string ColumnHeaderSort
        {
            get { return (string)GetValue(ColumnHeaderSortProperty); }
            set { SetValue(ColumnHeaderSortProperty, value); }
        }

        private void ColumnsSourceChanged(DependencyPropertyChangedEventArgs args)
        {
            var oldSource = args.OldValue as INotifyCollectionChanged;
            var newSource = args.NewValue as INotifyCollectionChanged;

            if (oldSource != null)
            {
                oldSource.CollectionChanged -= SourceCollectionChanged;
            }

            if (ColumnsSource != null)
            {
                Columns.Clear();

                foreach (var column in ColumnsSource)
                {
                    Columns.Add(
                        new DataGridTextColumn { Header = column.Header, Binding = new Binding(column.Binding) });
                }
            }
            else
            {
                Columns.Clear();
            }

            if (newSource != null)
            {
                newSource.CollectionChanged += SourceCollectionChanged;
            }
        }

        private void SourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var action = new Action(
                () =>
                {
                    switch (e.Action)
                    {
                        case NotifyCollectionChangedAction.Add:
                            foreach (var column in e.NewItems.Cast<FxDataGridColumnVm>())
                            {
                                Columns.Insert(e.NewStartingIndex, new DataGridTextColumn { Header = column.Header, Binding = new Binding(column.Binding) });
                            }
                            break;
                        case NotifyCollectionChangedAction.Remove:
                            foreach (var column in e.OldItems.Cast<FxDataGridColumnVm>())
                            {
                                Columns.RemoveAt(e.OldItems.IndexOf(column));
                            }
                            break;
                        case NotifyCollectionChangedAction.Replace:
                            break;
                        case NotifyCollectionChangedAction.Move:
                            Columns.Move(e.OldStartingIndex, e.NewStartingIndex);

                            var oldColumn = Columns[e.NewStartingIndex];
                            oldColumn.DisplayIndex = e.NewStartingIndex;
                            break;
                        case NotifyCollectionChangedAction.Reset:
                            Columns.Clear();
                            break;
                    }
                });

            if (Dispatcher.CheckAccess())
            {
                action.Invoke();
            }
            else
            {
                Dispatcher.BeginInvoke(action);
            }
        }

        // Methods /////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets if an element is a ShowDetailsControl
        /// </summary>
        public static bool GetIsShowDetailsControl(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsShowDetailsControlProperty);
        }

        /// <summary>
        ///  Sets if an element is a ShowDetailsControl
        /// </summary>
        public static void SetIsShowDetailsControl(DependencyObject obj, bool value)
        {
            obj.SetValue(IsShowDetailsControlProperty, value);
        }

        /// <summary>
        /// Handle MouseDown event :
        ///   - If user clicks on the Details Panel or any control within the Details Panel : do nothing
        ///   - If user clicks on the same item : close the current Details Panel
        ///   - If user clicks on another item : open the new Details Panel
        /// </summary>
        private void OnMainWindowPreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // First we must ensure that Original Source of the click is not a details panel or any control within the Detail
            var source = e.OriginalSource;

            // Runs element is not a FrameworkElement and it is not attached to parent (TextBlock)
            // So we must use the Parent property to retrieve the associated TextBlock element.
            var run = source as Run;
            if (run != null)
            {
                source = run.Parent;
            }

            //Check if a Control of the clicked cell have the attached property set to true.
            //Get the Cell first
            var cell = VisualTreeUtil.GetParent<DataGridCell>(source as FrameworkElement, MaxDeep);
            if (cell != null)
            {
                // and then parse all FrameworkElement
                var l = new List<FrameworkElement>();
                VisualTreeUtil.GetAllChild(cell, l);
                foreach (var item in l)
                {
                    if (GetIsShowDetailsControl(item))
                    {
                        var dataContext = item.DataContext;
                        ShowRowDetails(dataContext);
                        return;
                    }
                }
            }

            var row = VisualTreeUtil.GetParent<DataGridRow>(source as FrameworkElement, MaxDeep);
            // if user clicks on the same item
            if (_currentDetailsItem != null)
            {
                // Check if user clicks on the details or into a Popup (popup are not attached to the same Window)
                var detailsPanOrPopup = row != null ? VisualTreeUtil.GetChild<DataGridDetailsPresenter>(row) : null;
                if (VisualTreeUtil.IsParentOfTypeOrPopup(source as FrameworkElement, detailsPanOrPopup, MaxDeep))
                {
                    return;
                }

                if (row != null && row.DataContext == _currentDetailsItem)
                {
                    e.Handled = true;
                }
            }
            // Unselect the DetailsItem and then close the details panel
            ShowRowDetails(null);
        }

        /// <summary>
        /// Show or Hide the Details panel for the specified item
        /// </summary>
        private void ShowRowDetails(object item)
        {
            // Hide previous opened Details panel
            if (_currentDetailsItem != null)
            {
                var row = (DataGridRow)ItemContainerGenerator.ContainerFromItem(_currentDetailsItem);
                if (row != null)
                {
                    row.DetailsVisibility = Visibility.Collapsed;
                }
            }

            // If user clicks on the same item, close details panel
            if (item == _currentDetailsItem)
            {
                _currentDetailsItem = null;
            }
            else
            {
                _currentDetailsItem = item;

                // If users click on an item :
                // - Open Detail Panel of the row
                // - Ensure that item is visible by scrolling to the item
                if (_currentDetailsItem != null)
                {
                    var row = (DataGridRow)ItemContainerGenerator.ContainerFromItem(_currentDetailsItem);
                    if (row != null)
                    {
                        row.DetailsVisibility = Visibility.Visible;
                        Dispatcher.BeginInvoke(new Action(() => ScrollIntoView(row.DataContext)),
                        DispatcherPriority.Normal,
                        null);
                    }
                }
            }
        }

        protected override void OnRowDetailsVisibilityChanged(DataGridRowDetailsEventArgs e)
        {
            base.OnRowDetailsVisibilityChanged(e);

            if (ItemsSource == null)
            {
                return;
            }

            if (e.Row != null)
            {
                foreach (var row in ItemsSource.Cast<object>().Select(item => this.ItemContainerGenerator.ContainerFromItem(item) as DataGridRow).Where(row => row != null && row != e.Row))
                {
                    row.Opacity = 0.5;
                }
                e.Row.Opacity = 1;
            }
            else
            {
                foreach (var row in (from object item in ItemsSource select this.ItemContainerGenerator.ContainerFromItem(item)).OfType<DataGridRow>())
                {
                    row.Opacity = 1;
                }
            }
        }
    }
}
