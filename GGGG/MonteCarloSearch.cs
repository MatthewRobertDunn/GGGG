using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GGGG.Interface;
using MoreLinq;
using System.Threading.Tasks;
using GGGG.Twister;
using GGGG.Algorithms;
namespace GGGG
{
    //public class MonteCarloSearch
    //{
    //    private BoardEvaluator evaluator = new BoardEvaluator();
    //    private MoveGenerator generator = new MoveGenerator();
    //    private FastMonteCache monteCache = new FastMonteCache();
    //    private PosHeuristic[] moveHeuristics = new PosHeuristic[(9 + 2) * (9 + 2)];
    //    private static DateTime last = DateTime.Now;
    //    private static readonly log4net.ILog Log = log4net.LogManager.GetLogger("GtpEngineAdapter");

    //    private Random r = new Random();

    //    public MonteCarloSearch()
    //    {
    //        for (int i = 0; i < moveHeuristics.Length; i++)
    //        {
    //            moveHeuristics[i] = new PosHeuristic();
    //        }
    //    }

    //    public Tuple<Board, double> GetBestMove(int numRandomGames, Board board, BoardSquares player)
    //    {
    //        var boards = generator.GetValidMoves(board, player);


    //        List<Board> sampleBoards = new List<Board>(numRandomGames);

    //        double averageBoards = 0.0;

    //        List<Tuple<Board, double>> results = new List<Tuple<Board, double>>();


    //        foreach (var b in boards)
    //        {
    //            FillRandomGamesForBoard(b, numRandomGames, sampleBoards, player);
    //            Parallel.ForEach(sampleBoards, s =>
    //            //foreach (var s in sampleBoards)
    //            {
    //                var eval = evaluator.EvaluateBoard(s);
    //                monteCache.CacheBoard(s, eval, board.BoardHistory.Count);
    //            }
    //            );

    //            var cacheEntry = monteCache.GetCachedBoardEvals(b.BoardHash);

    //            double evalAvg = (cacheEntry.WhiteWon - cacheEntry.BlackWon) / (double)cacheEntry.NumberOfGames;

    //            averageBoards += cacheEntry.NumberOfGames;

    //            results.Add(Tuple.Create(b, evalAvg));
    //            sampleBoards.Clear();
    //        }

    //        averageBoards = averageBoards / boards.Count();

    //        Log.InfoFormat("Average sample size per move {0}", averageBoards);

    //        if (results.Count == 0)
    //        {
    //            return new Tuple<Board, double>(null, 0);
    //        }

    //        if (player == BoardSquares.White)
    //        {
    //            return results.MaxBy(x => x.Item2);
    //        }
    //        else
    //        {
    //            return results.MinBy(x => x.Item2);
    //        }


    //    }


    //    public Board GetRandomBoardAtEnd(Board board, BoardSquares player)
    //    {
    //        var moves = generator.GetMoveList(board, player).ToList();

    //        moves.Shuffle();

    //        Board child = null;

    //        foreach (var move in moves)
    //        {
    //            if (ShouldSkipMoveHeuristic(player, move))
    //                continue;

    //            Board test = board.PlayMove(move.Pos, move.Color);

    //            if (Board.MoveIsValid(board, test, player))
    //            {
    //                child = test;
    //                break;
    //            }
    //        }

    //        if (child == null)
    //            return GetRandomBoardAtEndWithoutHeuristics(board, player);  //If no more, try playing game without heuristics
    //        else
    //            return GetRandomBoardAtEnd(child, player.GetEnemy());

    //    }

    //    private bool ShouldSkipMoveHeuristic(BoardSquares player, GoMove move)
    //    {
    //        bool shouldSkipMove = false;

    //        var heuristic = moveHeuristics[move.Pos];

    //        var testRand = MyRandom.Random.NextDouble();

    //        double k = 20;

    //        if (heuristic.WhiteWon != 0 && heuristic.BlackWon != 0)
    //        {
    //            if (player == BoardSquares.White)
    //            {
    //                if (testRand > (heuristic.WhiteWon + k) / (heuristic.BlackWon + k))
    //                    shouldSkipMove = true;
    //            }

    //            if (player == BoardSquares.Black)
    //            {
    //                if (testRand > (heuristic.BlackWon + k) / (heuristic.WhiteWon + k))
    //                    shouldSkipMove = true;
    //            }
    //        }
    //        return shouldSkipMove;
    //    }


    //    public Board GetRandomBoardAtEndWithoutHeuristics(Board board, BoardSquares player, bool lastNull = false)
    //    {
    //        var moves = generator.GetMoveList(board, player).ToList();

    //        moves.Shuffle();

    //        Board child = null;

    //        foreach (var move in moves)
    //        {
    //            Board test = board.PlayMove(move.Pos, move.Color);
    //            if (Board.MoveIsValid(board, test, player))
    //            {
    //                child = test;
    //                break;
    //            }
    //        }

    //        if (child == null && lastNull)
    //        {
    //            return board;
    //        }

    //        if (child == null)
    //            return GetRandomBoardAtEndWithoutHeuristics(board, player.GetEnemy(), true); //play a null
    //        else
    //            return GetRandomBoardAtEndWithoutHeuristics(child, player.GetEnemy());

    //    }


    //    public Board GetRandomBoardAtDepth(Board board, int depth, BoardSquares player)
    //    {
    //        if (depth == 0)
    //            return board;

    //        var moves = generator.GetMoveList(board, player).ToList();

    //        moves.Shuffle();

    //        Board child = null;

    //        foreach (var move in moves)
    //        {
    //            Board test = board.PlayMove(move.Pos, move.Color);

    //            if (Board.MoveIsValid(board, test, player))
    //            {
    //                child = test;
    //                break;
    //            }
    //        }

    //        if (child == null)
    //            return null;

    //        return GetRandomBoardAtDepth(child, depth - 1, player.GetEnemy());
    //    }

    //    public void FillRandomGamesForBoard(Board board, int numRandomGames, List<Board> collection, BoardSquares player)
    //    {
    //        Parallel.For(0, numRandomGames, delegate(int i)
    //        // while (collection.Count < numRandomGames)
    //        {
    //            //int depth = MyRandom.Random.Next(1, 20);
    //            var chosenBoard = GetRandomBoardAtEnd(board, player);
    //            if (chosenBoard != null)
    //                collection.Add(chosenBoard);
    //        }
    //        );
    //    }
    //}


    public class PosHeuristic
    {
        public double WhiteWon;
        public double BlackWon;
        public double Draw;
    }
}
