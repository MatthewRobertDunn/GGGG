using GGGG.Interface;
using GGGG.Twister;

namespace GGGG
{
    public class PinDropParams
    {
        public PinDropParams(IFastBoard board, BoardSquares player)
        {
            this.Board = board;
            this.Player = player;
            this._parentBoard = board;
        }

        private IFastBoard _parentBoard;
        public IFastBoard Board;
        public BoardSquares Player;
        public bool LastNull = false;
        public bool IsFirst = true;
        public bool CheckPass = false;
        public bool CaptureOccured = false;
        public int CurrentDepth;
        public int MaxDepth;

        public IFastBoard ParentBoard
        {
            get { return _parentBoard; }
        }
    }
}