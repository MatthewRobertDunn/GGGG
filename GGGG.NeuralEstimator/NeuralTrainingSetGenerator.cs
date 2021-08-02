using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using GGGG.Interface;
using GGGG.Interface.IO;
using ProtoBuf;

namespace GGGG.NeuralEstimator
{
    public class NeuralTrainingSetGenerator
    {
        public static bool OnTrainingData(IFastBoard parent, ChosenMoveInfo chosen)
        {
            if (chosen.Pass)
                return true;

            if (chosen.Resign)
                return true;

            TrainingData data = new TrainingData();

            data.InputData = parent.ToNNInput();
            data.OuputData = chosen.ToNNOutput();
            data.NumberStones = parent.BoardVertices.Where(x => x == BoardSquares.White || x == BoardSquares.Black).Count();
            data.Player = chosen.ChosenBoard.LastMove.Color;
            data.BoardSize = chosen.ChosenBoard.BoardSize;
            data.BoardHash = parent.BoardHash.HashValue;
            data.NumberGames = (int)chosen.SampleCount;
            data.Komi = chosen.Komi;

            SaveTrainingData(data);

            return false;
        }

        public static void SaveTrainingData(TrainingData data)
        {
            var path =  AppFolder.GetPathToAppFolder();
            var fileName = Path.Combine(path, NeuralTrainer.FileName);
            
            using (var stream = File.Open(fileName, FileMode.Append, FileAccess.Write, FileShare.Read))
            {
                //Write out training data row
                ProtoBuf.Serializer.SerializeWithLengthPrefix(stream, data, PrefixStyle.Fixed32);
            }
        }
    }
}