using System;
using System.Collections;
using System.Collections.Generic;

namespace CoreSharp.Breeze.Metadata
{
    public class MetadataDictionary : MetadataDictionary<object>
    {
        public MetadataDictionary()
        {
        }

        public MetadataDictionary(Dictionary<string, object> dict) : base(dict)
        {
        }
    }

    public class MetadataDictionary<TType> : IDictionary<string, TType>, IDictionary
    {
        public MetadataDictionary()
        {
            OriginalDictionary = new Dictionary<string, TType>();
        }

        public MetadataDictionary(Dictionary<string, TType> dict)
        {
            OriginalDictionary = dict;
        }

        public Dictionary<string, TType> OriginalDictionary { get; }

        public TType this[string key]
        {
            get { return OriginalDictionary[key]; }
            set { OriginalDictionary[key] = value; }
        }

        #region Interface implementations

        void IDictionary.Clear()
        {
            OriginalDictionary.Clear();
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return OriginalDictionary.GetEnumerator();
        }

        public void Remove(object key)
        {
            ((IDictionary)OriginalDictionary).Remove(key);
        }

        object IDictionary.this[object key]
        {
            get { return ((IDictionary)OriginalDictionary)[key]; }
            set { ((IDictionary) OriginalDictionary)[key] = value; }
        }

        IEnumerator<KeyValuePair<string, TType>> IEnumerable<KeyValuePair<string, TType>>.GetEnumerator()
        {
            return OriginalDictionary.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return OriginalDictionary.GetEnumerator();
        }

        public void Add(KeyValuePair<string, TType> item)
        {
            ((ICollection<KeyValuePair<string, TType>>)OriginalDictionary).Add(item);
        }

        public bool Contains(object key)
        {
            return ((IDictionary) OriginalDictionary).Contains(key);
        }

        public void Add(object key, object value)
        {
            ((IDictionary)OriginalDictionary).Add(key, value);
        }

        void ICollection<KeyValuePair<string, TType>>.Clear()
        {
            ((ICollection<KeyValuePair<string, TType>>)OriginalDictionary).Clear();
        }

        public bool Contains(KeyValuePair<string, TType> item)
        {
            return ((ICollection<KeyValuePair<string, TType>>)OriginalDictionary).Contains(item);
        }

        public void CopyTo(KeyValuePair<string, TType>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<string, TType>>)OriginalDictionary).CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<string, TType> item)
        {
            return ((ICollection<KeyValuePair<string, TType>>)OriginalDictionary).Remove(item);
        }

        public void CopyTo(Array array, int index)
        {
            ((ICollection) OriginalDictionary).CopyTo(array, index);
        }

        int ICollection.Count { get { return OriginalDictionary.Count; } }
        public object SyncRoot { get { return ((ICollection) OriginalDictionary).SyncRoot; } }
        public bool IsSynchronized { get { return ((ICollection)OriginalDictionary).IsSynchronized; } }
        int ICollection<KeyValuePair<string, TType>>.Count 
        { 
            get
            {
                return ((ICollection<KeyValuePair<string, TType>>)OriginalDictionary).Count;
            } 
        }
        ICollection IDictionary.Values { get { return ((IDictionary) OriginalDictionary).Values; } }
        bool IDictionary.IsReadOnly { get { return ((IDictionary)OriginalDictionary).IsReadOnly; } }
        public bool IsFixedSize { get { return ((IDictionary)OriginalDictionary).IsFixedSize; } }
        bool ICollection<KeyValuePair<string, TType>>.IsReadOnly { get { return ((IDictionary)OriginalDictionary).IsReadOnly; } }

        public bool ContainsKey(string key)
        {
            return OriginalDictionary.ContainsKey(key);
        }

        public void Add(string key, TType value)
        {
            OriginalDictionary.Add(key, value);
        }

        public bool Remove(string key)
        {
            return OriginalDictionary.Remove(key);
        }

        public bool TryGetValue(string key, out TType value)
        {
            return OriginalDictionary.TryGetValue(key, out value);
        }

        TType IDictionary<string, TType>.this[string key]
        {
            get { return OriginalDictionary[key]; }
            set { OriginalDictionary[key] = value; }
        }

        ICollection<string> IDictionary<string, TType>.Keys { get { return OriginalDictionary.Keys; } }
        ICollection IDictionary.Keys { get { return ((IDictionary) OriginalDictionary).Keys; } }
        ICollection<TType> IDictionary<string, TType>.Values { get { return OriginalDictionary.Values; } }

        #endregion
    }
}
