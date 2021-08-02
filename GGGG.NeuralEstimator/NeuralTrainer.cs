using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Encog.Engine.Network.Activation;
using Encog.ML.Data;
using Encog.MathUtil.Randomize;
using Encog.Neural.Data.Basic;
using Encog.Neural.Networks;
using Encog.Neural.Networks.Layers;
using Encog.Neural.Networks.Training;
using Encog.Neural.Networks.Training.Propagation.Resilient;
using Encog.Neural.NeuralData;
using Encog.Persist;
using Encog.Util.Simple;
using GGGG.Interface;
using GGGG.Interface.IO;
using ProtoBuf;

namespace GGGG.NeuralEstimator
{
    public class NeuralTrainer
    {
        public Dictionary<TrainingKey, IList<TrainingData>> trainingData = new Dictionary<TrainingKey, IList<TrainingData>>();
        public static string FileName = Properties.NeuralEstimator.Default.FileName;
        public static string Extention = ".gtrain";


        public void LoadTrainingData()
        {
            Console.WriteLine("Loading training data");
            int count = 0;
            foreach (var item in GetNextTrainingData())
            {
                //try to remove junky crap
                if (item.NumberGames < 100)
                    continue;

                if (item.NumberGames > 50000)
                    continue;
                int realBoardSize = item.BoardSize - 2;
                if (item.NumberStones > 20)
                    continue;

                var key = new TrainingKey
                              {
                                  BoardSize = item.BoardSize,
                                  NumberStones = item.NumberStones,
                                  Player = item.Player,
                                  Komi = item.Komi
                              };

                if (!trainingData.ContainsKey(key))
                    trainingData[key] = new List<TrainingData>();


                var existingBoard = trainingData[key].FirstOrDefault(x => x.BoardHash == item.BoardHash);

                if (existingBoard == null)
                {
                    count++;
                    trainingData[key].Add(item);
                }
                else
                {
                    if (item.NumberGames > existingBoard.NumberGames)
                    {
                        trainingData[key].Remove(item);
                        trainingData[key].Add(item);
                    }
                }




            }
        }


        public void TrainOnAllData()
        {
            for (float komi = 0; komi < 20; komi += 0.5f)
                foreach (var player in new BoardSquares[] { BoardSquares.White, BoardSquares.Black, })
                    foreach (var size in new int[] { 11, 13, 15, 21 })
                    {
                        var key = new TrainingKey() { BoardSize = size, Player = player, Komi = komi };
                        var data = GetTrainingData(key, 0, 30);
                        if (data.Count < 100)
                            continue;

                        Console.WriteLine("Training network {0} {1} {2}", key.Player, key.BoardSize, key.Komi);
                        var network = TrainNetwork(data, size);
                        var fileName = string.Format("{0}-{1}-{2}", key.Player, key.BoardSize, key.Komi);
                        SaveNetwork(network, fileName + ".nn");
                    }
        }


        public void SaveNetwork(BasicNetwork network, string fileName)
        {
            var fullPath = Path.Combine(AppFolder.GetPathToAppFolder(), fileName);
            EncogDirectoryPersistence.SaveObject(new FileInfo(fullPath), network);
        }




        public IList<TrainingData> GetTrainingData(TrainingKey key, int startStones, int endStones)
        {
            IList<TrainingData> inputRows = new List<TrainingData>();

            for (int i = startStones; i < endStones; i++)
            {
                key.NumberStones = i;
                if (trainingData.ContainsKey(key))
                {
                    foreach (var item in trainingData[key])
                        inputRows.Add(item);
                }

            }

            return inputRows.ToList();
        }

        public BasicNetwork TrainNetwork(IList<TrainingData> inputList, int boardSize)
        {
            int realBoardSize = boardSize - 2;
            //Create new simple layered neural network
            BasicNetwork network = new BasicNetwork();
            //121 inputs (for 11x11 go board)
            network.AddLayer(new BasicLayer(new ActivationSigmoid(), true, boardSize * boardSize));
            //2 middle processing layers
            network.AddLayer(new BasicLayer(new ActivationSigmoid(), true, (int)(realBoardSize * Properties.NeuralEstimator.Default.HiddenLayer1NeuronMultiplier)));
            network.AddLayer(new BasicLayer(new ActivationLOG(), true, (int)(realBoardSize * Properties.NeuralEstimator.Default.HiddenLayer1NeuronMultiplier)));
            //121 outputs
            network.AddLayer(new BasicLayer(new ActivationSigmoid(), true, boardSize * boardSize));

            //finalize network to allow acceleration 
            network.Structure.FinalizeStructure();
            //randomize weights before training
            network.Reset();

            var input = new double[inputList.Count][];
            var output = new double[inputList.Count][];

            for (int i = 0; i < inputList.Count; i++)
            {
                var data = inputList[i];
                input[i] = data.InputData;
                output[i] = data.OuputData;
            }
            INeuralDataSet trainingSet = new BasicNeuralDataSet(input, output);
            ITrain train = new ResilientPropagation(network, trainingSet);

            //EncogUtility.TrainToError(train,0.01);

            int epoch = 0;
            do
            {
                train.Iteration();
                string foo = "Epoch #" + epoch + " Error:" + train.Error;
                Console.WriteLine(foo);

                System.Diagnostics.Debug.WriteLine(foo);
                epoch++;
            } while ((epoch < Properties.NeuralEstimator.Default.MaxTrainingIterations) && (train.Error > Properties.NeuralEstimator.Default.DesiredError));


            return network;
        }


        public IEnumerable<TrainingData> GetNextTrainingData()
        {
            int count = 0;
            foreach (var fileName in Directory.EnumerateFiles(AppFolder.GetPathToAppFolder(), "*" + Extention))
            {
                using (var stream = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    var results = ProtoBuf.Serializer.DeserializeItems<TrainingData>(stream, PrefixStyle.Fixed32, 0);
                    foreach (var trainingData in results)
                    {
                        count++;
                        yield return trainingData;
                    }
                }

            }
        }
    }
}
