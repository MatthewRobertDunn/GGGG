using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GGGG
{
    /// <summary>

    /// Fixed maximum size collection optimized for extremely high insert/remove performance

    /// </summary>

    /// <typeparam name="T"></typeparam>

    public class FastStack<T>
    {
        private int itemIndex;
        private int maxItems;
        private T[] stack;

        public FastStack(int maxItems)
        {
            this.stack = new T[maxItems];
            this.maxItems = maxItems;
        }

        public void Push(T item)
        {
            if (itemIndex >= maxItems)
                throw new Exception("Stack is full");


            stack[itemIndex] = item;
            itemIndex += 1;

        }



        public T Pop()
        {
            if (itemIndex <= 0)
                throw new Exception("Stack is empty");
            itemIndex--;

            return stack[itemIndex];
        }

        public T Pop(Func<T> construct)
        {
            if (itemIndex > 0)
                return Pop();
            return construct();
        }


        public int Count
        {
            get { return this.itemIndex; }
        }

        public int MaxItems
        {
            get { return this.maxItems; }
        }

    }
}
