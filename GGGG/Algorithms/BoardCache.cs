using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GGGG.Algorithms
{
    public class BoardCache<T> where T : class
    {
        readonly LruCache<Int64, T> _internalCache;

        public BoardCache()
        {
            _internalCache = new LruCache<Int64, T>(new TimeSpan(0, 10, 0), 50000000, 10000);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void AddItem(Int64 boardHash, T item)
        {
            _internalCache.AddObject(boardHash, item);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public T GetItem(Int64 boardHash)
        {
            return _internalCache.GetObject(boardHash);
        }
    }
}
