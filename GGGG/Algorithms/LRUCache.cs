using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace GGGG.Algorithms
{

    ////This class implements an LRU based cache. Uses a linked list to keep track of node usage
    ////Clients should use the following methods:
    ////AddObject(TKey key, TValue cacheObject)
    ////TValue GetObject(TKey key)
    public class LruCache<TKey, TValue>
    {
        private readonly Dictionary<TKey, NodeInfo> cachedNodesDictionary = new Dictionary<TKey, NodeInfo>();
        private readonly LinkedList<NodeInfo> lruLinkedList = new LinkedList<NodeInfo>();

        private readonly uint maxSize;
        private readonly TimeSpan timeOut;

        private static readonly ReaderWriterLockSlim rwl = new ReaderWriterLockSlim();

        private Timer cleanupTimer;

        public LruCache(TimeSpan itemExpiryTimeout, uint maxCacheSize = (uint)100, uint memoryRefreshInterval = (uint)1000)
        {
            this.timeOut = itemExpiryTimeout;
            this.maxSize = maxCacheSize;
            AutoResetEvent autoEvent = new AutoResetEvent(false);
            TimerCallback tcb = this.RemoveExpiredElements;
            this.cleanupTimer = new Timer(tcb, autoEvent, 0, memoryRefreshInterval);
        }

        public void AddObject(TKey key, TValue cacheObject)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            //Trace.WriteLine(string.Format("Adding a cache object with key: {0}", key.ToString()));
            rwl.EnterWriteLock();
            try
            {
                NodeInfo node;
                if (this.cachedNodesDictionary.TryGetValue(key, out node))
                {
                    this.Delete(node);
                }

                this.ShrinkToSize(this.maxSize - 1);
                this.CreateNodeandAddtoList(key, cacheObject);
            }
            finally
            {
                rwl.ExitWriteLock();
            }
        }


        public TValue GetObject(TKey key)
        {
            TValue data = default(TValue);
            NodeInfo node;
            rwl.EnterReadLock();
            try
            {
                if (this.cachedNodesDictionary.TryGetValue(key, out node))
                {
                    if (node != null && !node.IsExpired())
                    {
                        //Trace.WriteLine(string.Format("Cache hit for key: {0}", key.ToString()));
                        node.AccessCount++;
                        data = node.Value;

                        if (node.AccessCount > 20)
                        {
                            ThreadPool.QueueUserWorkItem(this.AddBeforeFirstNode, key);
                        }
                    }
                }
                else
                {
                    //Trace.WriteLine(string.Format("Cache miss for key: {0}", key.ToString()));
                }

                return data;
            }
            finally
            {
                rwl.ExitReadLock();
            }
        }

        private void RemoveExpiredElements(object stateInfo)
        {
            rwl.EnterWriteLock();
            try
            {
                while (this.lruLinkedList.Last != null)
                {
                    NodeInfo node = this.lruLinkedList.Last.Value;
                    if (node != null && node.IsExpired())
                    {
                        this.Delete(node);
                    }
                    else
                    {
                        break;
                    }
                }
            }
            finally
            {
                rwl.ExitWriteLock();
            }
        }

        private void CreateNodeandAddtoList(TKey userKey, TValue cacheObject)
        {
            NodeInfo node = new NodeInfo(userKey, cacheObject, (this.timeOut > DateTime.MaxValue.Subtract(DateTime.UtcNow) ? DateTime.MaxValue : DateTime.UtcNow.Add(this.timeOut)));

            node.LLNode = this.lruLinkedList.AddFirst(node);
            this.cachedNodesDictionary[userKey] = node;
        }

        private void AddBeforeFirstNode(object stateinfo)
        {
            rwl.EnterWriteLock();
            try
            {
                TKey key = (TKey)stateinfo;
                NodeInfo nodeInfo;
                if (this.cachedNodesDictionary.TryGetValue(key, out nodeInfo))
                {
                    if (nodeInfo != null && !nodeInfo.IsExpired() && nodeInfo.AccessCount > 20)
                    {
                        if (nodeInfo.LLNode != this.lruLinkedList.First)
                        {
                            this.lruLinkedList.Remove(nodeInfo.LLNode);
                            nodeInfo.LLNode = this.lruLinkedList.AddBefore(this.lruLinkedList.First, nodeInfo);
                            nodeInfo.AccessCount = 0;
                        }
                    }
                }
            }
            finally
            {
                rwl.ExitWriteLock();
            }
        }

        private void ShrinkToSize(uint desiredSize)
        {
            while (this.cachedNodesDictionary.Count > desiredSize)
            {
                this.RemoveLeastValuableNode();
            }
        }

        private void RemoveLeastValuableNode()
        {
            if (this.lruLinkedList.Last != null)
            {
                NodeInfo node = this.lruLinkedList.Last.Value;
                this.Delete(node);
            }
        }

        private void Delete(NodeInfo node)
        {
            //Trace.WriteLine(string.Format("Evicting object from cache for key: {0}", node.Key.ToString()));
            this.lruLinkedList.Remove(node.LLNode);
            this.cachedNodesDictionary.Remove(node.Key);
        }

        ////This class represents data stored in the LinkedList Node and Dictionary
        private class NodeInfo
        {
            private readonly DateTime timeOutTime;

            internal NodeInfo(TKey key, TValue value, DateTime timeouttime)
            {
                this.Key = key;
                this.Value = value;
                this.timeOutTime = timeouttime;
            }

            internal TKey Key { get; private set; }

            internal TValue Value { get; private set; }

            internal int AccessCount { get; set; }

            internal LinkedListNode<NodeInfo> LLNode { get; set; }

            internal bool IsExpired()
            {
                return DateTime.UtcNow >= this.timeOutTime;
            }
        }
    }
}

