using System;
using System.Collections.Generic;

namespace GGGG.Interface
{
    public interface IFastBoard : IDisposable
    {
        /// <summary>
        /// Exposes all the vertices of the board
        /// </summary>
        BoardSquares[] BoardVertices { get; }

        /// <summary>
        /// Number of captured whitestones thus far
        /// </summary>
        int CapturedWhiteStones { get; }


        /// <summary>
        /// Number of captured black stones thus far
        /// </summary>
        int CapturedBlackStones { get; }

        /// <summary>
        /// Contains hashes of all previous boards.
        /// </summary>
        IList<Int64> BoardHistory { get; }

        /// <summary>
        /// Returns all previous moves
        /// </summary>
        IList<GoMove> MoveHistory { get; }

        /// <summary>
        /// Plays a given move on the board
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="player"></param>
        /// <returns></returns>
        IFastBoard PlayMove(int pos, BoardSquares player);

        /// <summary>
        /// Returns the last move that was played on this board
        /// </summary>
        GoMove LastMove { get; }

        /// <summary>
        /// Returns the current hash of the board
        /// </summary>
        ZobristHash BoardHash { get; }


        /// <summary>
        /// Converts an array index to an x,y tuple
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        Tuple<int, int> PosToXY(int pos);

        /// <summary>
        /// Returns current board size
        /// Always returns 2 bigger than the board really is to make room for edges
        /// </summary>
        int BoardSize { get; }

        IFastBoard Clone();
    }
}
