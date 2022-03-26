using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueprintLoader
{
    public static class Extensions
    {
        public static void Deconstruct<K, V>(this KeyValuePair<K, V> pair, out K key, out V val)
        {
            key = pair.Key;
            val = pair.Value;
        }
    }
}
