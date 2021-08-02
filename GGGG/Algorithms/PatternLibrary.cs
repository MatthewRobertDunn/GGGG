using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using GGGG.Interface;

namespace GGGG.Algorithms
{
    public class PatternLibrary
    {

        private static readonly ConcurrentDictionary<Int64, PatternCell[,]> Patterns = new ConcurrentDictionary<long, PatternCell[,]>();

        static PatternLibrary()
        {
            LoadPatterns();
        }

        public static void LoadPatterns()
        {
            List<PatternCell[,]> templates = new List<PatternCell[,]>();

            //bwb
            //+++
            //aaa
            var template = new PatternCell[3, 3];
            template[0, 0] = PatternCell.Black;
            template[1, 0] = PatternCell.White;
            template[2, 0] = PatternCell.Black;
            template[0, 1] = PatternCell.Empty;
            template[1, 1] = PatternCell.Empty;
            template[2, 1] = PatternCell.Empty;
            template[0, 2] = PatternCell.Any;
            template[1, 2] = PatternCell.Any;
            template[2, 2] = PatternCell.Any;

            templates.Add(template);

            //bw+
            //+++
            //a+a
            template = new PatternCell[3, 3];
            template[0, 0] = PatternCell.Black;
            template[1, 0] = PatternCell.White;
            template[2, 0] = PatternCell.Empty;
            template[0, 1] = PatternCell.Empty;
            template[1, 1] = PatternCell.Empty;
            template[2, 1] = PatternCell.Empty;
            template[0, 2] = PatternCell.Any;
            template[1, 2] = PatternCell.Empty;
            template[2, 2] = PatternCell.Any;
            templates.Add(template);

            //bwa
            //b++
            //a+a
            template = new PatternCell[3, 3];
            template[0, 0] = PatternCell.Black;
            template[1, 0] = PatternCell.White;
            template[2, 0] = PatternCell.Any;
            template[0, 1] = PatternCell.Black;
            template[1, 1] = PatternCell.Empty;
            template[2, 1] = PatternCell.Empty;
            template[0, 2] = PatternCell.Any;
            template[1, 2] = PatternCell.Empty;
            template[2, 2] = PatternCell.Any;
            templates.Add(template);

            foreach (var p in templates)
            {
                foreach (var pattern in GetAllSymmetries(p))
                {
                    foreach (var hash in GetAllHashesForPattern(p))
                    {
                        Patterns.TryAdd(hash, pattern);
                    }
                }
            }
        }

        public static IEnumerable<Int64> GetAllHashesForPattern(PatternCell[,] pattern)
        {
            int n = pattern.GetLength(0);
            int index = 0;

            var cellsContainingAny = new List<int>();

            ZobristHash hash = new ZobristHash();

            for (int y = 0; y < n; y++)
                for (int x = 0; x < n; x++)
                {
                    if (x == 1 && y == 1)
                        continue;

                    var patternLocation = new Tuple<int, int>(x, y);

                    var test = pattern[x, y];

                    if (test == PatternCell.Any)
                    {
                        cellsContainingAny.Add(patternLocation.XYToPos(3));
                    }
                    else
                        if (test != PatternCell.Empty)
                            hash.Delta((BoardSquares)test, patternLocation.XYToPos(3));

                    index++;
                }

            //make set containing all possibilities



            IList<BoardSquares> possibleValuesForAny = new List<BoardSquares>
                                           {
                                               BoardSquares.White, BoardSquares.Black, BoardSquares.Edge,
                                               BoardSquares.Empty
                                           };

            foreach (var combination in possibleValuesForAny.Permutate(cellsContainingAny.Count))
            {
                var result = combination.ToList();
                var anyHash = hash.Clone();
                for (int i = 0; i < cellsContainingAny.Count; i++)
                {
                    int pos = cellsContainingAny[i];
                    var stone = result[i];
                    if (stone != BoardSquares.Empty)
                        anyHash.Delta(stone, pos);

                }

                yield return anyHash.HashValue;
            }
        }

        public static PatternCell[,] GetInverseColor(PatternCell[,] pattern)
        {
            int n = pattern.GetLength(0);
            PatternCell[,] ret = new PatternCell[n, n];

            for (int x = 0; x < n; x++)
            {
                for (int y = 0; y < n; y++)
                {
                    var test = pattern[x, y];

                    PatternCell result;
                    if (test == PatternCell.White)
                        result = PatternCell.Black;
                    else if (test == PatternCell.Black)
                        result = PatternCell.White;
                    else
                        result = test;

                    ret[x, y] = result;
                }
            }

            return ret;
        }

        public static IEnumerable<PatternCell[,]> GetAllSymmetries(PatternCell[,] pattern)
        {
            foreach (var p in GetAllRotations(pattern))
            {
                yield return p;
            }

            foreach (var p in GetAllRotations(GetInverseColor(pattern)))
            {
                yield return p;
            }
        }

        public static IEnumerable<PatternCell[,]> GetAllRotations(PatternCell[,] pattern)
        {
            yield return pattern;
            var template1 = RotatePattern(pattern);
            var template2 = RotatePattern(template1);
            var template3 = RotatePattern(template2);

            yield return template1;
            yield return template2;
            yield return template3;

        }

        public static unsafe bool MatchesAnyPattern(int pos, IFastBoard board)
        {
            fixed (BoardSquares* cellPointer = board.BoardVertices)
            {
                var hash = board.HashSurrounding(pos,3);
                PatternCell[,] foundPattern = null;
                if (Patterns.TryGetValue(hash.Item2, out foundPattern))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsMatch(PatternCell[,] pattern, Tuple<int, int> pos, FastBoard board)
        {
            if (pos.Item1 < 1 || pos.Item1 > 7)
                return false;

            if (pos.Item2 < 1 || pos.Item2 > 7)
                return false;

            int n = pattern.GetLength(0);
            for (int y = 0; y < n; y++)
                for (int x = 0; x < n; x++)
                {
                    PatternCell test = pattern[x, y];
                    if (test == PatternCell.Any)
                        continue;

                    var currentLocation = new Tuple<int, int>(x, y);
                    var boardLocation = pos.VectorAdd(currentLocation.VectorSubtract(new Tuple<int, int>(1, 1)));
                    var boardCell = board.GetItemAt(boardLocation);

                    if ((byte)boardCell != (byte)test)
                        return false;
                }

            return true;
        }


        static PatternCell[,] RotatePattern(PatternCell[,] pattern)
        {
            int n = pattern.GetLength(0);
            PatternCell[,] ret = new PatternCell[n, n];

            for (int i = 0; i < n; ++i)
            {
                for (int j = 0; j < n; ++j)
                {
                    ret[i, j] = pattern[n - j - 1, i];
                }
            }

            return ret;
        }

    }


    public static class Extensions
    {
        public static string PrettyPrint(this PatternCell[,] cell)
        {
            StringBuilder s = new StringBuilder();
            for (int y = 0; y < cell.GetLength(1); y++)
            {
                for (int x = 0; x < cell.GetLength(0); x++)
                {
                    var stone = cell[x, y];
                    switch (stone)
                    {
                        case PatternCell.Edge:
                            s.Append("x");
                            break;
                        case PatternCell.White:
                            s.Append("w");
                            break;
                        case PatternCell.Black:
                            s.Append("b");
                            break;
                        case PatternCell.Empty:
                            s.Append("+");
                            break;
                        case PatternCell.Any:
                            s.Append("*");
                            break;
                    }

                }
                s.Append(Environment.NewLine);
            }

            return s.ToString();
        }

    }

    public class Pattern
    {
        public PatternCell[,] Template;
        public int Response;

        public override string ToString()
        {
            StringBuilder s = new StringBuilder();
            for (int y = 0; y < Template.GetLength(1); y++)
                for (int x = 0; x < Template.GetLength(0); x++)
                {
                    var stone = Template[x, y];
                    switch (stone)
                    {
                        case PatternCell.Edge:
                            s.Append("x");
                            break;
                        case PatternCell.White:
                            s.Append("w");
                            break;
                        case PatternCell.Black:
                            s.Append("b");
                            break;
                        case PatternCell.Empty:
                            s.Append("+");
                            break;
                        case PatternCell.Any:
                            s.Append("*");
                            break;
                    }
                }
            s.Append(Environment.NewLine);
            return s.ToString();
        }
    }



    public enum PatternCell : byte
    {
        Empty = 0,
        Black = 1,
        White = 2,
        Edge = 3,
        Any = 4
    }
}
