using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GGGG.MemoryManagement
{
    public class ObjectPool<T> where T : class
    {
        FastStack<ObjectPoolReference<T>> pool;
        Func<T> construct;

        public ObjectPool(Func<T> construct, int maxSize)
        {
            this.construct = construct;
            pool = new FastStack<ObjectPoolReference<T>>(maxSize);
        }

        public ObjectPoolReference<T> GetObject() 
        {
            if (pool.Count > 0)
                return pool.Pop();

            return new ObjectPoolReference<T>(this, construct());
        }

        public void PutObject(ObjectPoolReference<T> obj)
        {
            if (pool.Count < pool.MaxItems)
            {
                pool.Push(obj);
            }
        }
    }


    
    public class ObjectPoolReference<T> : IDisposable  where T : class
    {
        public ObjectPool<T> Pool { get; private set; }

        public T Instance { get; private set; }

        public ObjectPoolReference(ObjectPool<T> pool, T instance)
        {
            Pool = pool;
            Instance = instance;
        }

        ~ObjectPoolReference()
        {
            this.Dispose();
        }


        public void Dispose()
        {
            Pool.PutObject(this);
        }
    }
}
