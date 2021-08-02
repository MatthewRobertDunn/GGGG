using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GGGG.Algorithms;
using GGGG.Interface;
using GGGG.MarkAndSweep;
using GGGG.NeuralEstimator;
using GGGG.Threading;
using GGGG.Twister;
using MoreLinq;

namespace GGGG
{
    public class UCTSearch
    {
        const double UCTK = 1.0;
        private FastMonteCache monteCache;
        public static FastMoveGenerator generator = new FastMoveGenerator();
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger("GtpEngineAdapter");
        private float komi = 0.0f;
        //private double dynamicKomi = 0.0;
        private bool passEarly;
        private bool dontResign;
        //public double[] magicBoard;

        WinrateEstimator estimator = new WinrateEstimator();

        BoardCache<object> neuralCache = new BoardCache<object>();

        public UCTSearch()
        {
            monteCache = new FastMonteCache();
            estimator.LoadNeuralNetworks();
        }

        public UCTSearch(FastMonteCache cache, float komi, bool passEarly, bool dontResign)
            : this()
        {

            this.monteCache = cache;
            this.komi = komi;
            this.passEarly = passEarly;
            this.dontResign = dontResign;
        }



        public UCTSearch(float komi, bool passEarly, bool dontResign)
            : this()
        {
            this.komi = komi;
            this.passEarly = passEarly;
            this.dontResign = dontResign;

        }

        public static bool IsThinking = false;
        private Task thinkingTask = null;

        public void StartThinking(IFastBoard parent, BoardSquares player)
        {
            //if (magicBoard == null)
            // magicBoard = new double[parent._boardVertices.Length];

            StopThinking();
            Log.Info("Started thinking");
            IsThinking = true;
            var currentDepth = parent.BoardHistory.Count - 1;
            //Make a dictionary of all top level moves
            var moves = generator.GetValidMoves(parent, player);

            Dictionary<int, Int64> tMoves = moves.Where(fastBoard => fastBoard.LastMove.Pos != 0).ToDictionary(fastBoard => fastBoard.LastMove.Pos, fastBoard => fastBoard.BoardHash.HashValue);
            AMAFTopLevel topLevelMoves = new AMAFTopLevel() { Player = player, TopLevelMoves = tMoves };

            var task = new Task(() => MyParallel.While(() => IsThinking, x => PlaySimulation(parent, player, currentDepth, topLevelMoves)), TaskCreationOptions.LongRunning);
            task.Start();
            thinkingTask = task;
        }

        public void StopThinking()
        {
            if (thinkingTask == null)
                return;
            IsThinking = false;
            //Wait until we really stopped thinking
            while (!thinkingTask.IsCompleted)
            {
                System.Threading.Thread.Sleep(10);
            }


            if (thinkingTask.Exception != null)
            {
                Log.Error("Error during simulation", thinkingTask.Exception);
            }
        }


        public ChosenMoveInfo GetBestMove(TimeSpan timeSpan, IFastBoard board, BoardSquares boardSquares)
        {
            StartThinking(board, boardSquares);
            System.Threading.Thread.Sleep(timeSpan);
            var move = GetBestMove(board, boardSquares);
            StopThinking();
            return move;
        }

        public ChosenMoveInfo GetBestMove(IFastBoard parent, BoardSquares player)
        {

            ChosenMoveInfo chosenMoveInfo = new ChosenMoveInfo();

            Log.InfoFormat("Board score {0}", parent.CalcScore(komi));

            var moves = generator.GetValidMoves(parent, player);

            var scores = (from child in moves
                          let cacheKey = child.BoardHash.HashValue
                          let cacheEntry = monteCache.GetCachedBoardEvals(cacheKey)
                          where cacheEntry.NumberOfGames > 0
                          select new ScoredMove(cacheEntry, child)).ToList();

            if (scores.Count == 0)
            {
                throw new Exception("No moves left; this shouldn't happen");
            }


            PrintUTCDebugInfo(scores, player);

            var sortedInfo = scores.OrderByDescending(x => x.CacheEntry.NumberOfGames).ToList();
            chosenMoveInfo.ChosenBoard = sortedInfo.Select(x => x.Board).FirstOrDefault();
            chosenMoveInfo.ScoredMoves = scores;

            //if pass early is not set don't select another move in preference to pass
            if (!passEarly && chosenMoveInfo.ChosenBoard.LastMove.Pos == 0 && sortedInfo.Count > 1)
            {
                chosenMoveInfo.ChosenBoard = sortedInfo.Skip(1).Select(x => x.Board).FirstOrDefault();
            }

            double winrate = sortedInfo.Select(x => x.CacheEntry).First().GetWinRate(player);

            chosenMoveInfo.WinPercentage = winrate;
            chosenMoveInfo.SampleCount = sortedInfo.Select(x => x.CacheEntry).First().NumberOfGames;

            //Check if best winrate is 0 if so resign
            if (winrate < Properties.UctSearch.Default.ResignWinrate && !dontResign)
            {
                {
                    Log.Info("Choosing to resign, winrate too low");
                    chosenMoveInfo.ChosenBoard = parent;
                    chosenMoveInfo.Resign = true;
                }

            }

            if (passEarly)
            {
                if (chosenMoveInfo.WinPercentage > Properties.UctSearch.Default.PassEarlyWinrate && parent.LastMove.Pos == 0)
                {
                    chosenMoveInfo.ChosenBoard = parent;
                    chosenMoveInfo.Pass = true;
                }
            }

            //PrintMagicBoard();
            Log.InfoFormat("Chosen move {0}", chosenMoveInfo.ChosenBoard.LastMove.Pos);
            Log.InfoFormat("Capture cache hits {0} lies {1}", FastMonteCarloFunctions.capturesInfoUsedCount, FastMonteCarloFunctions.captureLies);
            FastMonteCarloFunctions.capturesInfoUsedCount = 0;
            FastMonteCarloFunctions.captureLies = 0;
            if (chosenMoveInfo.ChosenBoard.LastMove.Pos == 0)
            {
                chosenMoveInfo.Pass = true;
            }

            chosenMoveInfo.Komi = (float)komi;
            NeuralTrainingSetGenerator.OnTrainingData(parent, chosenMoveInfo);
            return chosenMoveInfo;
        }

        private void PrintUTCDebugInfo(IEnumerable<ScoredMove> scores, BoardSquares player)
        {
            var sortedInfo = scores.OrderByDescending(x => x.CacheEntry.GetWinRate(player));
            Log.Info("Top 5 moves by winrate");
            sortedInfo.Take(5).ForEach(x => Log.InfoFormat("Move {0} winrate {1}", x.Board.LastMove.Pos, x.CacheEntry.GetWinRate(player)));
            Log.InfoFormat("Top 5 moves by num games");
            var gamesSort = sortedInfo.OrderByDescending(x => x.CacheEntry.NumberOfGames);
            gamesSort.Take(5).ForEach(x => Log.InfoFormat("Move {0} count {1}", x.Board.LastMove.Pos, x.CacheEntry.NumberOfGames));
        }

        private object neuralLock = new object();
        public void DoNeuralEstimates(IFastBoard parent, BoardSquares player, int currentDepth, IList<IFastBoard> moves)
        {

            //Don't do it too deep
            if (parent.MoveHistory.Count - currentDepth > 3 || parent.MoveHistory.Count > 25)
                return;

            //If we have done it before don't do it again
            if (neuralCache.GetItem(parent.BoardHash.HashValue) != null)
                return;


            Monitor.Enter(neuralLock);
            try
            {
                //If we have done it before don't do it again
                if (neuralCache.GetItem(parent.BoardHash.HashValue) != null)
                    return;

                var estimates = estimator.GetWinrateEstimates(parent, komi, player, moves).ToArray();
                //.OrderByDescending(x => x.Item2)
                //.Take().ToArray();

                foreach (var estimate in estimates)
                {
                    double worth = 1.0 / estimates.Length;
                    float nnScoreAdjust = 1.0f;
                    monteCache.AddEstimate(estimate.Item1, Math.Max(0, estimate.Item2 - 0.1), 0.1, currentDepth);
                }
                neuralCache.AddItem(parent.BoardHash.HashValue, new object());
            }
            finally
            {
                Monitor.Exit(neuralLock);
            }
        }

        public IFastBoard UctSelect(IFastBoard parent, BoardSquares player)
        {
            var moves = generator.GetValidMoves(parent, player).ToList();
            //if (MyRandom.Random.NextDouble() > 0.8)
            //DoNeuralEstimates(parent, player, currentDepth, moves);

            var cacheParentEntry = monteCache.GetCachedBoardEvals(parent.BoardHistory.Last());
            var uctMoveList = moves
                        .Select(x =>
                         {
                             var cacheEntryChild = monteCache.GetCachedBoardEvals(x.BoardHash.HashValue);
                             double uctValue = MyRandom.Random.NextDouble();
                             if (cacheEntryChild.NumberOfGames > 0)
                             {
                                 var winRate = cacheEntryChild.GetWinRate(player);
                                 var uct = UCTK *
                                           Math.Sqrt(Math.Log(cacheParentEntry.NumberOfGames) /
                                                     (5 * cacheEntryChild.NumberOfGames));
                                 uctValue = winRate + uct;
                             }

                             bool isInCache = cacheEntryChild.NumberOfGames != 0;

                             //Temporary boost to exploring capture moves
                             if (x.LastMove.CaptureOccured && cacheEntryChild.NumberOfGames < 50)
                             {
                                 uctValue += 0.1;
                             }

                             return
                                 new
                                     {
                                         GoMove = x,
                                         UtcValue = uctValue,
                                         IsInCache = isInCache
                                     };
                         }).ToList();

            uctMoveList = uctMoveList.OrderByDescending(x => x.UtcValue).ToList();
            var r = uctMoveList.First();
            if (!this.passEarly && r.GoMove.LastMove.Pos == 0 && uctMoveList.Count > 1)
            {
                r = uctMoveList.Skip(1).First();
            }
            return r.GoMove;
        }


        public int AddSuperMoves(IFastBoard parent, IFastBoard simulation, double eval, int currentDepth, double worth = 1.0)
        {
            //First determine what level we should start the rave?

            var startingHash = parent.BoardHistory
                                .Skip(currentDepth)
                                .Where(x => monteCache.GetCachedBoardEvals(x).NumberOfGames < 100)
                                .FirstOrDefault();

            var startIndex = parent.BoardHistory.IndexOf(startingHash) + 1;

            var movePerms = GetRavePermutations(parent, simulation, startIndex).ToList();

            foreach (var movePerm in movePerms)
            {
                var history = GetBoardHistoryForMove(parent.BoardHash.HashValue, movePerm);
                monteCache.CacheBoard(new[] { history }, eval, 1, worth);
            }

            return movePerms.Count;
        }

        public long GetBoardHistoryForMove(long parentHash, GoMove move)
        {
            ZobristHash boardHash = new ZobristHash(parentHash);
            boardHash.Delta(move.Color, move.Pos);
            return boardHash.HashValue;
        }

        public IEnumerable<GoMove> GetRavePermutations(IFastBoard parent, IFastBoard simulation, int startIndex)
        {
            return
                     simulation.MoveHistory.Skip(startIndex)
                     .TakeWhile(x => !x.CaptureOccured)
                     .Where(x => x.Color == parent.LastMove.Color.GetEnemy())
                     .Take(1000);
        }

        public IEnumerable<IList<GoMove>> GetBoardPermutations(IFastBoard parent, IFastBoard simulation)
        {
            //get list of moves up to capture?

            var firstMoves =
                simulation.MoveHistory.Skip(parent.MoveHistory.Count)
                                .TakeWhile(x => !x.CaptureOccured)
                                .Where(x => x.Color == parent.LastMove.Color.GetEnemy())
                                .Take(2)
                                .ToList();

            var secondMoves =
                simulation.MoveHistory.Skip(parent.MoveHistory.Count)
                                .TakeWhile(x => !x.CaptureOccured)
                                .Where(x => x.Color == parent.LastMove.Color)
                                .Take(firstMoves.Count)
                                .ToList();

            if (firstMoves.Count > secondMoves.Count)
            {
                firstMoves = firstMoves.Take(secondMoves.Count).ToList();
            }

            if (firstMoves.Count == 0 || secondMoves.Count == 0)
                yield break;


            //Get permutations of first moves


            var firstPerms = firstMoves.PermutateUnique().ToArray();
            var secondPerms = secondMoves.PermutateUnique().ToArray();



            for (int i = 0; i < firstPerms.Length; i++)
            {
                var firstMoveSet = firstPerms[i].ToArray();
                var secondMoveSet = secondPerms[i].ToArray();

                var result = new GoMove[firstMoveSet.Length * 2];

                for (int k = 0; k < firstMoveSet.Length * 2; k = k + 2)
                {
                    result[k] = firstMoveSet[k / 2];
                    result[k + 1] = secondMoveSet[k / 2];
                }

                yield return result;
            }

        }


        public void PlaySimulation(IFastBoard parent, BoardSquares player, int currentDepth, AMAFTopLevel topLevelMoves, bool lastNull = false)
        {
            var cacheParentEntry = monteCache.GetCachedBoardEvals(parent.BoardHistory.Last());
            //If parent has no entries, just play a random game from this board
            if (cacheParentEntry.NumberOfGames > 0)
            {
                //If parent has entries, there might be a better move to play
                var next = UctSelect(parent, player);
                if (!(next.LastMove.Pos == 0 && lastNull))
                {
                    if (next.LastMove.Pos == 0)
                    {
                        PlaySimulation(next, player.GetEnemy(), currentDepth, topLevelMoves, true); //play a null
                    }
                    else
                        PlaySimulation(next, player.GetEnemy(), currentDepth, topLevelMoves);
                }
                else
                {
                    monteCache.CacheBoard(next.BoardHistory.Skip(currentDepth).Take(parent.BoardHistory.Count - currentDepth + 1), next.CalcScore(komi), 1, 1.0);
                }
            }
            else
            {
                for (int i = 0; i < 5; i++)
                {

                    var pinDropInfo = new PinDropParams(parent, player);
                    pinDropInfo.MaxDepth = 500;

                    if (parent.LastMove.Pos == 0 && parent.BoardHistory.Count > 1)
                    {
                        pinDropInfo.CheckPass = true;
                        pinDropInfo.Player = pinDropInfo.Player.GetEnemy();
                    }

                    //var testBoard = MarkAndSweepSimulation.PlayMarkAndSweepSimulation(pinDropInfo);

                    var testBoard = FastMonteCarloFunctions.GetRandomBoardAtEnd(pinDropInfo);

                    int superMoves = AddSuperMoves(parent, testBoard.Board, testBoard.CalcScore(komi), currentDepth, 0.1);
                    //If no supermoves add manually
                    //if (superMoves == 0)
                    //    monteCache.CacheBoard(testBoard.Board.BoardHistory.Skip(currentDepth).Take(parent.BoardHistory.Count - currentDepth + 1), testBoard.CalcScore(komi), 1, 1.0);
                    //else
                    monteCache.CacheBoard(testBoard.Board.BoardHistory.Skip(currentDepth).Take(parent.BoardHistory.Count - currentDepth + 1), testBoard.CalcScore(komi), 1, 1.0);
                }
            }
        }

        public double EstimateScore(IFastBoard board, BoardSquares player)
        {
            var finalBoard = SelfPlay(board, TimeSpan.FromSeconds(1), player);
            return finalBoard.CalcScore(komi);
        }


        public IEnumerable<int> GetDeadStones(IFastBoard board, BoardSquares player)
        {
            var finalBoard = SelfPlay(board, TimeSpan.FromSeconds(1), player);

            for (int i = 0; i < board.BoardVertices.Length; i++)
            {
                var vertex = board.BoardVertices[i];

                if (vertex == BoardSquares.White || vertex == BoardSquares.Black)
                {
                    //if its not still there on the final board its dead
                    if (finalBoard.BoardVertices[i] == BoardSquares.Empty)
                    {
                        yield return i;
                        continue;
                    }

                    //if any moves have been played in its position after the game ended its dead

                    var movesPlayed = finalBoard.MoveHistory.Skip(board.MoveHistory.Count).Any(x => x.Pos == i);

                    if (movesPlayed)
                    {
                        yield return i;
                        continue;
                    }
                }
            }
        }

        private static MemoryCache cache = new MemoryCache("Scores");
        public IFastBoard SelfPlay(IFastBoard board, TimeSpan timePerMove, BoardSquares player, Func<IFastBoard, ChosenMoveInfo, bool> moveCallBack = null)
        {
            var cacheEntry = cache.Get(board.BoardHash.ToString());

            if (cacheEntry != null)
                return (IFastBoard)cacheEntry;


            UCTSearch search = new UCTSearch(this.monteCache, this.komi, false, true);

            bool lastPass = false;
            while (true)
            {
                var move = search.GetBestMove(timePerMove, board, player);
                if (moveCallBack != null)
                    if (moveCallBack(board, move))
                        return move.ChosenBoard;

                if (move.Pass)
                {
                    if (lastPass)
                    {
                        cache.Add(board.BoardHash.ToString(), move.ChosenBoard, DateTimeOffset.Now.AddMinutes(30));
                        return move.ChosenBoard;
                    }

                    lastPass = true;
                }
                else
                    lastPass = false;
                board = move.ChosenBoard;
                player = player.GetEnemy();
            }
        }
    }
}
