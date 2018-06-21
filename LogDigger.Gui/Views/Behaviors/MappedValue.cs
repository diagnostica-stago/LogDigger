using LogDigger.Gui.ViewModels.Core;

namespace LogDigger.Gui.Views.Behaviors
{
    public class MappedValue : AViewModel
    {
        public object ColumnBinding { get; set; }
        public object RowBinding { get; set; }

        public object Value
        {
            get => GetProperty<object>();
            set => SetProperty(value);
        }
    }
}