using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GelDesk
{
    public static class CollectionExtensions
    {
        #region IDictionary<TKey, TValue>
        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue defaultValue = default(TValue))
        {
            TValue value;
            if (!dict.TryGetValue(key, out value))
                return defaultValue;
            else
                return value;
        }

        public static bool SetValueRemoveDefault<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue value,
            TValue defaultValue = default(TValue))
        {
            if (EqualityComparer<TValue>.Default.Equals(value, defaultValue))
            {
                dict.Remove(key);
                return true;
            }
            dict[key] = value;
            return false;
        }
        #endregion
    }
}
