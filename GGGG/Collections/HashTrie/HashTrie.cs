using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CellRtree
{
    public class HashTrie<TValue> : IDictionary<long, TValue>
    {
        private TrieListNode<TValue> root = new TrieListNode<TValue>(256);

        public IEnumerator<KeyValuePair<long, TValue>> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(KeyValuePair<long, TValue> item)
        {

        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(KeyValuePair<long, TValue> item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(KeyValuePair<long, TValue>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(KeyValuePair<long, TValue> item)
        {
            throw new NotImplementedException();
        }

        public int Count { get; private set; }
        public bool IsReadOnly { get; private set; }
        public bool ContainsKey(long key)
        {
            throw new NotImplementedException();
        }

        public void Add(long key, TValue value)
        {
            var keyBytes = BitConverter.GetBytes(key);
            var child = root;
            for (int i = keyBytes.Length - 1; i >= 0; i--)
            {
                child = root.AddChild(keyBytes[i], 256);
            }
            child.Value = value;
        }

        public bool Remove(long key)
        {
            var keyBytes = BitConverter.GetBytes(key);

            if (root.NodeExists(Array.ConvertAll(keyBytes, x => (int)x)))
            {
                root[keyBytes[3]][keyBytes[2]][keyBytes[1]][keyBytes[0]].ClearIfEmpty();
                root[keyBytes[3]][keyBytes[2]][keyBytes[1]].ClearIfEmpty();
                root[keyBytes[3]][keyBytes[2]].ClearIfEmpty();
                root[keyBytes[3]].ClearIfEmpty();
                return true;
            }

            return false;
        }

        public bool TryGetValue(long key, out TValue value)
        {
            var keyBytes = BitConverter.GetBytes(key).Reverse().ToArray();
            var node = root.GetNode(Array.ConvertAll(keyBytes, x => (int)x));

            if (node.IsNull)
            {
                value = default(TValue);
                return false;
            }

            value = node.Value;

            return true;
        }

        public TValue this[long key]
        {
            get
            {
                TValue result;
                TryGetValue(key, out result);
                return result;
            }
            set
            {
                Add(key, value);
            }
        }

        public ICollection<long> Keys { get; private set; }
        public ICollection<TValue> Values { get; private set; }
    }
}
