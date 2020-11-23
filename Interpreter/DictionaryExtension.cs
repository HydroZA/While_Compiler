using System;
using System.Collections.Generic;
using System.Text;

namespace Interpreter
{
    public static class DictionaryExtension
    {
        public static void AddOrUpdate<TKey, TValue>(
            this IDictionary<TKey, TValue> map, TKey key, TValue value)
        {
            map[key] = value;
        }
    }
}
