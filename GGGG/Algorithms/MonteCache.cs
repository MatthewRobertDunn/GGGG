using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.CompilerServices;
using GGGG.Interface;

namespace GGGG.Algorithms
{
    //public class MonteCache
    //{
    //    LruCache<string, MonteCacheEntry> internalCache;

    //    public MonteCache()
    //    {
    //        internalCache = new LruCache<string, MonteCacheEntry>(new TimeSpan(0, 60, 0), 500000, 10000);
    //    }


    //    [MethodImpl(MethodImplOptions.Synchronized)]
    //    public void CacheBoard(Board board, double eval, int currentDepth)
    //    {
    //        int max = currentDepth + 14;
    //        max = Math.Min(max, board.MoveHistory.Count);

    //        for (int i = currentDepth; i < max; i++)
    //        {
    //            string hash = board.MoveHistory[i];
    //            AddOrUpdateEntry(hash, eval,currentDepth);
    //        }
    //    }

    //    [MethodImpl(MethodImplOptions.Synchronized)]
    //    public MonteCacheEntry GetCachedBoardEvals(string hash)
    //    {
    //        var result = internalCache.GetObject(hash);
    //        if(result == null)
    //            return  new MonteCacheEntry();

    //        return result;
    //    }


    //    public void AddOrUpdateEntry(string hash, double eval, int currentDepth)
    //    {
    //        var item = internalCache.GetObject(hash);
    //        if (item == null)
    //            item = new MonteCacheEntry();

    //        //Item is higher in the tree than the curret depth, reset all stats (pass)
    //        if(item.Depth < currentDepth)
    //        {
    //            item = new MonteCacheEntry {Depth = currentDepth};
    //        }

    //        if (eval > 0)
    //            item.WhiteWon += 1;
    //        else
    //            item.BlackWon += 1;

    //        internalCache.AddObject(hash, item);
    //    }

    //}
}
