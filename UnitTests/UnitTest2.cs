using System;
using System.Diagnostics;
using GGGG.MemoryManagement;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class UnitTest2
    {
        [TestMethod]
        public void TestMethod1()
        {
            int length = 81;
            var pool = new ObjectPool<int[]>(() => new int[length], 10);

            Stopwatch watch = new Stopwatch();

            watch.Start();

            for (int i = 0; i < 10000000; i++)
            {
                using (var result = pool.GetObject())
                    Assert.AreEqual(length, result.Instance.Length);
            }
           watch.Stop();

           watch.Reset();

           watch.Start();

           for (int i = 0; i < 10000000; i++)
           {
               using (var result = pool.GetObject())
                   Assert.AreEqual(length, result.Instance.Length);
           }
           watch.Stop();

        }
    }
}
