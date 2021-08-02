using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GGGG.Algorithms;
using GGGG.Interface;

namespace GGGG.MarkAndSweep
{

    public struct PlayMarkSweepMoveResults
    {
        public BoardSquares CapturedStonesColor;
        public TerminatedList CapturedStones;
        public bool MoveWasValid;
    }

    public static class MarkAndSweepExtention
    {
        public static PlayMarkSweepMoveResults PlayMarkSweepMove(this IFastBoard board, GoMove p, bool doEyeCheck = true)
        {

            //Play the move, inspect outcome and see if we need to reverse anything

            board.BoardVertices[p.Pos] = p.Color;
            var captures = board.MarkAndSweep(p.Color);

            //If any suicide occured the move was invalid and we undo
            //Suicide is never OK
            if (captures.CapturedStonesColor == p.Color && captures.CapturedStones.Count > 0)
            {
                //Put back the captured stones first
                captures.CapturedStones.ForEach(x => board.BoardVertices[x] = captures.CapturedStonesColor);
                //Remove the stone we just put down also
                board.BoardVertices[p.Pos] = BoardSquares.Empty;
                return new PlayMarkSweepMoveResults() { MoveWasValid = false };
            }

            //Move could be invalid if we are filling an eye also
            //If this condition is true then captured stone count must also be zero
            if (doEyeCheck && board.TrueForAllSurrounding(x => x == p.Color || x == BoardSquares.Edge, p.Pos))
            {
                //first we undo the eye filling move we already made
                board.BoardVertices[p.Pos] = BoardSquares.Edge;

                //Second we play in that position as the enemy without doing the eyecheck we are doing now
                //eg play move assuming move is legal.
                var testParams = p;
                testParams.Color = testParams.Color.GetEnemy();
                var testPlay = board.PlayMarkSweepMove(testParams, false);

                //if the move was valid means there wasn't any suicide for the enemy to play into our eye
                if (testPlay.MoveWasValid)
                {
                    //We only have to remove captures if the move was valid; if it was suicide the move
                    //was illegal  any captured stones are already dealt with

                    //Put back the captured stones first if any
                    testPlay.CapturedStones.ForEach(x => board.BoardVertices[x] = captures.CapturedStonesColor);


                    //Put back the original eye filling move
                    board.BoardVertices[p.Pos] = p.Color;
                    //move is valid
                    return new PlayMarkSweepMoveResults() { MoveWasValid = true };
                }
                else
                {
                    //If move wasn't valid for the enemy there is no reason we should play here either
                    //Any stones have been put back on the board already 
                    board.BoardVertices[p.Pos] = BoardSquares.Empty;
                    return new PlayMarkSweepMoveResults() { MoveWasValid = false };
                }
            }

            //Move wasn't illegal eyefill or suicide, allow shit to happen

            return new PlayMarkSweepMoveResults()
                {
                    CapturedStonesColor = captures.CapturedStonesColor,
                    CapturedStones = captures.CapturedStones,
                    MoveWasValid = true
                };

        }

        public static unsafe PlayMarkSweepMoveResults MarkAndSweep(this IFastBoard board, BoardSquares player)
        {
            TerminatedList captures;
            //Try and capture any enemy stones first
            BoardSquares colorCapturedStones;
            fixed (BoardSquares* cellPointer = board.BoardVertices)
            {

                MarkStones(board, player.GetEnemy(), cellPointer);
                captures = SweepDeadStones(board, player.GetEnemy(), cellPointer);
                colorCapturedStones = player.GetEnemy();
                //If an enemy was captured, can't possibly be suicide
                if (captures.Count == 0)
                {
                    //Check for suicide
                    board.MarkStones(player, cellPointer);
                    captures = board.SweepDeadStones(player, cellPointer);
                    colorCapturedStones = player;
                }

                //Depressure all stones
                board.DepressureAllStones(cellPointer);
            }

            return new PlayMarkSweepMoveResults() { CapturedStones = captures, CapturedStonesColor = colorCapturedStones };
        }


        private static unsafe void DepressureAllStones(this IFastBoard board, BoardSquares* cellPointer)
        {
            int boardSize = board.BoardSize - 1;

            for (int x = 1; x < boardSize; x++)
                for (int y = 1; y < boardSize; y++)
                {
                    int i = board.XYToPos(x, y);
                    var currentcellPointer = cellPointer + i;
                    var cell = *currentcellPointer;

                    if (cell == BoardSquares.PressuredWhite || cell == BoardSquares.PressuredBlack)
                    {
                        *currentcellPointer = cell.GetUnpressuredVersion();
                    }
                }
        }

        /// <summary>
        /// Performs a single mark and prune iteration for a player
        /// </summary>
        /// <param name="board"> </param>
        /// <param name="player">Player to mark and sweep for</param>
        /// <returns>true if this was the last iteration for this player</returns>
        public static unsafe void MarkStones(this IFastBoard board, BoardSquares player, BoardSquares* cellPointer)
        {
            bool isFinished;
            int vertexCount = board.BoardVertices.Length;
            int boardSize = board.BoardSize;
            do
            {
                isFinished = true;
                for (int i = 0; i < vertexCount; i++)
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
                    testCell = *(currentcellPointer - boardSize);
                    if (testCell == BoardSquares.Empty || testCell == player.GetPressuredVersion())
                    {
                        *currentcellPointer = player.GetPressuredVersion();
                        isFinished = false;
                        continue;
                    }

                    //Down
                    testCell = *(currentcellPointer + boardSize);
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
                    for (int i = vertexCount; i >= 0; i--)
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
                        testCell = *(currentcellPointer - boardSize);
                        if (testCell == BoardSquares.Empty || testCell == player.GetPressuredVersion())
                        {
                            *currentcellPointer = player.GetPressuredVersion();
                            isFinished = false;
                            continue;
                        }

                        //Down
                        testCell = *(currentcellPointer + boardSize);
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

        public static unsafe TerminatedList SweepDeadStones(this IFastBoard board, BoardSquares player, BoardSquares* cellPointer)
        {
            int boardSize = board.BoardSize;
            TerminatedList capturedStones = new TerminatedList(19 * 19);

            for (int x = 1; x < boardSize - 1; x++)
                for (int y = 1; y < boardSize - 1; y++)
                {
                    int i = board.XYToPos(x, y);
                    var currentcellPointer = cellPointer + i;
                    //var cell = *currentcellPointer;
                    if (*currentcellPointer == player)
                    {
                        //remove this stone
                        *currentcellPointer = BoardSquares.Empty;
                        //_boardHash.Delta(*currentcellPointer, i);
                        capturedStones.AddItem(i);
                    }
                }

            //if (player == BoardSquares.White)
            //board.capturedWhiteStones += numCaptured;
            //else
            //capturedBlackStones += numCaptured;

            return capturedStones;
        }
    }
}
