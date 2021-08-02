using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using CSharpTest.Net.Collections;
using GGGG.Interface;

namespace GGGG.Algorithms
{
    public class FastMonteCache
    {
        LurchTable<Int64, MonteCacheEntry> internalCache;

        public FastMonteCache()
        {
            internalCache = new LurchTable<long, MonteCacheEntry>(LurchTableOrder.Access,10000000);
        }


       // [MethodImpl(MethodImplOptions.Synchronized)]
        public void AddEstimate(IFastBoard board, double estimate, double confidence, int currentDepth)
        {
            int max = currentDepth + 20;
            max = Math.Min(max, board.BoardHistory.Count);

            //Int64 hash = board.BoardHistory[board.BoardHistory.Count - 1];
            //AddOrUpdateEstimate(hash, board.LastMove.Color, estimate, confidence);


            for (int i = max - 1; i >= currentDepth; i--)
            {
                Int64 hash = board.BoardHistory[i];
                AddOrUpdateEstimate(hash, board.LastMove.Color, estimate, confidence);
            }

            //AMAF heuristic todo:
            //for (int i = currentDepth; i < max - 1; i++)
            //{
            //    var move = board.MoveHistory[i];
            //    if (move.CaptureOccured)
            //        break;
            //    if (move.Color == topLevelMoves.Player)
            //    {
            //        if (topLevelMoves.TopLevelMoves.ContainsKey(move.Pos))
            //            AddOrUpdateEstimate(topLevelMoves.TopLevelMoves[move.Pos], board.LastMove.Color, estimate, confidence);
            //    }
            //}
        }


       // [MethodImpl(MethodImplOptions.Synchronized)]
        public void CacheBoard(IEnumerable<long> boardHistory, double eval, int boardCount = 1, double worth = 1.0)
        {
            foreach (var hash in boardHistory)
            {
                AddOrUpdateEntry(hash, eval, worth/ boardCount);
            }
        }
        
       // [MethodImpl(MethodImplOptions.Synchronized)]
        public void CacheBoard(IFastBoard board, double eval, int currentDepth,double worth = 1.0)
        {
            int max = currentDepth + 9;
            max = Math.Min(max, board.BoardHistory.Count);

            for (int i = max - 1; i >= currentDepth; i--)
            {
                Int64 hash = board.BoardHistory[i];
                AddOrUpdateEntry(hash, eval);
            }
        }

        public MonteCacheEntry GetCachedBoardEvals(Int64 hash)
        {
            MonteCacheEntry result;
            if (internalCache.TryGetValue(hash, out result))
                return result;
            return result;
        }


        public void AddOrUpdateEstimate(Int64 hash, BoardSquares player, double estimate, double confidence)
        {
            ////MonteCacheEntry item;
            ////internalCache.TryGetValue(hash, out item);
            //double whiteIncrease = 0;
            //double blackIncrease = 0;
            //if (player == BoardSquares.White)
            //{
            //    whiteIncrease += (estimate * confidence);   //0.7 * 100 white won 70 games
            //    blackIncrease += ((1.0 - estimate) * confidence);  //0.3 * 100, black won 30 games
            //}

            //if (player == BoardSquares.Black)
            //{
            //    blackIncrease += (estimate * confidence);   //0.7 * 100 black won 70 games
            //    whiteIncrease += ((1.0 - estimate) * confidence);  //0.3 * 100, black won 30 games
            //}

            //internalCache.AddOrUpdate(hash, new MonteCacheEntry(){BlackWon = blackIncrease,WhiteWon = whiteIncrease},
            //    (x,y)
            //        =>new MonteCacheEntry(BL))
        }


        private void AddOrUpdateEntry(Int64 hash, double eval, double worth = 1.0)
        {
            double whiteIncrease = 0;
            double blackIncrease = 0;

            if (eval > 0)
                whiteIncrease += (worth + (eval / 10000000000.0));
            else if (eval < 0)
                blackIncrease += (worth + ((-eval) / 10000000000.0));


            var newItem = new MonteCacheEntry() { BlackWon = blackIncrease, WhiteWon = whiteIncrease };

            internalCache.AddOrUpdate(hash, newItem, (x, y) =>
                new MonteCacheEntry()
                    {
                        BlackWon = y.BlackWon + blackIncrease,
                        WhiteWon = y.WhiteWon + whiteIncrease
                    }
                );
        }

    }
}
