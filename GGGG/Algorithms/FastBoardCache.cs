using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GGGG.Twister;

namespace GGGG.Algorithms
{
    public class FastBoardCache<TKey, T>
    {
        private SortedList<DateTime, TKey> expiryList;
        private Dictionary<TKey, FastBoardCacheItem<T>> dict;
        private Timer cleaningTimer;
        private TimeSpan expiry;
        public FastBoardCache(TimeSpan expiry)
        {
            this.expiry = expiry;
            expiryList = new SortedList<DateTime, TKey>(1000000);
            dict = new Dictionary<TKey, FastBoardCacheItem<T>>(1000000);
            //cleaningTimer = new Timer(new TimerCallback(CleanStuff), null, 10000, 10000);
        }


        public void AddItem(TKey key, T item)
        {
            lock (readWriteLock)
            {
                FastBoardCacheItem<T> oldValue;
                if (dict.TryGetValue(key, out oldValue))
                {
                    if (Math.Abs(DateTime.Now.Ticks - oldValue.Date.Ticks) < SecondsPerTick)
                    {
                        //don't bother updating the date, too close
                        dict[key] = new FastBoardCacheItem<T>() { Date = oldValue.Date, Item = item };
                        return;
                    }

                    expiryList.Remove(oldValue.Date);
                }

                var dateKey = TryAddItem(key, DateTime.Now);
                var newValue = new FastBoardCacheItem<T>() { Date = dateKey, Item = item };
                dict[key] = newValue;
            }
        }

        private object readWriteLock = new object();

        public bool TryGetItem(TKey key, out T item)
        {
            lock (readWriteLock)
            {
                FastBoardCacheItem<T> oldValue;
                if (dict.TryGetValue(key, out oldValue))
                {
                    if (Math.Abs(DateTime.Now.Ticks - oldValue.Date.Ticks) < SecondsPerTick)
                    {
                        //don't bother updating the date and shizzle this time
                        item = oldValue.Item;
                        return true;
                    }
                    expiryList.Remove(oldValue.Date);
                }
                else
                {
                    item = default(T);
                    return false;
                }

                var dateKey = TryAddItem(key, DateTime.Now);
                var newValue = new FastBoardCacheItem<T>() { Date = dateKey, Item = oldValue.Item };
                dict[key] = newValue;

                item = oldValue.Item;
                return true;
            }
        }


        private void CleanStuff(object state)
        {
            if (Monitor.TryEnter(readWriteLock))
            {
                try
                {
                    if (expiryList.Count == 0)
                        return;

                    var item = expiryList.First();

                    while ((DateTime.Now - item.Key) > expiry)
                    {
                        expiryList.Remove(item.Key);
                        dict.Remove(item.Value);

                        if (expiryList.Count == 0)
                            break;
                        item = expiryList.First();
                    }

                }
                finally
                {
                    Monitor.Exit(readWriteLock);
                }
            }
        }



        private const double SecondsPerTick = 10000000;
        private DateTime TryAddItem(TKey key, DateTime newTime)
        {
            double randValue = GetRandValue((int)(1.0 * SecondsPerTick));
            var start = newTime + new TimeSpan((long)randValue);


            while (this.expiryList.ContainsKey(start))
            {
                randValue = GetRandValue((int)(1.0 * SecondsPerTick));
                start = newTime + new TimeSpan((long)randValue);
            }

            this.expiryList.Add(start, key);
            return start;
        }

        private static double GetRandValue(int rndAmount)
        {
            var randValue = (double)MyRandom.Random.Next(rndAmount);
            randValue = randValue - ((double)rndAmount / 2);
            return randValue;
        }

    }

    public struct FastBoardCacheItem<T>
    {
        public T Item { get; set; }
        public DateTime Date { get; set; }
    }
}
