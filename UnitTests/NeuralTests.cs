using System;
using GGGG.NeuralEstimator;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class NeuralTests
    {
        [TestMethod]
        public void NeuralGenerateSet()
        {
            NeuralTrainingSetGenerator trainingSetGenerator = new NeuralTrainingSetGenerator();

        }

        [TestMethod]
        public void NeuralTrainTest()
        {
            var learner = new NeuralTrainer();

            learner.LoadTrainingData();
            learner.TrainOnAllData();
        }

    }
}
