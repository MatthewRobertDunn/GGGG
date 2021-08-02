using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GGGG.NewBoardAgain
{
    public class BoardPool<T>
    {
        [ThreadStatic]
        private static FastStack<T> pool;
        int maxItems = 0;

        public BoardPool(int maxItems)
        {
            this.maxItems = maxItems;
        }

        public FastStack<T> Pool
        {
            get
            {
                if (pool == null)
                    pool = new FastStack<T>(maxItems);

                return pool;

            }
        }
    }
}
