using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Zony.Lib.Plugin.Common.Extensions
{
    public static class EnumerableExtensions
    {
        public static IList<T> ToList<T>(this ConcurrentBag<T> bag)
        {
            IList<T> list = new List<T>();
            foreach (var item in bag)
            {
                list.Add(item);
            }
            return list;
        }
    }
}
