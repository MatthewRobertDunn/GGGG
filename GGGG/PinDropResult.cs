using GGGG.Interface;

namespace GGGG
{
    public struct PinDropResult
    {
        public IFastBoard Board;

        public bool Adjucated;
        public double WhitePoints;
        public double BlackPoints;
    }
}