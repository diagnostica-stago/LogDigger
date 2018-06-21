using System;

namespace LogDigger.Gui.ViewModels.Core
{
    public struct IndexedDate : IEquatable<IndexedDate>
    {
        public bool Equals(IndexedDate other)
        {
            return _date.Equals(other._date) && _index == other._index;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            return obj is IndexedDate && Equals((IndexedDate)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (_date.GetHashCode() * 397) ^ _index;
            }
        }

        private readonly DateTime _date;
        private readonly int _index;

        public IndexedDate(DateTime date, int index)
        {
            _date = date;
            _index = index;
        }

        public int Index
        {
            get { return _index; }
        }

        public DateTime Date
        {
            get { return _date; }
        }
    }
}