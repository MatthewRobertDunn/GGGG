using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using GGGG.Algorithms;
using GGGG.Interface;

namespace GGGG.NewBoardAgain
{
    public class BoardBase
    {
        protected int _boardSize;
        protected int capturedWhiteStones;
        protected int capturedBlackStones;
        protected GoMove lastMove;
        protected BoardSquares[] _boardVertices;
        protected List<Int64> boardHistory;
        protected List<GoMove> moveHistory;
        protected ZobristHash boardHash;
        protected bool isDisposed = false;

        public BoardSquares[] BoardVertices
        {
            get
            {
                Debug.Assert(!isDisposed);
                return _boardVertices;
            }
        }


        public GoMove LastMove
        {
            get
            {
                Debug.Assert(!isDisposed);
                return lastMove;
            }
        }

        public int CapturedBlackStones
        {
            get
            {
                Debug.Assert(!isDisposed);
                return capturedBlackStones;
            }
        }

        public int CapturedWhiteStones
        {
            get { Debug.Assert(!isDisposed); return capturedWhiteStones; }
        }

        public IList<Int64> BoardHistory
        {
            get { Debug.Assert(!isDisposed); return boardHistory; }
        }

        public IList<GoMove> MoveHistory
        {
            get { Debug.Assert(!isDisposed); return moveHistory; }
        }

        public ZobristHash BoardHash
        {
            get { Debug.Assert(!isDisposed); return boardHash; }
        }

        public int BoardSize
        {
            get
            {
                Debug.Assert(!isDisposed);
                return _boardSize;
            }
        }

        protected BoardBase()
        {
        }

        public BoardBase(int boardSize)
        {
            _boardSize = boardSize + 2;
            _boardVertices = new BoardSquares[_boardSize * _boardSize];
            this.boardHistory = new List<Int64>();
            this.moveHistory = new List<GoMove>();
            this.boardHash = new ZobristHash();
            SetupBoard();
            BoardHistory.Add(BoardHash.HashValue);
            lastMove = new GoMove() {Color = BoardSquares.White, Pos = 0};
        }

        public override string ToString()
        {
            Debug.Assert(!isDisposed);
            StringBuilder s = new StringBuilder();
            for (int y = 0; y < this.BoardSize; y++)
            {
                for (int x = 0; x < this.BoardSize; x++)
                {
                    var stone = _boardVertices[XYToPos(x, y)];
                    switch (stone)
                    {
                        case BoardSquares.Edge:
                            s.Append("x");
                            break;
                        case BoardSquares.White:
                            s.Append("w");
                            break;
                        case BoardSquares.Black:
                            s.Append("b");
                            break;
                        case BoardSquares.Empty:
                            s.Append("+");
                            break;
                    }
                }
                s.Append(Environment.NewLine);
            }

            return s.ToString();
        }

        private void SetupBoard()
        {
            for (int x = 0; x < BoardSize; x++)
                for (int y = 0; y < BoardSize; y++)
                {
                    //Edges
                    if (x == 0 || x == BoardSize - 1 || y == 0 || y == BoardSize - 1)
                    {
                        _boardVertices[XYToPos(x, y)] = BoardSquares.Edge;
                    }
                    else
                    {
                        _boardVertices[XYToPos(x, y)] = BoardSquares.Empty;
                    }
                }
        }


        public IEnumerable<int> GetEmptySquaresArround(int pos)
        {
            Debug.Assert(!isDisposed);
            var up = _boardVertices[Above(pos)];

            if (up == BoardSquares.Empty)
                yield return Above(pos);

            var down = _boardVertices[Below(pos)];

            if (down == BoardSquares.Empty)
                yield return Below(pos);

            var left = _boardVertices[Left(pos)];

            if (left == BoardSquares.Empty)
                yield return Left(pos);

            var right = _boardVertices[Right(pos)];

            if (right == BoardSquares.Empty)
                yield return Right(pos);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Tuple<int, int> PosToXY(int pos)
        {
            int x = pos % BoardSize;
            int y = (int)Math.Floor((double)pos / BoardSize);
            return Tuple.Create(x, y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Above(int pos)
        {
            return pos - BoardSize;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Below(int pos)
        {
            return pos + BoardSize;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Left(int pos)
        {
            return pos - 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Right(int pos)
        {
            return pos + 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int XYToPos(int x, int y)
        {
            return y * BoardSize + x;
        }
    }
}
