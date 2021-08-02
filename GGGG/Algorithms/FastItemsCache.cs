using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace GGGG.Algorithms
{
    /// <summary>
    /// Lock free cache with abbility to limit number of items
    /// </summary>
    /// <typeparam name="KEY"></typeparam>
    /// <typeparam name="ITEM"></typeparam>
    public class LockFreeItemCache<KEY, ITEM>
    {
        private ConcurrentDictionary<KEY, ITEM> cache;
        private ConcurrentQueue<KEY> orderIndex = new ConcurrentQueue<KEY>();
        private int maxItems;
        private System.Timers.Timer cleanupTimer;

        public LockFreeItemCache(int maxItems, int cleanUpSeconds)
        {
            cache = new ConcurrentDictionary<KEY, ITEM>(8, maxItems);
            this.maxItems = maxItems;
            cleanupTimer = new Timer(cleanUpSeconds * 1000);
            cleanupTimer.Elapsed += new ElapsedEventHandler(cleanupTimer_Elapsed);
            cleanupTimer.Start();
        }

        void cleanupTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            while (cache.Count > maxItems)
            {
                KEY keyToRemove;
                if (orderIndex.TryDequeue(out keyToRemove))
                {
                    ITEM removedItem;
                    cache.TryRemove(keyToRemove, out removedItem);
                }

            }

        }



        public bool TryAdd(KEY key, ITEM item)
        {
            if (cache.TryAdd(key, item))
            {
                orderIndex.Enqueue(key);
                return true;
            }
            return false;
        }

        public bool UpdateOrAdd(KEY key, ITEM newItem)
        {
            if (TryAdd(key, newItem))
                return true;
            ITEM oldItem;

            if (GetItem(key, out oldItem))
            {
                if (cache.TryUpdate(key, newItem, oldItem))
                {
                    return true;
                }
            }
            return false;
        }

        public bool GetItem(KEY key, out ITEM item)
        {
            return cache.TryGetValue(key, out item);
        }

    }
}
