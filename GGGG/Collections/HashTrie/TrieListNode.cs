using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CellRtree
{
    public struct TrieListNode<T>
    {
        private readonly T[] item;
        private TrieListNode<T>[][] _childrenLazy;

        public TrieListNode(long children)
        {
            _childrenLazy = new TrieListNode<T>[1][];
            item = new T[1];
        }

        private TrieListNode<T>[] _children
        {
            get { return _childrenLazy[0]; }
            set { _childrenLazy[0] = value; }
        }



        public long ChildCount
        {
            get
            {
                if (_children == null)
                    return 0;
                return _children.Count(x => !x.IsEmpty);
            }
        }

        public bool IsEmpty
        {
            get { return item[0].Equals(default(T)) && ChildCount == 0; }
        }

        public bool IsNull
        {
            get { return this._childrenLazy == null && item == null; }
        }

        public TrieListNode<T> GetNode(int[] path)
        {
            TrieListNode<T> currentNode = this;

            for (long i = 0; i < path.Length; i++)
            {
                var node = path[i];
                if (currentNode[node].IsNull)
                    return new TrieListNode<T>();

                currentNode = currentNode[node];
            }

            return currentNode;
        }

        public bool NodeExists(int[] path)
        {
            return !GetNode(path).IsNull;
        }

        public T Value
        {
            get { return item[0]; }
            set { this.item[0] = value; }
        }


        public TrieListNode<T> AddChild(long id, long children)
        {
            if(this._children == null)
            {
                this._children = new TrieListNode<T>[children];
            }

            if (this._children[id].IsNull) //Only replace null nodes
            {
                TrieListNode<T> child = new TrieListNode<T>(children);
                this._children[id] = child;
            }

            return this._children[id];
        }

        public void Clear()
        {
            if (!IsNull)
                for (long i = 0; i < _children.Length; i++)
                {
                    _children[i] = new TrieListNode<T>();
                }
        }

        public void ClearIfEmpty()
        {
            if (this.IsEmpty)
            {
                Clear();
            }
        }

        public void RemoveChild(long index)
        {
            this._children[index] = new TrieListNode<T>();
        }

        public TrieListNode<T> this[long index]
        {
            get
            {
                if (_children == null)
                    return new TrieListNode<T>();

                return _children[index];
            }
        }
    }
}

