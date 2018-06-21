using System;
using System.Collections;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace LogDigger.Gui.Views.Controls
{
    /// <summary>
    /// Base class extended from the DataGrid control.
    /// The class manage the display of the data after a duration correpsonding to an animation.
    /// </summary>
    public class DataGridEx : DataGrid
    {
        public static readonly DependencyProperty MultiSelectionContextMenuProperty = DependencyProperty.Register(
            "MultiSelectionContextMenu", typeof(ContextMenu), typeof(DataGridEx), new PropertyMetadata(default(ContextMenu)));

        public static readonly DependencyProperty SingleSelectionContextMenuProperty = DependencyProperty.Register(
            "SingleSelectionContextMenu", typeof(ContextMenu), typeof(DataGridEx), new PropertyMetadata(default(ContextMenu)));

        public static readonly DependencyProperty SelectedItemsProperty =
            DependencyProperty.Register("SelectedItems", typeof(IEnumerable), typeof(DataGridEx), new PropertyMetadata(default(IEnumerable)));

        public new IEnumerable SelectedItems
        {
            get { return (IEnumerable)GetValue(SelectedItemsProperty); }
            set { throw new Exception("This property is read-only. To bind to it you must use 'Mode=OneWayToSource'."); }
        }

        public ContextMenu SingleSelectionContextMenu
        {
            get { return (ContextMenu) GetValue(SingleSelectionContextMenuProperty); }
            set { SetValue(SingleSelectionContextMenuProperty, value); }
        }

        public ContextMenu MultiSelectionContextMenu
        {
            get { return (ContextMenu) GetValue(MultiSelectionContextMenuProperty); }
            set { SetValue(MultiSelectionContextMenuProperty, value); }
        }

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            base.OnSelectionChanged(e);
            SetValue(SelectedItemsProperty, base.SelectedItems);
        }

        protected override void OnContextMenuOpening(ContextMenuEventArgs e)
        {
            if (SelectedItems?.Cast<object>().Count() > 1)
            {
                ContextMenu = MultiSelectionContextMenu;
            }
            else
            {
                ContextMenu = SingleSelectionContextMenu;
            }
            base.OnContextMenuOpening(e);
        }
    }
}