using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using GGGG.Algorithms;
using GGGG.Interface;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GGGG;
using System.Diagnostics;

namespace UnitTests
{
    [TestClass]
    public class UnitTest1
    {

        [TestMethod()]
        public void PermTest()
        {
            List<int> foo = new List<int>() {1,2,3,4,5};

            var crap = foo.Permutate();

        }

        [TestMethod()]
        public void GoMovetest()
        {
            GoMove move = new GoMove();
            move.Color = BoardSquares.White;
            move.Pos = 22;

            var test = move.ToString();
        }

        //[TestMethod()]
        //public void patternMatchTest()
        //{
        //    IFastBoard board = new FastBoard(9,9);

        //    board = board.PlayMove(1, 1, BoardSquares.Black);
        //    board = board.PlayMove(2, 1, BoardSquares.White);
        //    board = board.PlayMove(3, 1, BoardSquares.Black);
        //    Debug.WriteLine(board.ToString());   
        //    var result = PatternLibrary.MatchesAnyPattern(board.XYToPos(2,2), board);

        //    Assert.IsTrue(result);
        //}

        [TestMethod()]
        public void IsEyeEdgeTest()
        {
            //FastBoard board = BoardFactory.GetBoard(9);
            //board = board.PlayMove(2, 1, BoardSquares.White);
            //board = board.PlayMove(2, 2, BoardSquares.White);
            //board = board.PlayMove(1, 2, BoardSquares.White);
            //Debug.WriteLine(board.ToString());            

            //Assert.IsTrue(board.PosIsEye(board.XYToPos(1, 1), BoardSquares.White));


        }


        [TestMethod()]
        public void DiagEdgeCaptureTest()
        {
            IFastBoard board = BoardFactory.GetBoard(9);


            board = board.PlayMove(1, 4, BoardSquares.White);
            board = board.PlayMove(1, 5, BoardSquares.White);
            board = board.PlayMove(2, 4, BoardSquares.Black);
            board = board.PlayMove(2, 5, BoardSquares.Black);
            board = board.PlayMove(1, 6, BoardSquares.Black);
            board = board.PlayMove(2, 6, BoardSquares.Black);
            board = board.PlayMove(1, 7, BoardSquares.White);
            board = board.PlayMove(2, 7, BoardSquares.Black);
            board = board.PlayMove(1, 8, BoardSquares.White);
            board = board.PlayMove(2, 8, BoardSquares.Black);

            Debug.WriteLine(board.ToString());

            board = board.PlayMove(1, 9, BoardSquares.Black);

            Debug.WriteLine(board.ToString());
        }


        [TestMethod()]
        public void TestStringBuild()
        {
            //FastBoard board = BoardFactory.GetBoard(9);

            //board = board.PlayMove(1, 1, BoardSquares.Black);
            //board = board.PlayMove(2, 1, BoardSquares.Black);

            //board = board.PlayMove(4, 1, BoardSquares.Black);
            //board = board.PlayMove(5, 1, BoardSquares.Black);

            //board = board.PlayMove(3, 1, BoardSquares.Black);


            //var item = board.GoStrings[12];
            //Assert.IsTrue(board.GoStrings[13] == item);
            //Assert.IsTrue(board.GoStrings[14] == item);
            //Assert.IsTrue(board.GoStrings[15] == item);
            //Assert.IsTrue(board.GoStrings[16] == item);

            //Assert.IsTrue(item.Stones.Count() == 5);

            //Assert.IsTrue(item.Liberties.Count() == 6);

        }



        [TestMethod()]
        public void BigCaptureTest()
        {
            IFastBoard board = BoardFactory.GetBoard(9);

            board = board.PlayMove(1, 1, BoardSquares.Black);
            board = board.PlayMove(1, 2, BoardSquares.Black);
            board = board.PlayMove(1, 3, BoardSquares.Black);
            board = board.PlayMove(1, 4, BoardSquares.Black);

            board = board.PlayMove(3, 1, BoardSquares.Black);
            board = board.PlayMove(3, 2, BoardSquares.Black);
            board = board.PlayMove(3, 3, BoardSquares.Black);
            board = board.PlayMove(3, 4, BoardSquares.Black);

            board = board.PlayMove(2, 1, BoardSquares.White);
            board = board.PlayMove(2, 2, BoardSquares.White);
            board = board.PlayMove(2, 3, BoardSquares.White);
            board = board.PlayMove(2, 4, BoardSquares.White);


            Console.WriteLine("Before big capture");
            Console.WriteLine(board.ToString());


            board = board.PlayMove(2, 5, BoardSquares.Black);
            Console.WriteLine("After big capture");
            Console.WriteLine(board.ToString());

            Assert.IsTrue(board.GetItemAt(2, 1) == BoardSquares.Empty);
            Assert.IsTrue(board.GetItemAt(2, 2) == BoardSquares.Empty);
            Assert.IsTrue(board.GetItemAt(2, 3) == BoardSquares.Empty);
            Assert.IsTrue(board.GetItemAt(2, 4) == BoardSquares.Empty);
        }

        [TestMethod()]
        public void CaptureOneEyeTest()
        {
            IFastBoard board = BoardFactory.GetBoard(9);



            board = board.PlayMove(3, 4, BoardSquares.Black);
            board = board.PlayMove(5, 4, BoardSquares.Black);
            board = board.PlayMove(4, 3, BoardSquares.Black);
            board = board.PlayMove(4, 5, BoardSquares.Black);

            board = board.PlayMove(3, 3, BoardSquares.White);
            board = board.PlayMove(5, 3, BoardSquares.White);
            board = board.PlayMove(4, 2, BoardSquares.White);


            Debug.WriteLine("Before one eye capture");
            Debug.WriteLine(board.ToString());


            Debug.Assert(board.MoveIsValid(board.PlayMove(4, 4, BoardSquares.White), BoardSquares.White));
            board = board.PlayMove(4, 4, BoardSquares.White);
            Debug.WriteLine("After one eye capture");
            Debug.WriteLine(board.ToString());

            Assert.IsTrue(board.GetItemAt(4, 4) == BoardSquares.White);
            Assert.IsTrue(board.GetItemAt(4, 3) == BoardSquares.Empty);

            Console.WriteLine(board.ToString());
        }


        [TestMethod()]
        public void NotSuicideCapture()
        {
            IFastBoard board = BoardFactory.GetBoard(9);



            board = board.PlayMove(1, 2, BoardSquares.White);
            board = board.PlayMove(2, 2, BoardSquares.Black);
            board = board.PlayMove(1, 4, BoardSquares.White);
            board = board.PlayMove(2, 4, BoardSquares.White);
            board = board.PlayMove(2, 3, BoardSquares.White);

            Debug.WriteLine("Before suicide");
            Debug.WriteLine(board.ToString());


            board = board.PlayMove(1, 3, BoardSquares.Black);
            Debug.WriteLine("After suicide");
            Debug.WriteLine(board.ToString());

            board = board.PlayMove(1, 1, BoardSquares.White);
            Debug.WriteLine(board.ToString());

            Assert.IsTrue(board.GetItemAt(4, 4) == BoardSquares.Empty);

            Debug.WriteLine(board.ToString());
        }

        [TestMethod()]
        public void SuicideTest()
        {
            IFastBoard board = BoardFactory.GetBoard(9);



            board = board.PlayMove(3, 4, BoardSquares.Black);
            board = board.PlayMove(5, 4, BoardSquares.Black);
            board = board.PlayMove(4, 5, BoardSquares.Black);
            board = board.PlayMove(4, 3, BoardSquares.Black);

            Debug.WriteLine("Before suicide");
            Debug.WriteLine(board.ToString());


            Assert.IsFalse(board.MoveIsValid(board.PlayMove(4, 4, BoardSquares.White), BoardSquares.White));
            board = board.PlayMove(4, 4, BoardSquares.White);

            Debug.WriteLine("After suicide");

            Assert.IsTrue(board.GetItemAt(4, 4) == BoardSquares.Empty);

            Debug.WriteLine(board.ToString());
        }

        [TestMethod()]
        public void DestroyStoneTest()
        {
            IFastBoard board = BoardFactory.GetBoard(9);

            board = board.PlayMove(4, 4, BoardSquares.White);

            board = board.PlayMove(3, 4, BoardSquares.Black);
            board = board.PlayMove(5, 4, BoardSquares.Black);
            board = board.PlayMove(4, 5, BoardSquares.Black);

            Debug.WriteLine("Before capture");
            Debug.WriteLine(board.ToString());

            board = board.PlayMove(4, 3, BoardSquares.Black);

            Assert.IsTrue(board.GetItemAt(4, 4) == BoardSquares.Empty);

            Debug.WriteLine("After capture");
            Debug.WriteLine(board.ToString());
        }
    }
}

