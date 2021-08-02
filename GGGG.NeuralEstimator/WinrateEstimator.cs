using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Encog.Neural.Networks;
using Encog.Persist;
using GGGG.Interface;
using GGGG.Interface.IO;
using ProtoBuf;

namespace GGGG.NeuralEstimator
{
    public class WinrateEstimator
    {
        Dictionary<TrainingKey, BasicNetwork> dict = new Dictionary<TrainingKey, BasicNetwork>();
        public void LoadNeuralNetworks()
        {
            foreach (var network in GetNextTrainingData())
            {
                dict.Add(network.Item1, network.Item2);
            }
        }

        public IEnumerable<Tuple<IFastBoard,double>> GetWinrateEstimates(IFastBoard parent, float komi, BoardSquares player, IList<IFastBoard> moves )
        {
            var key = new TrainingKey() { BoardSize = parent.BoardSize, Komi = komi, Player = player };

            BasicNetwork network;
            dict.TryGetValue(key, out network);

            if (network == null)
                yield break;

            var input = parent.ToNNInput();
            var output = new double[input.Length];

            network.Compute(input, output);


            foreach(var move in moves)
            {
                var estimate = output[move.LastMove.Pos];
                yield return new Tuple<IFastBoard, double>(move,estimate);
            }

        }

        public IEnumerable<Tuple<TrainingKey, BasicNetwork>> GetNextTrainingData()
        {
            foreach (var fileName in Directory.EnumerateFiles(AppFolder.GetPathToAppFolder(), "*.nn"))
            {
                var fileInfo = new FileInfo(fileName);

                var metaData = Path.GetFileNameWithoutExtension(fileName).Split('-');
                BoardSquares player;
                BoardSquares.TryParse(metaData[0], true, out player);
                int boardSize = int.Parse(metaData[1]);
                float komi = float.Parse(metaData[2]);

                var key = new TrainingKey() { BoardSize = boardSize, Komi = komi, Player = player };
                var network = EncogDirectoryPersistence.LoadObject(fileInfo);

                yield return new Tuple<TrainingKey, BasicNetwork>(key, (BasicNetwork)network);
            }
        }
    }
}
