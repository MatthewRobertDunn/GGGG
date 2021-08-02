using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GGGG.Algorithms;
using GGGG.Interface;
using GGGG.Twister;

namespace GGGG
{
    public class FastMonteCarloFunctions
    {
        public static FastMoveGenerator generator = new FastMoveGenerator();

        public static CapturesCache captureCache = new CapturesCache();

        public static Int32 capturesInfoUsedCount;
        public static Int32 captureLies;


        public static unsafe PinDropResult GetRandomBoardAtEnd(PinDropParams pindropInfo)
        {
            //Console.WriteLine(board.ToString());
            //Console.ReadLine();
            var moves = generator.GetMoveList(pindropInfo.Board, pindropInfo.Player).ToList();
            //moves.Shuffle();
            IFastBoard child = null;
            //var captureInfo = captureCache.GetCaptures(board.BoardHash.HashValue, player);

            foreach (var move in DoMoveOrdering(moves, pindropInfo.Board, pindropInfo.Player))
            //foreach (var move in moves)
            {
                var test = pindropInfo.Board.PlayMove(move.Pos, move.Color);
                if (pindropInfo.Board.MoveIsValid(test, move.Color))
                {
                    child = test;
                    break;
                }
                test.Dispose();
            }

            if (child == null && pindropInfo.LastNull)
            {
                return new PinDropResult() { Board = pindropInfo.Board };
            }

            if (!pindropInfo.CheckPass)
                pindropInfo.Player = pindropInfo.Player.GetEnemy();

            if (child == null)
            {
                pindropInfo.LastNull = true;
                pindropInfo.IsFirst = false;
                return GetRandomBoardAtEnd(pindropInfo);
            }
            else
            {
                if (pindropInfo.Board != pindropInfo.ParentBoard)
                    pindropInfo.Board.Dispose();

                pindropInfo.LastNull = false;
                pindropInfo.IsFirst = false;
                pindropInfo.Board = child;

                return GetRandomBoardAtEnd(pindropInfo);
            }
        }

        public static unsafe IEnumerable<GoMove> DoMoveOrdering(List<GoMove> moves, IFastBoard parent, BoardSquares player)
        {
            return moves.OrderByDescending(x => GetMovePriority(x, parent, player));
        }

        public static bool SkipEdgesHeuristic(Tuple<int, int> lastMove, Board board)
        {
            if (lastMove.Item1 == 1 || lastMove.Item2 == 1 || lastMove.Item1 == board.Width - 2 || lastMove.Item2 == board.Height - 2)
                return true;

            return false;
        }

        public static unsafe double GetMovePriority(GoMove move, IFastBoard parent, BoardSquares player)
        {
            fixed (BoardSquares* cellPointer = parent.BoardVertices)
            {

                var cell = cellPointer + move.Pos;

                var advancedBoard = parent as IAdvancedBoard;
                if (advancedBoard != null)
                {
                    if (advancedBoard.IsCapture(move.Pos))
                    {
                        return MyRandom.Random.NextDouble() * 2;
                    }

                }

                ////Decreased probability of play into squares with only 1 liberty
                //int libertyCount = parent.CountTrueForSurroundings(x => x == BoardSquares.Empty || x == player, cell);
                //if (libertyCount == 1)
                //{
                //    return MyRandom.Random.NextDouble() * 0.05;
                //}

                var movexy = parent.PosToXY(move.Pos);

                ////Tinsy bias to playing moves close by the last move    
                if (parent.LastMove.Pos != 0)
                {

                    var distance = parent.LastMoveXY().DistanceFrom(movexy);
                    if (distance <= 2)
                    {
                        return MyRandom.Random.NextDouble() * 1.1;
                    }
                }

                //rarely play on edge
                if (movexy.Item1 == 1 || movexy.Item1 == parent.BoardSize - 2)
                    return MyRandom.Random.NextDouble() * 0.9;

                if (movexy.Item2 == 1 || movexy.Item2 == parent.BoardSize - 2)
                    return MyRandom.Random.NextDouble() * 0.9;

                //Bias play near existing stones
                if (parent.TrueForAnySurrounding(x => x == BoardSquares.White || x == BoardSquares.Black, cell))
                {
                    return MyRandom.Random.NextDouble() * 1.1;
                }
            }
            return MyRandom.Random.NextDouble();
        }

    }
}



//Decrease probability of play into its own eye hole
//if (parent.TrueForAllSurrounding(x => x == player || x == BoardSquares.Edge, cell))
//{
//    sortedList.Add(Tuple.Create(0.3, move));
//    continue;
//}

////Decreased probability of play into squares with only 1 liberty
//int libertyCount = parent.CountTrueForSurroundings(x => x == BoardSquares.Empty || x == player, cell);
//if (libertyCount == 1)
//{
//    //Console.WriteLine("One liberty");
//    sortedList.Add(Tuple.Create(MyRandom.Random.NextDouble() * 0.2, move));
//    continue;
//}

//pattern match
//var test = MyRandom.Random.Next();

//var test2 = parent.PosToXY(move.Pos);
//if (test2.Item1 != 1 && test2.Item1 != parent.Width - 2)
//    if (test2.Item2 != 1 && test2.Item2 != parent.Width - 2)
//        //if (parent.TrueForAnySurrounding(x => x == BoardSquares.White || x == BoardSquares.Black, cell))
//        {
//            if (NewPatternLibrary.IsPatternMatch(move.Pos, parent, player))
//            {
//                sortedList.Add(Tuple.Create(MyRandom.Random.NextDouble() * 1000.0, move));
//                //Console.WriteLine("Pattern match");
//                continue;
//            }
//        }

//Boosted probability of play into squares next to last move
