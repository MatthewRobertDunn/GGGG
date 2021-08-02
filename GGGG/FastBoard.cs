using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GGGG.Algorithms;
using GGGG.Interface;
using GGGG.Numerics.GGGG.Numerics;

namespace GGGG
{
    public unsafe class FastBoard : IFastBoard
    {
        private int _width;
        private int _height;
        private int capturedWhiteStones;
        private int capturedBlackStones;
        private List<GoMove> moveHistory = new List<GoMove>();
        private ZobristHash _boardHash = new ZobristHash();

        public int CapturedBlackStones
        {
            get { return capturedBlackStones; }
        }
        
        private GoMove lastMove;


        private BoardSquares[] _boardVertices;
        private List<Int64> boardHistory = new List<Int64>();

        public IList<Int64> BoardHistory
        {
            get { return boardHistory; }
        }

        
        public IList<GoMove> MoveHistory
        {
            get { return moveHistory; }
        }
        

        public int CapturedWhiteStones
        {
            get { return capturedWhiteStones; }
        }

        public GoMove LastMove
        {
            get { return lastMove; }
        }


        public ZobristHash BoardHash
        {
            get { return _boardHash; }
        }

        private FastBoard()
        {

        }

        public FastBoard(int width, int height)
        {

            width = width + 2;
            height = height + 2;

            _width = width;
            _height = height;
            _boardVertices = new BoardSquares[(width) * (height)];
            SetupBoard();
            boardHistory.Add(_boardHash.HashValue);
        }


        public int Height
        {
            get { return _height; }
        }

        public int Width
        {
            get { return _width; }
        }

        public int BoardSize
        {
            get { return _height; }
        }

        public BoardSquares[] BoardVertices
        {
            get { return _boardVertices; }
        }


        public void SetupBoard()
        {
            for (int x = 0; x < _width; x++)
                for (int y = 0; y < _height; y++)
                {
                    //Edges
                    if (x == 0 || x == _width - 1 || y == 0 || y == _height - 1)
                    {
                        BoardVertices[XYToPos(x, y)] = BoardSquares.Edge;
                    }
                    else
                    {
                        BoardVertices[XYToPos(x, y)] = BoardSquares.Empty;
                    }
                }
        }


        public IFastBoard PlayMove(int x, int y, BoardSquares move)
        {
            return PlayMove(XYToPos(x, y), move);
        }

        public IFastBoard PlayMove(int pos, BoardSquares player)
        {
            FastBoard newBoard = (FastBoard)this.Clone();
            newBoard.PlayMoveInternal(pos, player);
            return newBoard;
        }

        public void PlayMoveInternal(int pos, BoardSquares player)
        {
            if (pos == 0)
            {
                _boardHash.Delta(BoardSquares.White, pos);
                boardHistory.Add(_boardHash.HashValue);
                lastMove = new GoMove() { Color = player, Pos = pos };
                return;
            }
            BoardVertices[pos] = player;

            _boardHash.Delta(player, pos);

            int captures = MarkAndSweep(player);

            lastMove = new GoMove() { Color = player, Pos = pos };
            lastMove.CaptureOccured = captures != 0;
            moveHistory.Add(lastMove);
            boardHistory.Add(_boardHash.HashValue);

        }

        public unsafe bool DoOnlyEyesRemain(BoardSquares* cellPointer)
        {
            for (int x = 1; x < Width - 1; x++)
                for (int y = 1; y < Height - 1; y++)
                {
                    int i = XYToPos(x, y);
                    var currentcellPointer = cellPointer + i;
                    var cell = *currentcellPointer;

                    if (cell == BoardSquares.Empty)
                    {
                        if (!this.TrueForAllSurrounding(k => k == BoardSquares.Edge || k == BoardSquares.White || k == BoardSquares.Black, currentcellPointer))
                            return false;
                    }
                }
            return true;
        }


    

        private unsafe int MarkAndSweep(BoardSquares player)
        {
            int captures = 0;
            //Try and capture any enemy stones first
            fixed (BoardSquares* cellPointer = _boardVertices)
            {

                MarkStones(player.GetEnemy(), cellPointer);
                captures = SweepDeadStones(player.GetEnemy(), cellPointer);

                //If an enemy was captured, can't possibly be suicide

                if (captures == 0)
                {
                    //Check for suicide
                    MarkStones(player, cellPointer);
                    SweepDeadStones(player, cellPointer);
                }

                //Depressure all stones
                DepressureAllStones(cellPointer);
            }

            return captures;
        }

        private unsafe void DepressureAllStones(BoardSquares* cellPointer)
        {

            for (int x = 1; x < Width - 1; x++)
                for (int y = 1; y < Height - 1; y++)
                {
                    int i = XYToPos(x, y);
                    var currentcellPointer = cellPointer + i;
                    var cell = *currentcellPointer;

                    if (cell == BoardSquares.PressuredWhite || cell == BoardSquares.PressuredBlack)
                    {
                        *currentcellPointer = cell.GetUnpressuredVersion();
                    }
                }
        }

        private unsafe int SweepDeadStones(BoardSquares player, BoardSquares* cellPointer)
        {
            int numCaptured = 0;

            for (int x = 1; x < Width - 1; x++)
                for (int y = 1; y < Height - 1; y++)
                {
                    int i = XYToPos(x, y);
                    var currentcellPointer = cellPointer + i;
                    //var cell = *currentcellPointer;
                    if (*currentcellPointer == player)
                    {
                        //remove this stone
                        *currentcellPointer = BoardSquares.Empty;
                        numCaptured++;
                        _boardHash.Delta(*currentcellPointer, i);
                    }
                }

            if (player == BoardSquares.White)
                capturedWhiteStones += numCaptured;
            else
                capturedBlackStones += numCaptured;

            return numCaptured;
        }
        
        /// <summary>
        /// Performs a single mark and prune iteration for a player
        /// </summary>
        /// <param name="player">Player to mark and sweep for</param>
        /// <returns>true if this was the last iteration for this player</returns>
        private unsafe void MarkStones(BoardSquares player, BoardSquares* cellPointer)
        {

            bool isFinished;
            do
            {
                isFinished = true;
                for (int i = 0; i < _boardVertices.Length; i++)
                {
                    var currentcellPointer = cellPointer + i;
                    var cell = *currentcellPointer;

                    //if the cell doesn't match the current player, it's not under pressure
                    if (cell != player)
                        continue;

                    //Right
                    BoardSquares testCell = *(currentcellPointer + 1);
                    if (testCell == BoardSquares.Empty || testCell == player.GetPressuredVersion())
                    {
                        *currentcellPointer = player.GetPressuredVersion();
                        isFinished = false;
                        continue;
                    }

                    //Left
                    testCell = *(currentcellPointer - 1);
                    if (testCell == BoardSquares.Empty || testCell == player.GetPressuredVersion())
                    {
                        *currentcellPointer = player.GetPressuredVersion();
                        isFinished = false;
                        continue;
                    }

                    //Up
                    testCell = *(currentcellPointer - _height);
                    if (testCell == BoardSquares.Empty || testCell == player.GetPressuredVersion())
                    {
                        *currentcellPointer = player.GetPressuredVersion();
                        isFinished = false;
                        continue;
                    }

                    //Down
                    testCell = *(currentcellPointer + _height);
                    if (testCell == BoardSquares.Empty || testCell == player.GetPressuredVersion())
                    {
                        *currentcellPointer = player.GetPressuredVersion();
                        isFinished = false;
                        continue;
                    }
                }

                if (isFinished == false)
                {
                    isFinished = true;
                    for (int i = _boardVertices.Length; i >= 0; i--)
                    {
                        var currentcellPointer = cellPointer + i;
                        var cell = *currentcellPointer;

                        //if the cell doesn't match the current player, it's not under pressure
                        if (cell != player)
                            continue;

                        //Right
                        BoardSquares testCell = *(currentcellPointer + 1);
                        if (testCell == BoardSquares.Empty || testCell == player.GetPressuredVersion())
                        {
                            *currentcellPointer = player.GetPressuredVersion();
                            isFinished = false;
                            continue;
                        }

                        //Left
                        testCell = *(currentcellPointer - 1);
                        if (testCell == BoardSquares.Empty || testCell == player.GetPressuredVersion())
                        {
                            *currentcellPointer = player.GetPressuredVersion();
                            isFinished = false;
                            continue;
                        }

                        //Up
                        testCell = *(currentcellPointer - _height);
                        if (testCell == BoardSquares.Empty || testCell == player.GetPressuredVersion())
                        {
                            *currentcellPointer = player.GetPressuredVersion();
                            isFinished = false;
                            continue;
                        }

                        //Down
                        testCell = *(currentcellPointer + _height);
                        if (testCell == BoardSquares.Empty || testCell == player.GetPressuredVersion())
                        {
                            *currentcellPointer = player.GetPressuredVersion();
                            isFinished = false;
                            continue;
                        }
                    }
                }

            } while (isFinished == false);
        }


        public IFastBoard Clone()
        {
            FastBoard newBoard = new FastBoard();
            newBoard.capturedBlackStones = this.capturedBlackStones;
            newBoard.capturedWhiteStones = this.capturedWhiteStones;
            newBoard._height = this._height;
            newBoard._width = this._width;
            newBoard._boardVertices = (BoardSquares[])BoardVertices.Clone();
            newBoard._boardHash = this._boardHash.Clone();
            newBoard.boardHistory = new List<long>(this.boardHistory);
            newBoard.moveHistory = new List<GoMove>(this.moveHistory);
            return newBoard;
        }


        public BoardSquares[] GetSquareContentsAround(int pos)
        {
            var result = new BoardSquares[4];
            result[0] = BoardVertices[Above(pos)];
            result[1] = BoardVertices[Below(pos)];
            result[2] = BoardVertices[Left(pos)];
            result[3] = BoardVertices[Right(pos)];
            return result;
        }


        public int Above(int pos)
        {
            return pos - _height;
        }

        public int Below(int pos)
        {
            return pos + _height;
        }

        public int Left(int pos)
        {
            return pos - 1;
        }

        public int Right(int pos)
        {
            return pos + 1;
        }

        public Tuple<int, int> PosToXY(int pos)
        {
            int x = pos % _height;
            int y = (int)Math.Floor((double)pos / _height);
            return Tuple.Create(x, y);
        }

        public int XYToPos(int x, int y)
        {
            return y * _height + x;
        }




        public unsafe Tuple<bool, Int64> HashSurrounding(int pos, int patternSize)
        {
            var xy = PosToXY(pos);
            var center = patternSize / 2;

            bool patternContainsStones = false;
            ZobristHash hash = new ZobristHash();

            for (int y = -center; y < patternSize - center; y++)
                for (int x = -center; x < patternSize - center; x++)
                {
                    if (x == 0 && y == 0)
                        continue;

                    var offset = new Tuple<int, int>(x, y);
                    var boardPosition = xy.VectorAdd(offset);

                    if (boardPosition.Item1 < 0 || boardPosition.Item1 >= _width)
                        continue;

                    if (boardPosition.Item2 < 0 || boardPosition.Item2 >= _height)
                        continue;

                    var stone = GetItemAt(boardPosition);

                    if (stone == BoardSquares.Empty)
                        continue;

                    var patternPosition = new Tuple<int, int>(x + center, y + center);

                    if (stone == BoardSquares.White || stone == BoardSquares.Black)
                        patternContainsStones = true;

                    hash.Delta(stone, patternPosition.XYToPos(patternSize));
                }

            return new Tuple<bool, long>(patternContainsStones, hash.HashValue);
        }

        public unsafe Int64 HashSurrounding(BoardSquares* cell)
        {
            BoardSquares[] surrounds = new BoardSquares[8];

            //NOTE, odering is imporant here!

            //top left
            surrounds[0] = *(cell - _height - 1);

            //Up
            surrounds[1] = *(cell - _height);

            //top right
            surrounds[2] = *(cell - _height + 1);

            //Left
            surrounds[3] = *(cell - 1);

            //Right
            surrounds[4] = *(cell + 1);

            //bottom left
            surrounds[5] = *(cell + _height - 1);

            //Down
            surrounds[6] = *(cell + _height);

            //bottom right
            surrounds[7] = *(cell + _height + 1);


            ZobristHash hash = new ZobristHash();
            for (int index = 0; index < surrounds.Length; index++)
            {
                var boardSquare = surrounds[index];

                if (boardSquare == BoardSquares.Empty)
                    continue;

                hash.Delta(boardSquare, index);
            }

            return hash.HashValue;
        }

        public unsafe bool TrueForAllSurrounding(Func<BoardSquares, bool> func, BoardSquares* currentcellPointer)
        {
            //Right
            BoardSquares testCell = *(currentcellPointer + 1);
            if (!func(testCell))
                return false;

            //Left
            testCell = *(currentcellPointer - 1);
            if (!func(testCell))
                return false;

            //Up
            testCell = *(currentcellPointer - _height);
            if (!func(testCell))
                return false;

            //Down
            testCell = *(currentcellPointer + _height);
            if (!func(testCell))
                return false;

            return true;
        }

        public BoardSquares GetItemAt(int x, int y)
        {
            return (BoardSquares)_boardVertices[XYToPos(x, y)];
        }

        public BoardSquares GetItemAt(Tuple<int, int> pos)
        {
            return (BoardSquares)_boardVertices[XYToPos(pos.Item1, pos.Item2)];
        }
        
        public override string ToString()
        {
            StringBuilder s = new StringBuilder();
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
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

        public void Dispose()
        {
            //throw new NotImplementedException();
        }
    }
}
