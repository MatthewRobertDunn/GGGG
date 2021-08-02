using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using GGGG.Interface;

namespace GGGG
{
    public class AlphaBetaSearch
    {
        private BoardEvaluator evaluator = new BoardEvaluator();
        private MoveGenerator generator = new MoveGenerator();
        private TranspositionTable table = new TranspositionTable();
        private static int BoardsEvaluated;
        private static DateTime last = DateTime.Now;
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger("GtpEngineAdapter");

        public Tuple<Board, double> GetBestMove(int depth, Board board, double alpha, double beta, BoardSquares player)
        {
            BoardsEvaluated = 0;
            table.CacheHits = 0;
            Stopwatch watch = new Stopwatch();
            watch.Start();
            var result = GetBestMoveInternal(depth, board, alpha, beta, player);
            watch.Stop();
            Log.InfoFormat("Nodes searched {0} cache hits {1} took {2}", BoardsEvaluated, table.CacheHits, watch.ElapsedMilliseconds / 1000);
            return result;
        }


        private Tuple<Board, double> GetBestMoveInternal(int depth, Board board, double alpha, double beta, BoardSquares player)
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
                    Log.InfoFormat("{0} BoardsEvaluated {1:0.00} sec", BoardsEvaluated, boardsSec);
                    Log.InfoFormat("Alpha {0}", val);
                    Log.Info(board.ToString());
                    last = DateTime.Now;
                }

                return Tuple.Create(board, val);
            }
            var childMoves = generator.GetMoveList(board, player).ToList();

            Board chosenBoard = null;

            if (player == BoardSquares.White)
            {
                IEnumerable<GoMove> orderedMoves;
                if (depth != 1 && depth != 2)
                    orderedMoves = childMoves.OrderByDescending(x => table.GetEntry(board.MovesList + x.ToString(), player));
                else
                    orderedMoves = childMoves;

                foreach (var childMove in orderedMoves)
                {
                    var childBoard = board.PlayMove(childMove.Pos, player);
                    if (!Board.MoveIsValid(board, childBoard, player))
                        break;

                    var result = GetBestMoveInternal(depth - 1, childBoard, alpha, beta, player.GetEnemy());


                    var oldAlpha = alpha;
                    alpha = Math.Max(alpha, result.Item2);
                    if (oldAlpha != alpha)
                    {
                        chosenBoard = childBoard;
                    }
                    if (beta <= alpha)
                        break;
                }
                if (chosenBoard != null)
                    table.AddBoardToCache(chosenBoard, alpha);

                return Tuple.Create(chosenBoard, alpha);
            }
            else
            {
                IEnumerable<GoMove> orderedMoves;
                if (depth != 1 && depth != 2)
                    orderedMoves = childMoves.OrderByDescending(x => table.GetEntry(board.MovesList + x.ToString(), player));
                else
                    orderedMoves = childMoves;
                foreach (var childMove in orderedMoves)
                {
                    var childBoard = board.PlayMove(childMove.Pos, player);

                    if (!Board.MoveIsValid(board, childBoard, player))
                        break;

                    var result = GetBestMoveInternal(depth - 1, childBoard, alpha, beta, player.GetEnemy());
                    //table.AddBoardToCache(result.Item1, result.Item2);
                    var oldBeta = beta;
                    beta = Math.Min(beta, result.Item2);
                    if (oldBeta != beta)
                    {
                        chosenBoard = childBoard;
                    }
                    if (beta < alpha)
                        break;
                }
                if (chosenBoard != null)
                    table.AddBoardToCache(chosenBoard, beta);

                return Tuple.Create(chosenBoard, beta);
            }
            //);

        }
    }
}
