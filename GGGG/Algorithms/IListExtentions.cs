using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GGGG.Algorithms
{
    public static class IListExtentions
    {
        public static void RotateRight<T>(IList<T> sequence, int count)
        {
            T tmp = sequence[count - 1];
            sequence.RemoveAt(count - 1);
            sequence.Insert(0, tmp);
        }

        public static IEnumerable<IList<T>> PermutateUnique<T>(this IList<T> sequence)
        {
            return PermutateUnique(sequence, sequence.Count);
        }

        public static IEnumerable<IList<T>> PermutateUnique<T>(this IList<T> sequence, int count)
        {
            if (count == 1) yield return sequence;
            else
            {
                for (int i = 0; i < count; i++)
                {
                    foreach (var perm in PermutateUnique(sequence, count - 1))
                        yield return perm.ToArray();
                    RotateRight(sequence, count);
                }
            }
        }

    }
}
