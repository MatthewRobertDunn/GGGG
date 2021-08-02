using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CellRtree
{
    //Can only take pointers to structs in c#, hence struct limit
    public unsafe struct TrieNodeUnsafe : IDisposable
    {
        //Contains number of children if non leaf node
        //Contains pointer to stored item if leaf node
        private int childCount;

        //Pointer to children if non leaf node, null if leaf.
        private TrieNodeUnsafe* children;

        private void CreateChildren(int childCount)
        {
            if (children == null)
            {
                //Allocate enough ram for childCount children on global unmanaged heap
                children = (TrieNodeUnsafe*)Marshal.AllocHGlobal(sizeof(TrieNodeUnsafe) * childCount);
                for (int i = 0; i < childCount; i++)
                {
                    children[i].children = null;
                }

                this.childCount = childCount;
            }
        }

        private TrieNodeUnsafe GetChild(int index)
        {
            if (IsLeaf)
                throw new Exception("Node is a leaf!");

            return children[index];
        }

        public void SetValue(int value)
        {
            //Leaf nodes can't have children
            DeleteChildren();
            childCount = value;
        }

        public bool TryGetValue(int[] key, out int value)
        {
            value = 0;
            fixed (TrieNodeUnsafe* parent = &this)
            {
                TrieNodeUnsafe* currentNode = parent;
                for (int i = 0; i < key.Length; i++)
                {
                    if (!currentNode->IsLeaf)
                    {
                        int child = key[i];
                        currentNode = (currentNode->children + child);
                    }
                    else
                        return false;
                }

                if (!currentNode->IsLeaf)
                    return false;

                value = currentNode->Value;
                return true;
            }
        }


        public void AddValue(int[] key, int value, int childCount)
        {

            fixed (TrieNodeUnsafe* parent = &this)
            {
                TrieNodeUnsafe* currentNode = parent;
                for (int i = 0; i < key.Length; i++)
                {
                    if (currentNode->IsLeaf)
                        currentNode->CreateChildren(childCount);

                    int child = key[i];
                    currentNode = (currentNode->children + child);
                }
                currentNode->SetValue(value);
            }
        }


        public void Clear()
        {
            SetValue(0);
        }

        public bool IsLeaf
        {
            get { return children == null; }
        }

        public int Value
        {
            get
            {
                return childCount;
            }
        }



        private void DeleteChildren()
        {
            //delete them.
            if (children != null)
            {
                //Free all my child nodes
                for (int i = 0; i < childCount; i++)
                    children[i].DeleteChildren();

                Marshal.FreeHGlobal((IntPtr)children);
                children = null;
            }
        }

        public void Dispose()
        {
            DeleteChildren();
        }
    }
}
