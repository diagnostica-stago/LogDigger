using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LogDigger.Business.Utils;

namespace LogDigger.Gui.ViewModels.Filters
{
    public class DateFilterVm<TItem> : AFilterItemVm<TItem>
    {
        private readonly Func<TItem, DateTime> _dateAccessor;

        public DateFilterVm(Func<TItem, DateTime> dateAccessor)
            : base("Date")
        {
            _dateAccessor = dateAccessor;
            IncludeUnknownDate = false;
        }

        public long LowerDateTick
        {
            get => GetProperty<long>();
            set => SetProperty(value);
        }

        public long UpperDateTick
        {
            get => GetProperty<long>();
            set => SetProperty(value);
        }

        public long MaximumDateTick
        {
            get => GetProperty<long>();
            set => SetProperty(value);
        }

        public long MinimumDateTick
        {
            get => GetProperty<long>();
            set => SetProperty(value);
        }

        public bool IncludeUnknownDate
        {
            get => GetProperty<bool>();
            set => SetProperty(value);
        }

        public override Task InitFilter(IEnumerable<TItem> list)
        {
            MinimumDateTick = list.Where(e => _dateAccessor(e) != DateTime.MinValue).Select(e => _dateAccessor(e).Ticks).MinOrDefault(0);
            LowerDateTick = MinimumDateTick;
            var maxDate = list.Select(e => _dateAccessor(e).Ticks).MaxOrDefault(DateTime.MaxValue.Ticks);
            MaximumDateTick = maxDate;
            UpperDateTick = MaximumDateTick;
            return Task.CompletedTask;
        }

        protected override bool FilterItem(TItem entry)
        {
            return IsInRange(_dateAccessor(entry), new DateTime(LowerDateTick), new DateTime(UpperDateTick), IncludeUnknownDate);
        }
        
        private bool IsInRange(DateTime date, DateTime lowerBound, DateTime upperBound, bool includeUnknownDates)
        {
            var lower = lowerBound > DateTime.MinValue ? lowerBound.AddSeconds(-1) : lowerBound;
            var upper = upperBound < DateTime.MaxValue ? upperBound.AddSeconds(1) : upperBound;
            if (lowerBound.Equals(upperBound) && lowerBound.Equals(date))
            {
                return true;
            }
            return date > lower && date < upper || (DateTime.MinValue == date && includeUnknownDates);
        }
    }
}