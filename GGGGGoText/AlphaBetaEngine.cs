using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GGGG;
using GGGG.Interface;

namespace GGGGGoText
{
    public class AlphaBetaEngine : IGoEngine
    {
        private Board board = null;
        private AlphaBetaSearch search = new AlphaBetaSearch();
        private Action<string> debugLog;
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger("GtpEngineAdapter");
        public void Start(uint size, float komi, uint level)
        {
            board = new Board((int)size, (int)size);
            Log.Info(board.ToString());
        }

        public void Setup(GoColor color, uint x, uint y)
        {
            throw new NotImplementedException();
        }

        public GTextMove Reply(GoColor color)
        {
            Tuple<Board,double> result;
            if( color == GoColor.Black)
                result = search.GetBestMove(4, board, double.MinValue, double.MaxValue, BoardSquares.Black);
            else
                result = search.GetBestMove(4, board, double.MinValue, double.MaxValue, BoardSquares.White);

            var whitePiecesBefore = board.CapturedWhiteStones;
            var blackPiecesBefore = board.CapturedBlackStones;
            var move = result.Item1;
            board = result.Item1;
            Log.Info(board.ToString());

            if (color == GoColor.Black)
            {
                if (board.CapturedBlackStones > blackPiecesBefore)
                    return new GoMovePass();
            }

            if (color == GoColor.White)
            {
                if (board.CapturedWhiteStones > whitePiecesBefore)
                    return new GoMovePass();
            }

            return new GTextMove(color, (uint)move.LastMove.Item1 - 1,(uint) move.LastMove.Item2 -1);
        }

        public void Play(GTextMove move)
        {
            BoardSquares player;

            if(move.Color == GoColor.White)
                player = BoardSquares.White;
            else
                player = BoardSquares.Black;

            board = board.PlayMove((int)move.X + 1, (int)move.Y + 1, player);
            Log.Info(board.ToString());
        }

        public void Undo()
        {
            throw new NotImplementedException();
        }

        public void Quit()
        {
            Environment.Exit(0);
        }

        public string Name
        {
            get { return "Matty Go"; }
        }

        public string Version
        {
            get { return "V0.1"; }
        }


        public Action<string> DebugMessageHandler
        {
            set { debugLog = value; }
        }

        public void SetTimeSettings(int seconds)
        {
            throw new NotImplementedException();
        }

        public void SetTimeLeft(GoColor color, int p)
        {
            throw new NotImplementedException();
        }
    }
}
