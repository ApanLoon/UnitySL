using System.Collections.Generic;

namespace Assets.Scripts.Types
{
    public class MultiMap<K, V>
    {
        protected Dictionary<K, List<V>> Map = new Dictionary<K, List<V>>();

        public void Add(K key, V value)
        {
            if (Map.ContainsKey(key) == false)
            {
                Map[key] = new List<V>();
            }
            Map[key].Add(value);
        }
    }
}
