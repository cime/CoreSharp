using System.Collections;
using System.Collections.Generic;

namespace CoreSharp.Breeze.Metadata
{
    public abstract class MetadataList<TNew> : IList<TNew>
        where TNew : MetadataDictionary 
    {
        protected MetadataList()
        {
            OriginalList = new List<Dictionary<string, object>>();
            NewList = new List<TNew>();
        }

        protected MetadataList(List<Dictionary<string, object>> origList)
        {
            OriginalList = origList;
            NewList = new List<TNew>();
            foreach (var origItem in OriginalList)
            {
                NewList.Add(Convert(origItem));
            }
        }

        protected abstract TNew Convert(Dictionary<string, object> item);

        public List<Dictionary<string, object>> OriginalList { get; }

        public List<TNew> NewList { get; }

        public IEnumerator<TNew> GetEnumerator()
        {
            return NewList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return NewList.GetEnumerator();
        }

        public void Add(TNew item)
        {
            NewList.Add(item);
            OriginalList.Add(item.OriginalDictionary);
        }

        public void Clear()
        {
            NewList.Clear();
            OriginalList.Clear();
        }

        public bool Contains(TNew item)
        {
            return NewList.Contains(item);
        }

        public void CopyTo(TNew[] array, int arrayIndex)
        {
            NewList.CopyTo(array, arrayIndex);
        }

        public bool Remove(TNew item)
        {
            OriginalList.Remove(item.OriginalDictionary);
            return NewList.Remove(item);
        }

        public int Count { get { return NewList.Count; } }
        public bool IsReadOnly { get { return ((ICollection<TNew>)NewList).IsReadOnly; } }
        public int IndexOf(TNew item)
        {
            return NewList.IndexOf(item);
        }

        public void Insert(int index, TNew item)
        {
            NewList.Insert(index, item);
            OriginalList.Insert(index, item.OriginalDictionary);
        }

        public void RemoveAt(int index)
        {
            NewList.RemoveAt(index);
            OriginalList.RemoveAt(index);
        }

        public TNew this[int index]
        {
            get { return NewList[index]; }
            set
            {
                NewList[index] = value;
                OriginalList[index] = value.OriginalDictionary;
            }
        }
    }
}
