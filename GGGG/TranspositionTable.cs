using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Caching;
using System.Collections.Specialized;
using GGGG.Interface;

namespace GGGG
{
    public class TranspositionTable
    {
        private MemoryCache cache;

        private CacheItemPolicy policy = new CacheItemPolicy();

        public int CacheHits;
        public TranspositionTable()
        {
            //Create a name / value pair for properties
            var config = new NameValueCollection();
            config.Add("pollingInterval", "00:02:05");
            config.Add("physicalMemoryLimitPercentage", "0");
            config.Add("cacheMemoryLimitMegabytes", "800");

            //instantiate cache
            cache = new MemoryCache("CustomCache", config);
        }

        public void AddBoardToCache(Board board, double eval)
        {
            cache.Add(board.MovesList.GetHashCode().ToString(), eval, policy);
        }


        public double GetEntry(string movesList, BoardSquares player)
        {
            var result = cache.Get(movesList.GetHashCode().ToString());

            if (result != null)
            {
                CacheHits++;
                return (double)result;
            }
            
            if (player == BoardSquares.White)
                return double.MinValue;
            else
                return double.MaxValue;
        }

    }



    public class TranspositionEntry
    {
        public Board Board;
        public double Eval;
    }
}
