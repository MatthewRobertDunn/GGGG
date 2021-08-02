using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GGGG;
using GGGG.Interface;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class SamTests
    {
        [TestMethod]
        public void TestSam()
        {
            var parent = BoardFactory.GetBoard(9);

            var board = parent.PlayMove(1, 1,BoardSquares.Black);
            board = board.PlayMove(2, 1, BoardSquares.White);
            board = board.PlayMove(3, 1, BoardSquares.Black);
            board = board.PlayMove(4, 1, BoardSquares.White);
            board = board.PlayMove(5, 1, BoardSquares.Black);
            board = board.PlayMove(6, 1, BoardSquares.White);
            board = board.PlayMove(7, 1, BoardSquares.Black);
            board = board.PlayMove(8, 1, BoardSquares.White);

            UCTSearch search = new UCTSearch(0.5f,true,false);

            var results = search.GetRavePermutations(parent, board,0).ToList();

            var boardhashes = search.GetBoardHistoryForMove(parent.BoardHash.HashValue, results.First());



        }
    }
}
