using System;
using GGGG;
using GGGG.Algorithms;
using GGGG.Interface;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class CapturesCacheTest
    {
        [TestMethod]
        public void TestMethod1()
        {

            CapturesCache cache = new CapturesCache();

            cache.AddCapture(1, 1, BoardSquares.White);
            cache.AddCapture(1, 2, BoardSquares.White);
            cache.AddCapture(1, 3, BoardSquares.Black);
            cache.AddCapture(1, 4, BoardSquares.Black);

            var result = cache.GetCaptures(1, BoardSquares.Black);

            Assert.AreEqual(4, result.Length);
        }
    }
}
