namespace GGGG.Interface
{
    public struct MonteCacheEntry
    {
        public double WhiteWon;
        public double BlackWon;

        public double NumberOfGames
        {
            get { return WhiteWon + BlackWon; }
        }

        public double GetWinRate(BoardSquares player)
        {
            if (player == BoardSquares.White)
                return WhiteWon / (double)this.NumberOfGames;
            else
            {
                return BlackWon / (double)this.NumberOfGames;
            }
        }
    }
}