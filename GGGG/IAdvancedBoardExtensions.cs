using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using GGGG.Interface;
using GGGG.NewBoardAgain;

namespace GGGG
{
    public static class IAdvancedBoardExtensions
    {

        public static bool IsCapture(this IAdvancedBoard board, int pos)
        {
            return board.TrueForAnySurrounding(x => x.liberties.Count == 1, pos);
        }

        public static IEnumerable<int> GetStones(this IAdvancedBoard board, BoardSquares player)
        {
            foreach (var s in board.Strings)
            {
                if (s.Player == player)
                {
                    foreach (var stone in s.stones)
                    {
                        yield return stone;
                    }
                }
            }
        }

        public static bool TrueForAnySurrounding(this IAdvancedBoard board, Func<GoString2, bool> func, int pos)
        {
            GoString2[] stringIndex = board.StringsIndex;

            //Right
            GoString2 testCell = stringIndex[pos + 1];
            if (testCell != null)
                if (func(testCell))
                    return true;

            //Left
            testCell = stringIndex[pos - 1];
            if (testCell != null)
                if (func(testCell))
                    return true;

            //Up
            testCell = stringIndex[pos - board.BoardSize];
            if (testCell != null)
                if (func(testCell))
                    return true;

            //Down
            testCell = stringIndex[pos + board.BoardSize];
            if (testCell != null)
                if (func(testCell))
                    return true;

            return false;
        }
    }
}