namespace GGGG.Interface
{
    public class ScoredMove
    {
        public MonteCacheEntry CacheEntry;
        public IFastBoard Board;

        public ScoredMove()
        {

        }

        public ScoredMove(MonteCacheEntry cacheEntry, IFastBoard board)
            : this()
        {
            this.CacheEntry = cacheEntry;
            this.Board = board;
        }
    }
}