using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GGGG.Interface;
using GGGG.Twister;

namespace GGGG.MarkAndSweep
{
    public static class MarkAndSweepSimulation
    {
        public static FastMoveGenerator generator = new FastMoveGenerator();

        public static PinDropResult PlayMarkAndSweepSimulation(PinDropParams pindropInfo)
        {
            var moves = generator.GetMoveList(pindropInfo.Board, pindropInfo.Player).ToList();
            IFastBoard child = null;

            //System.Diagnostics.Debug.WriteLine(pindropInfo.Board.ToString());
            if (pindropInfo.CurrentDepth < pindropInfo.MaxDepth)
                while (moves.Count > 0)
                {
                    int moveNum = MyRandom.Random.Next(0, moves.Count);
                    var move = moves[moveNum];
                    var isValid = pindropInfo.Board.PlayMarkSweepMove(move);
                    if (isValid.MoveWasValid)
                    {
                        child = pindropInfo.Board;

                        if (!pindropInfo.CaptureOccured)
                        {
                            //update hash with move
                            pindropInfo.Board.BoardHash.Delta(move.Color,move.Pos);
                            //update board history
                            pindropInfo.Board.BoardHistory.Add(pindropInfo.Board.BoardHash.HashValue);
                            //update move history
                            pindropInfo.Board.MoveHistory.Add(move);
                        }

                        if (isValid.CapturedStones.Count > 0)
                        {
                            pindropInfo.CaptureOccured = true;
                        }



                        break;
                    }
                    else
                    {
                        moves.RemoveAt(moveNum);
                    }
                }

            if (child == null && pindropInfo.LastNull)
            {
                return new PinDropResult() { Board = pindropInfo.Board };
            }


            //wtf is this?
            if (!pindropInfo.CheckPass)
                pindropInfo.Player = pindropInfo.Player.GetEnemy();


            //No more moves left
            if (child == null)
            {
                pindropInfo.LastNull = true;
                pindropInfo.IsFirst = false;
                pindropInfo.CurrentDepth += 1;

                //update board with a null
                if (!pindropInfo.CaptureOccured)
                {
                    pindropInfo.Board.BoardHash.Delta(pindropInfo.Player, 0);
                    pindropInfo.Board.BoardHistory.Add(pindropInfo.Board.BoardHash.HashValue);
                    var lastMove = new GoMove() { Color = pindropInfo.Player, Pos = 0 };
                    pindropInfo.Board.MoveHistory.Add(lastMove);
                }
                return PlayMarkAndSweepSimulation(pindropInfo);
            }

            


            pindropInfo.LastNull = false;
            pindropInfo.IsFirst = false;
            pindropInfo.Board = child;
            pindropInfo.CurrentDepth += 1;
            return PlayMarkAndSweepSimulation(pindropInfo);
        }
    }
}
