using System;
using System.Diagnostics;
using GGGG.MemoryManagement;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class MemManagertests
    {
        [TestMethod]
        public void TestMethod1()
        {
            FixedWidthUnsafeMemoryManager manager = new FixedWidthUnsafeMemoryManager(81, 1000000);


            Stopwatch watch = new Stopwatch();
            watch.Start();
            for (int i = 0; i < 1000000; i++)
            {
                IntPtr ptr;
                var result = manager.GetNewMemory(out ptr);
                Assert.IsTrue(result);
            }

            watch.Stop();
           
 
        }
    }
}
