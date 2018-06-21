using System.Collections.ObjectModel;
using System.Linq;

namespace LogDigger.Gui.Views.Behaviors
{
    public class MappedValueCollection : ObservableCollection<MappedValue>
    {
        public MappedValueCollection()
        {
        }

        public bool Exist(object columnBinding, object rowBinding)
        {
            return this.Count(x => x.RowBinding == rowBinding &&
                                   x.ColumnBinding == columnBinding) > 0;
        }

        public MappedValue ReturnIfExistAddIfNot(object columnBinding, object rowBinding)
        {
            MappedValue value = null;

            if (Exist(columnBinding, rowBinding))
            {
                return this.Single(x => x.RowBinding == rowBinding && x.ColumnBinding == columnBinding);
            }

            value = new MappedValue();
            value.ColumnBinding = columnBinding;
            value.RowBinding = rowBinding;
            Add(value);

            return value;
        }

        public void RemoveByColumn(object columnBinding)
        {
            foreach (var item in this.Where(x => x.ColumnBinding == columnBinding).ToList())
                Remove(item);
        }

        public void RemoveByRow(object rowBinding)
        {
            foreach (var item in this.Where(x => x.RowBinding == rowBinding).ToList())
                Remove(item);
        }
    }
}