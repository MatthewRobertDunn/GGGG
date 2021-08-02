using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using GGGG.Interface;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GGGG.GoGui;
using GGGG;
using System.Diagnostics;
using GGGG.NewBoardAgain;

namespace UnitTests
{
    [TestClass]
    public class ReplayTests
    {
        public TestContext TestContext
        {
            get;
            set;
        }


        [TestMethod]
        public void TestMethod1()
        {
            string testGame = @";B[cc];W[cb];B[dc];W[bb];B[bc];W[db];B[ec];W[eb];B[fc];W[fb]
;B[gc];W[gb];B[hb];W[hc];B[hd];W[ab];B[ic];W[ha];B[ib];W[gd]
;B[fd];W[ge];B[fe];W[he];B[ie];W[ad];B[bd];W[ae];B[be];W[dd]
;B[de];W[ac];B[bf];W[ag];B[bg];W[ah];B[bh];W[ga];B[af];W[gf]
;B[ff];W[gg];B[fg];W[gh];B[fh];W[hf];B[if];W[hh];B[ig];W[ih]
;B[gi];W[hi];B[fi];W[dg];B[ai];W[bi];B[ci];W[di]";

            IAdvancedBoard board = new StringBasedBoard(9);
            foreach (var move in GoGuiReader.ReadGoGuiCrap(testGame))
            {
                board = (IAdvancedBoard) board.PlayMove(move);
                System.Diagnostics.Debug.WriteLine(board.ToString());
                var isValidString = ValidStrings(board);
                
                if (!isValidString)
                    Debugger.Break();

                Assert.IsTrue(isValidString);
            }
        }


        public bool ValidStrings(IAdvancedBoard board)
        {
            var allStrings = board.Strings;

            foreach (var s in allStrings)
            {
                foreach (var stone in s.Stones)
                {
                    var testStone = board.BoardVertices[stone];

                    if (testStone != s.Player)
                        return false;
                }

                foreach (var liberty in s.Liberties)
                {
                    var testLiberty = board.BoardVertices[liberty];

                    if (testLiberty != BoardSquares.Empty)
                        return false;
                }
            }

            return true;
        }
    }
}
