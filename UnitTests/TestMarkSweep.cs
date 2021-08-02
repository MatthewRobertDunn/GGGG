using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GGGG;
using GGGG.Interface;
using GGGG.MarkAndSweep;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace GGGGGoText
{
    [TestClass]
    public class TestMarkSweep
    {
        [TestMethod]
        public void TestPindrop()
        {
            var board = BoardFactory.GetBoard(9);


            Stopwatch watch = new Stopwatch();
            watch.Start();

            for (int i = 0; i < 10000; i++)
            {
                var nb = board.Clone();

                var p = new PinDropParams(nb, BoardSquares.Black);
                p.MaxDepth = 150;
                var result = MarkAndSweepSimulation.PlayMarkAndSweepSimulation(p);

                //System.Diagnostics.Debug.WriteLine(result.Board);
            }

            watch.Stop();

        }
    }
}
