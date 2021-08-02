using System;
using System.Collections.Generic;
using GGGG.Interface;

namespace GGGG
{
    public static class IFastBoardExtensions
    {

        public static IEnumerable<int> GetLiberties(this IFastBoard board)
        {
            for (int i = 0; i < board.BoardVertices.Length; i++)
            {
                if (board.BoardVertices[i] == BoardSquares.Empty)
                    yield return i;
            }
        }


        public static unsafe bool TrueForAllSurrounding(this IFastBoard board, Func<BoardSquares, bool> func, int pos)
        {
            fixed (BoardSquares* cellPointer = board.BoardVertices)
            {
                var cell = cellPointer + pos;
                return TrueForAllSurrounding(board, func, cell);
            }
        }

        public static unsafe bool TrueForAllSurrounding(this IFastBoard board, Func<BoardSquares, bool> func, BoardSquares* currentcellPointer)
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
            testCell = *(currentcellPointer - board.BoardSize);
            if (!func(testCell))
                return false;

            //Down
            testCell = *(currentcellPointer + board.BoardSize);
            if (!func(testCell))
                return false;

            return true;
        }

        public static IFastBoard PlayMove(this IFastBoard board, GTextMove move)
        {
            BoardSquares player;
            if (move.Color == GoColor.White)
                player = BoardSquares.White;
            else
                player = BoardSquares.Black;

            return board.PlayMove((int)move.X + 1, (int)move.Y + 1, player);
        }



        public static IFastBoard PlayMove(this IFastBoard board, int x, int y, BoardSquares move)
        {
            return board.PlayMove(board.XYToPos(x, y), move);
        }

        public static Tuple<int, int> LastMoveXY(this IFastBoard board)
        {
            return board.PosToXY(board.LastMove.Pos);
        }


        public static unsafe double CalcScore(this PinDropResult v, double komi = 0)
        {
            if (v.Adjucated)
                return (v.WhitePoints + komi) - v.BlackPoints;
            return CalcScore(v.Board, komi);
        }


        public static unsafe double CalcScore(this IFastBoard board, double komi = 0)
        {
            double whiteStoneCount = 0.0;
            double blackStoneCount = 0.0;

            fixed (BoardSquares* cellPointer = board.BoardVertices)
            {
                for (int x = 1; x < board.BoardSize - 1; x++)
                    for (int y = 1; y < board.BoardSize - 1; y++)
                    {
                        int i = board.XYToPos(x, y);
                        var currentcellPointer = cellPointer + i;
                        var cell = *currentcellPointer;

                        if (cell == BoardSquares.White)
                            whiteStoneCount++;
                        if (cell == BoardSquares.Black)
                            blackStoneCount++;

                        if (cell == BoardSquares.Empty)
                        {
                            if (board.TrueForAllSurrounding(c => c == BoardSquares.White || c == BoardSquares.Edge, currentcellPointer))
                                whiteStoneCount++;

                            if (board.TrueForAllSurrounding(c => c == BoardSquares.Black || c == BoardSquares.Edge, currentcellPointer))
                                blackStoneCount++;
                        }
                    }
            }
            return (whiteStoneCount + komi) - blackStoneCount;
        }

        public static BoardSquares GetItemAt(this IFastBoard board, int x, int y)
        {
            return (BoardSquares)board.BoardVertices[board.XYToPos(x, y)];
        }


        public static int XYToPos(this IFastBoard board, int x, int y)
        {
            return y * board.BoardSize + x;
        }

        public static unsafe bool TrueForAnySurrounding(this IFastBoard board, Func<BoardSquares, bool> func, int pos)
        {
            fixed (BoardSquares* cellPointer = board.BoardVertices)
            {
                var cell = cellPointer + pos;
                return TrueForAnySurrounding(board, func, cell);
            }
        }

        public static unsafe bool TrueForAnySurrounding(this IFastBoard board, Func<BoardSquares, bool> func, BoardSquares* currentcellPointer)
        {
            //Right
            BoardSquares testCell = *(currentcellPointer + 1);
            if (func(testCell))
                return true;

            //Left
            testCell = *(currentcellPointer - 1);
            if (func(testCell))
                return true;

            //Up
            testCell = *(currentcellPointer - board.BoardSize);
            if (func(testCell))
                return true;

            //Down
            testCell = *(currentcellPointer + board.BoardSize);
            if (func(testCell))
                return true;

            return false;
        }

        public static unsafe bool DoOnlyEyesRemain(this IFastBoard board, BoardSquares* cellPointer)
        {
            for (int x = 1; x < board.BoardSize - 1; x++)
                for (int y = 1; y < board.BoardSize - 1; y++)
                {
                    int i = board.XYToPos(x, y);
                    var currentcellPointer = cellPointer + i;
                    var cell = *currentcellPointer;

                    if (cell == BoardSquares.Empty)
                    {
                        if (!board.TrueForAllSurrounding(k => k == BoardSquares.Edge || k == BoardSquares.White || k == BoardSquares.Black, currentcellPointer))
                            return false;
                    }
                }
            return true;
        }

        public static unsafe bool MoveIsValid(this IFastBoard board, IFastBoard newBoard, BoardSquares player)
        {
            //Pass
            if (newBoard.LastMove.Pos == 0)
                return true;

            //Black suicide
            if (player == BoardSquares.Black)
            {
                if (newBoard.CapturedBlackStones > board.CapturedBlackStones)
                    return false;
            }

            //white suicide
            if (player == BoardSquares.White)
            {
                if (newBoard.CapturedWhiteStones > board.CapturedWhiteStones)
                    return false;
            }

            //Positional superko
            if (board.BoardHistory.Contains(newBoard.BoardHash.HashValue))
                return false;

            //only enable don't fill in eye check if only eyes remain
            fixed (BoardSquares* cellPointer = board.BoardVertices)
                if (board.TrueForAllSurrounding(x => x == player || x == BoardSquares.Edge, cellPointer + newBoard.LastMove.Pos))
                    //if (this.DoOnlyEyesRemain(cellPointer))
                {
                    //Don't do it unless it's a capture
                    var oldWhite = board.CapturedWhiteStones;
                    var oldBlack = board.CapturedBlackStones;

                    var testBoard = board.PlayMove(newBoard.LastMove.Pos, player.GetEnemy());

                    if (player == BoardSquares.White)
                        if (oldWhite != testBoard.CapturedWhiteStones)
                        {
                            testBoard.Dispose();
                            return true;
                        }
                    if (player == BoardSquares.Black)
                        if (oldBlack != testBoard.CapturedBlackStones)
                        {
                            testBoard.Dispose();
                            return true;
                        }

                    testBoard.Dispose();
                    return false;

                }

            return true;
        }

        public static unsafe int CountTrueForSurroundings(this IFastBoard board, Func<BoardSquares, bool> func, BoardSquares* currentcellPointer)
        {
            int count = 0;
            //Right
            BoardSquares testCell = *(currentcellPointer + 1);
            if (func(testCell))
                count++;

            //Left
            testCell = *(currentcellPointer - 1);
            if (func(testCell))
                count++;

            //Up
            testCell = *(currentcellPointer - board.BoardSize);
            if (func(testCell))
                count++;

            //Down
            testCell = *(currentcellPointer + board.BoardSize);
            if (func(testCell))
                count++;

            return count;
        }
    }
}