using CellRtree;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class HashTrieTests
    {
        [TestMethod]
        public void TestHashTrie()
        {
            HashTrie<string> hashTable = new HashTrie<string>();
            hashTable.Add(1234, "Hello");

            var resultString = hashTable[1234];

            Assert.AreEqual("Hello", resultString);

            hashTable.Remove(1234);

            hashTable.Add(0, "Hello");
            hashTable.Add(1, "Hello");

        }
    }
}