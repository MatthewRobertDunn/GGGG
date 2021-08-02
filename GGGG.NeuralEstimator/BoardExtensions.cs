using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GGGG.Interface;

namespace GGGG.NeuralEstimator
{
    public static class BoardExtensions
    {

        public static double[] ToNNOutput(this ChosenMoveInfo chosen)
        {
            var outputs = new double[chosen.ChosenBoard.BoardVertices.Length];

            foreach (var position in chosen.ScoredMoves)
            {
                outputs[position.Board.LastMove.Pos] = position.CacheEntry.GetWinRate(chosen.ChosenBoard.LastMove.Color);
            }

            return outputs;
        }

        public static double[] ToNNInput(this IFastBoard board)
        {
            var inputs = new double[board.BoardVertices.Length];

            for (int index = 0; index < board.BoardVertices.Length; index++)
            {
                var vertex = board.BoardVertices[index];
                switch (vertex)
                {
                    case BoardSquares.White:
                        inputs[index] = 1;
                        break;
                    case BoardSquares.Black:
                        inputs[index] = 0.75;
                        break;
                    case BoardSquares.Edge:
                        inputs[index] = 0.5;
                        break;
                    case BoardSquares.Empty:
                        inputs[index] = 0.0;
                        break;
                }
            }
            return inputs;
        }
    }

}
