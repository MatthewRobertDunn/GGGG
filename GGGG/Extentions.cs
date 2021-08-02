using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using GGGG.Interface;
using GGGG.Twister;

namespace GGGG
{

    using System;
    using System.Collections.Generic;


    public static class Extentions
    {
        public static void AddRangeUnique<T>(this List<T> source, IEnumerable<T> dest)
        {
            foreach (var item in dest)
            {
                if (!source.Contains(item))
                    source.Add(item);
            }
        }

        public static void AddUnique<T>(this List<T> source, T item)
        {
            if (!source.Contains(item))
            {
                source.Add(item);
            }
        }


        public static IEnumerable<IEnumerable<T>> Permutate<T>(this IList<T> list)
        {
            return list.Permutate(list.Count);
        }

        public static IEnumerable<IEnumerable<T>> Permutate<T>(this IList<T> list, int count)
        {

            var result = list.SelectMany(x => list, (x, y) => new[] { x, y }).ToList();

            //Reduce count as the above makes permutations of length 2
            count = count - 2;
            while (count > 0)
            {
                result = result.SelectMany(x => list, (x, y) =>
                                                          {
                                                              var newList = new T[x.Length + 1];
                                                              x.CopyTo(newList, 0);
                                                              newList[x.Length] = y;
                                                              return newList;
                                                          }
                    ).ToList();

                count--;
            }

            return result;
        }


        //public static IEnumerable<IEnumerable<T>> Combinations<T>(this IEnumerable<T> set)
        //{
        //    int n = set.Count();

        //    for (int skip = 0; skip < n; skip++)
        //    {
        //        for (int take = 1; take <= n - skip; take++)
        //        {
        //            yield return set.Skip(skip).Take(take);
        //        }
        //    }
        //}

        public static Tuple<int, int> VectorAdd(this Tuple<int, int> a, Tuple<int, int> b)
        {
            return new Tuple<int, int>(a.Item1 + b.Item1, a.Item2 + b.Item2);
        }

        public static Tuple<int, int> VectorSubtract(this Tuple<int, int> a, Tuple<int, int> b)
        {
            return new Tuple<int, int>(a.Item1 - b.Item1, a.Item2 - b.Item2);
        }

        public static int XYToPos(this Tuple<int, int> xy, int boardSize)
        {
            return xy.Item2 * boardSize + xy.Item1;
        }

        public static double DistanceFrom(this Tuple<int, int> from, Tuple<int, int> to)
        {
            return Math.Sqrt(Math.Pow(from.Item1 - to.Item1, 2) + Math.Pow(from.Item2 - to.Item2, 2));
        }

        public static GoString[] DeepClone(this GoString[] toClone)
        {
            var newStrings = new GoString[toClone.Length];

            for (int i = 0; i < toClone.Length; i++)
            {
                var item = toClone[i];
                if (item != null)
                    newStrings[i] = toClone[i].Clone();
            }

            return newStrings;
        }

        public static IEnumerable<T> YieldShuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = MyRandom.Random.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
                yield return value;
            }
        }

        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = MyRandom.Random.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BoardSquares GetEnemy(this BoardSquares player)
        {
            BoardSquares enemy = BoardSquares.White;
            if (player == BoardSquares.White)
                enemy = BoardSquares.Black;
            else
                enemy = BoardSquares.White;
            return enemy;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BoardSquares GetPressuredVersion(this BoardSquares player)
        {
            if (player == BoardSquares.White)
                return BoardSquares.PressuredWhite;
            else
                return BoardSquares.PressuredBlack;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BoardSquares GetUnpressuredVersion(this BoardSquares player)
        {
            if (player == BoardSquares.PressuredWhite)
                return BoardSquares.White;
            else
                return BoardSquares.Black;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static GoColor ToGoColor(this BoardSquares square)
        {
            if (square == BoardSquares.White)
                return GoColor.White;
            else
                return GoColor.Black;
        }

        [ThreadStatic]
        private static HashAlgorithm algorithm = null;

        public static string GetHashForBoard(this BoardSquares[] board)
        {
            //return board.GetHashCode().ToString();
            if (algorithm == null)
                algorithm = RIPEMD160.Create();

            var byteBoard = board.Cast<byte>().ToArray();
            var hash = algorithm.ComputeHash(byteBoard, 0, board.Length);

            return ASCIIEncoding.ASCII.GetString(hash);
        }

    }
}