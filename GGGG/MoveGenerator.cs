using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GGGG.Algorithms;
using GGGG.Interface;
using GGGG.Twister;

namespace GGGG
{
    public class MoveGenerator
    {
        public IEnumerable<Board> GetValidMoves(Board board, BoardSquares player)
        {
            for (int i = 0; i < board.BoardVertices.Length; i++)
            {
                var square = (BoardSquares)board.BoardVertices[i];
                if (square == BoardSquares.Empty)
                {
                    var result = board.PlayMove(i, player);
                    if (Board.MoveIsValid(board, result, player))
                        yield return result;
                }

            }
        }


        public IEnumerable<GoMove> GetMoveList(Board board, BoardSquares player)
        {
            for (int i = 0; i < board.BoardVertices.Length; i++)
            {
                var square = (BoardSquares)board.BoardVertices[i];
                if (square == BoardSquares.Empty)
                {
                    //if (!IsKo(board, i, player))
                    yield return new GoMove() { Color = player, Pos = i };

                }
            }
        }
    }


    public class FastMoveGenerator
    {
        public IEnumerable<IFastBoard> GetValidMoves(IFastBoard board, BoardSquares player)
        {
            var passMove = new GoMove() { Color = player, Pos = 0 };

            var moves = GetMoveList(board, player).ToList();

            int movesReturned = 0;

            foreach (var goMove in moves)
            {
                var result = board.PlayMove(goMove.Pos, goMove.Color);
                if (board.MoveIsValid(result, player))
                {
                    movesReturned++;
                    yield return result;
                }
            }

            if (board.LastMove.Pos != 0 || movesReturned == 0)
            {
                yield return board.PlayMove(passMove.Pos, passMove.Color);
            }
        }

        public IEnumerable<GoMove> GetMoveList(IFastBoard board, BoardSquares player)
        {
            for (int i = 0; i < board.BoardVertices.Length; i++)
            {
                var square = (BoardSquares)board.BoardVertices[i];
                if (square == BoardSquares.Empty)
                {

                    var move = new GoMove() { Color = player, Pos = i };
                    yield return move;
                }
            }
        }
    }
}
