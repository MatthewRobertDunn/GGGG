using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GGGG.MemoryManagement
{
    public unsafe class FixedWidthUnsafeMemoryManager
    {

        private byte* memory;
        private ConcurrentBag<IntPtr> pool;
        
        public FixedWidthUnsafeMemoryManager(int length, int totalChunks)
        {
            pool = new ConcurrentBag<IntPtr>();
            memory = (byte*) System.Runtime.InteropServices.Marshal.AllocHGlobal(length * totalChunks);

            for (int i = 0; i < (length * totalChunks);i += length)
            {
                pool.Add((IntPtr)(memory + i));
            }
        }
        
        public unsafe bool GetNewMemory(out IntPtr ptr)
        {
                return pool.TryTake( out ptr);
        }

        public unsafe void Recyle(IntPtr item)
        {
            pool.Add(item);
        }
    }
}
