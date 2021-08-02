using System;
using GGGG.Interface;
using ProtoBuf;

namespace GGGG.NeuralEstimator
{
    [ProtoContract]
    public class TrainingData
    {
        [ProtoMember(1)]
        public int BoardSize { get; set; }
        [ProtoMember(2)]
        public float Komi { get; set; }
        [ProtoMember(3)]
        public Int64 BoardHash { get; set; }
        [ProtoMember(4)]
        public int NumberStones { get; set; }
        [ProtoMember(5)]
        public BoardSquares Player { get; set; }
        [ProtoMember(6)]
        public int NumberGames { get; set; }
        [ProtoMember(7)]
        public double[] InputData { get; set; }
        [ProtoMember(8)]
        public double[] OuputData { get; set; }
    }
}