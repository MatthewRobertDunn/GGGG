using System;
using System.Linq;
using GGGG.Algorithms;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class TerminatedListTest
    {
        [TestMethod]
        public void TestMethod1()
        {

            TerminatedList list = new TerminatedList(81);

            list.AddItem(1);
            list.AddItem(2);
            list.AddItem(3);
            list.AddItem(4);
            list.AddItem(5);

            list.SequenceEqual(new int[] { 1, 2, 3, 4, 5 });

            list.RemoveItem(3);

            list.SequenceEqual(new int[] { 1, 2, 4, 5 });
        }
    }
}
