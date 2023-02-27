using Panama.Core.Interfaces;
using System.Collections.Generic;

namespace Panama.Core.Models
{
    public class Kvp<K,V> : IModel
    {
        private readonly KeyValuePair<K, V> _current;

        public Kvp(K key, V value) 
        {
            _current = new KeyValuePair<K, V>(key, value);
        }
        
        public K Key  => _current.Key;
        public V Value => _current.Value;
    }
}
