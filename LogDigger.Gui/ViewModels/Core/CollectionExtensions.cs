using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace LogDigger.Gui.ViewModels.Core
{
    public static class CollectionExtensions
    {
        public static void CopyFrom<TSource, TDestination>(this IList<TDestination> dest, IEnumerable<TSource> src, Func<TSource, TDestination> constructor, Action<TSource, TDestination> destinationValueConvertor)
        {
            var useDestinationValue = destinationValueConvertor != null;
            var srcEnum = src as IList<TSource> ?? src.ToList();
            int srcCount = srcEnum.Count;
            int destCount = dest.Count;
            for (int i = 0; i < srcCount; i++)
            {
                if (destCount - 1 < i)
                {
                    dest.Add(constructor(srcEnum[i]));
                }
                else
                {
                    if (dest[i] != null && useDestinationValue)
                    {
                        destinationValueConvertor(srcEnum[i], dest[i]);
                    }
                    else
                    {
                        dest[i] = constructor(srcEnum[i]);
                    }
                }
                destCount = dest.Count;
            }
            for (int i = srcCount - 1; i < destCount - 1; i++)
            {
                dest.RemoveAt(dest.Count - 1);
            }
        }

        public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> src)
        {
            return new ObservableCollection<T>(src);
        }
    }
}