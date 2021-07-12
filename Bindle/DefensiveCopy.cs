using System.Collections.Generic;
using System.Linq;

namespace Deislabs.Bindle
{
    internal static class DefensiveCopy
    {
        internal static IReadOnlyList<T> Create<T>(IEnumerable<T> source)
        {
            return new List<T>(source).AsReadOnly();
        }

        internal static IReadOnlyDictionary<TKey, TValue> Create<TKey, TValue>(
            IDictionary<TKey, TValue> source
        )
        where TKey: notnull
        {
            return new Dictionary<TKey, TValue>(source);
        }

        internal static IReadOnlyDictionary<TKey1, IReadOnlyDictionary<TKey2, TValue>> Create<TKey1, TKey2, TValue>(
            IDictionary<TKey1, IDictionary<TKey2, TValue>> source
        )
        where TKey1 : notnull
        where TKey2 : notnull
        {
            return new Dictionary<TKey1, IReadOnlyDictionary<TKey2, TValue>>(
                source.Select(kvp => KeyValuePair.Create(
                    kvp.Key, DefensiveCopy.Create(kvp.Value)
                ))
            );
        }
    }
}