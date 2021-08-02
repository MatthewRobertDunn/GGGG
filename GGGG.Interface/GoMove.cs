namespace GGGG.Interface
{
    public struct GoMove
    {
        public int Pos;
        public BoardSquares Color;
        
        //This will be true if this move caused a capture
        public bool CaptureOccured;

        public override string ToString()
        {
            int bytePos =  Pos;

            if (Color == BoardSquares.White)
                bytePos = bytePos | 128;

            return "" + (char) bytePos;
        }
    }
}
