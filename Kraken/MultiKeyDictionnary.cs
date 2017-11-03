using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Kraken
{
    /// <summary>
    /// l'ordre des 2 clefs n'a pas d'importance
    /// </summary>
    /// <typeparam name="K"></typeparam>
    /// <typeparam name="V"></typeparam>
    internal class InterchangableBiKeyDictionnary<K, V> : Dictionary<Tuple<K, K>, V>
    {

        new internal IEnumerable<V> Values
        {
            get
            {
                lock (this)
                {
                    List<V> l = new List<V>();
                    foreach (var entry in this)
                    {
                        l.Add(entry.Value);
                    }
                    return l;
                }
            }
        }


        [MethodImpl(MethodImplOptions.Synchronized)]
        internal void Add(K key1, K key2, V value)
        {
            Add(Key(key1, key2), value);
            Add(Key(key2, key1), value);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        internal bool ContainsKeys(K key1, K key2)
        {
            return ContainsKey(Key(key1, key2));
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        internal bool TryGetValue(K key1, K key2, out V value)
        {
            return TryGetValue(Key(key1, key2), out value);
        }

        private Tuple<K, K> Key(K key1, K key2)
        {
            return Tuple.Create(key1, key2);
        }
    }

    /// <summary>
    /// la permutation circulaire des 3 clefs n'a pas d'importance
    /// </summary>
    /// <typeparam name="K"></typeparam>
    /// <typeparam name="V"></typeparam>
    internal class CycliqueTripleKeyDictionary<K,V> : Dictionary<Tuple<K,K,K>, V>
    {
        new internal IEnumerable<V> Values
        {
            get
            {
                lock (this)
                {
                    List<V> l = new List<V>();
                    foreach (var entry in this)
                    {
                        l.Add(entry.Value);
                    }
                    return l;
                }
            }
        }
        [MethodImpl(MethodImplOptions.Synchronized)]
        internal void Add(K key1, K key2, K key3, V value)
        {
            Add(Key(key1, key2, key3), value);
            Add(Key(key2, key3, key1), value);
            Add(Key(key3, key1, key2), value);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        internal bool TryGetValue(K key1, K key2, K key3, out V value)
        {
            return TryGetValue(Key(key1, key2, key3), out value);
        }

        private Tuple<K, K, K> Key(K key1, K key2, K key3)
        {
            return Tuple.Create(key1, key2, key3);
        }
    }
}