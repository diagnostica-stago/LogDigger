using System.Collections.Generic;
using System.Linq;

namespace LogDigger.Business.Utils
{
    public static class EnumerableExtensions
    {
        public static long MinOrDefault(this IEnumerable<long> source, long defaultValue)
        {
            if (!source.Any())
            {
                return defaultValue;
            }
            return source.Min();
        }

        public static long MaxOrDefault(this IEnumerable<long> source, long defaultValue)
        {
            if (!source.Any())
            {
                return defaultValue;
            }
            return source.Max();
        }

        public static long? MinOrNull(this IEnumerable<long> source)
        {
            if (!source.Any())
            {
                return null;
            }
            return source.Min();
        }

        public static long? MaxOrNull(this IEnumerable<long> source)
        {
            if (!source.Any())
            {
                return null;
            }
            return source.Max();
        }
    }
}