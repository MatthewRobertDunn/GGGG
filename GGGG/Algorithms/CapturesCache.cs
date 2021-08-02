using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GGGG.Interface;

namespace GGGG.Algorithms
{
    public class CapturesCache
    {
        LruCache<Int64, Int16[]> internalCache;


        public CapturesCache()
        {
            internalCache = new LruCache<Int64, Int16[]>(new TimeSpan(0, 5, 0), 50000000, 10000);
        }

        public void AddCapture(Int64 board, Int16 capture, BoardSquares player)
        {
            //if (player == BoardSquares.Black)
            //{
            //    capture = (short)(-1 * capture);
            //}
            var item = internalCache.GetObject(board);
            if (item == null)
            {
                item = new Int16[1] { capture };
                internalCache.AddObject(board, item);
                return;
            }

            if (!item.Contains(capture))
            {
                Array.Resize(ref item, item.Length + 1);
                item[item.Length - 1] = capture;
                internalCache.AddObject(board, item);
            }
        }

        public Int16[] GetCaptures(Int64 board, BoardSquares player)
        {
            return internalCache.GetObject(board);
        }
    }
}
