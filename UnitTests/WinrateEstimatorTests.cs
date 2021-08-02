using GGGG.NeuralEstimator;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class WinrateEstimatorTests
    {
        [TestMethod]
        public void LoadNeuralNetworks()
        {
            WinrateEstimator estimator = new WinrateEstimator();
            estimator.LoadNeuralNetworks();
        }


    }
}