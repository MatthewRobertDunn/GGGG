using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using GGGG.Interface;

namespace GGGG
{
    public class MiniMax
    {

        private BoardEvaluator evaluator = new BoardEvaluator();
        private MoveGenerator generator = new MoveGenerator();
        private static int BoardsEvaluated;
        private static DateTime last = DateTime.Now;
        public Tuple<Board, double> GetBestMove(int depth, Board board, BoardSquares player)
        {
            //Console.WriteLine(board.ToString());
            //Console.ReadLine();
            if (depth <= 0)
            {
                System.Threading.Interlocked.Increment(ref BoardsEvaluated);
                var val = evaluator.EvaluateBoard(board);

                if (BoardsEvaluated % 100000 == 0)
                {
                    double boardsSec = 100000 / (DateTime.Now - last).TotalSeconds;
                    Console.WriteLine("{0} BoardsEvaluated {1:0.00} sec", BoardsEvaluated, boardsSec);
                    Console.WriteLine("Alpha {0}", val);
                    Console.WriteLine(board.ToString());

                    last = DateTime.Now;
                }
                return Tuple.Create(board, val);
            }
            var childMoves = generator.GetValidMoves(board, player);

            double alpha = double.MinValue;
            Board b = null;

            //Parallel.ForEach(childMoves, child =>
            foreach (var child in childMoves)
            {
                var result = GetBestMove(depth - 1, child, player.GetEnemy());
                var oldAlpha = alpha;
                alpha = Math.Max(alpha, -result.Item2);
                if (oldAlpha != alpha)
                {
                    b = result.Item1;
                }
            }
            // );
            return Tuple.Create(b, alpha);
        }
    }
}
