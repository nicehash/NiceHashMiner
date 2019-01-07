using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NiceHashMinerLegacy.Extensions
{
    public static class EnumerableExt
    {
        public static bool Same<T>(this IEnumerable<T> source, IEnumerable<T> other)
        {
            return source.All(other.Contains);
        }
    }
}
