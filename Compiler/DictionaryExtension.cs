using System;
using System.Collections.Generic;
using System.Text;

namespace CodeGen
{
    public static class DictionaryExtension
    {
        public static void AddOrUpdate<TKey, TValue>(
            this IDictionary<TKey, TValue> map, TKey key, TValue value)
        {
            map[key] = value;
        }

        // gets the value if it exists or inserts the entry and returns the value it if it does not
        public static int GetOrAdd (this IDictionary<string, int> map, string key, ref int value)
        {
            if (map.ContainsKey(key))
            {
                return map[key];
            }
            else
            {
                value++;
                map[key] = value;
                return value;
            }
        }
    }
}
