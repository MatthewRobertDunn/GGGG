using GGGG.Interface;

namespace GGGG.NeuralEstimator
{
    public struct TrainingKey
    {
        public BoardSquares Player;
        public int BoardSize;
        public float Komi;
        public int NumberStones;
    }
}