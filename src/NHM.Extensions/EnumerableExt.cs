using System.Collections.Generic;
using System.Linq;

namespace NHM.Extensions
{
    public static class EnumerableExt
    {
        public static bool Same<T>(this IEnumerable<T> source, IEnumerable<T> other)
        {
            return source.All(other.Contains);
        }

        public static Queue<T> ToQueue<T>(this IEnumerable<T> source)
        {
            var queue = new Queue<T>();
            foreach (var el in source)
            {
                queue.Enqueue(el);
            }
            return queue;
        }
    }
}
