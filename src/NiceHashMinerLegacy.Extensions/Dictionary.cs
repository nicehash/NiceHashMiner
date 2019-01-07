using System;
using System.Collections.Generic;
using System.Linq;

namespace NiceHashMinerLegacy.Extensions
{
    public static class Dictionary
    {
        public static Dictionary<TKey, TValue> ConcatDict<TKey, TValue>(
            this Dictionary<TKey, TValue> target, 
            params Dictionary<TKey, TValue>[] sources)
        {
            return target.ConcatDict(sources.AsEnumerable());
        }

        public static Dictionary<TKey, TValue> ConcatDict<TKey, TValue>(
            this Dictionary<TKey, TValue> target,
            IEnumerable<Dictionary<TKey, TValue>> sources)
        {
            return sources.Aggregate(target, (current, source) => current.Concat(source).ToDictionary(x => x.Key, x => x.Value));
        }

        public static Dictionary<TKey, List<TValue>> ConcatDictList<TKey, TValue>(
            this Dictionary<TKey, List<TValue>> target,
            params Dictionary<TKey, List<TValue>>[] sources)
        {
            return target.ConcatDictList(sources.AsEnumerable());
        }

        public static Dictionary<TKey, List<TValue>> ConcatDictList<TKey, TValue>(
            this Dictionary<TKey, List<TValue>> target,
            IEnumerable<Dictionary<TKey, List<TValue>>> sources)
        {
            if (sources == null)
                throw new ArgumentNullException($"{nameof(sources)} cannot be null");

            var ret = new Dictionary<TKey, List<TValue>>(target);

            foreach (var source in sources)
            {
                foreach (var key in source.Keys)
                {
                    ret.TryGetValue(key, out var list);
                    ret[key] = list?.Concat(source[key]).ToList() ?? source[key];
                }
            }

            return ret;
        }
    }
}
