using System.Collections.Generic;

namespace GGGG.Interface
{
    public struct ChosenMoveInfo
    {
        public double WinPercentage;
        public double SampleCount;
        public bool Resign;
        public bool Pass;
        public float Komi;
        public IFastBoard ChosenBoard;
        public IList<ScoredMove> ScoredMoves;
    }
}
