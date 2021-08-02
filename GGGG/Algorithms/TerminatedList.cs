using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GGGG.Algorithms
{
    public struct TerminatedList : IEnumerable<int>
    {
        int[] list;
        int count;
        int maxItems;

        public int MaxSize
        {
            get { return maxItems; }
        }
        public TerminatedList(int size)
        {
            list = new int[size];
            count = 0;
            this.maxItems = size;
        }

        public int Length
        {
            get { return count; }
        }

        public void AddItem(int item)
        {
            if (!this.Contains(item))
            {
                list[count] = item;
                count++;
            }
        }

        public string this[int index]
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public unsafe void AddItems(TerminatedList items)
        {
            fixed (int* src = this.list)
                for (int i = 0; i < items.count; i++)
                {
                    if (!this.Contains(items.list[i], src))
                    {
                        list[count] = items.list[i];
                        count++;
                    }
                }
        }

        public int Count
        {
            get
            {
                return this.count;
            }
        }

        public void RemoveAll(Predicate<int> func)
        {
            var itemsToRemove = this.Where(x => func(x));

            foreach (var item in itemsToRemove)
            {
                RemoveItem(item);
            }
        }

        public unsafe void RemoveItem(int item)
        {
            int index = GetIndexOf(item);
            if (index != -1)
                fixed (int* src = this.list)
                {
                    MemCopy(src, index + 1, src, index, count - index - 1);
                    count -= 1;
                }
        }

        public int GetIndexOf(int item)
        {
            for (int i = 0; i < count; i++)
            {
                if (list[i].Equals(item))
                    return i;
            }

            return -1;
        }

        public unsafe bool Contains(int item)
        {
            fixed (int* src = this.list)
                for (int i = 0; i < count; i++)
                {
                    if (src[i] == item)
                        return true;
                }
            return false;
        }

        public unsafe bool Contains(int item, int* srcList)
        {
            for (int i = 0; i < count; i++)
            {
                if (srcList[i] == item)
                    return true;
            }
            return false;
        }

        public IEnumerator<int> GetEnumerator()
        {
            for (int i = 0; i < count; i++)
            {
                yield return list[i];
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            for (int i = 0; i < count; i++)
            {
                yield return list[i];
            }
        }

        public void ForEach(Action<int> action)
        {
            for (int i = 0; i < count; i++)
            {
                action(list[i]);
            }
        }

        public unsafe void CopyTo(ref TerminatedList clone)
        {
            Debug.Assert(clone.MaxSize == this.MaxSize);
            clone.count = this.count;
            clone.maxItems = this.maxItems;
            fixed (int* src = this.list)
            fixed (int* dst = clone.list)
            {
                MemCopy(src, 0, dst, 0, count);
            }
            //Buffer.BlockCopy(this.list, 0, clone.list, 0, count * 4);
        }

        public unsafe TerminatedList Clone()
        {
            var clone = new TerminatedList(this.list.Length);
            clone.count = this.count;
            clone.maxItems = this.maxItems;
            fixed (int* src = this.list)
            fixed (int* dst = clone.list)
            {
                MemCopy(src, 0, dst, 0, count);
            }
            return clone;
        }



        public unsafe void MemCopy(int* src, int srcOffset, int* dest, int dstOffset, int count)
        {
            for (int i = 0; i < count; i++)
            {
                *(dest + dstOffset + i) = *(src + srcOffset + i);
            }

        }

    }
}
