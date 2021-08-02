using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GGGG.Interface.Twister;
using SharpNeat.Utility;

namespace GGGG.Twister
{
    public static class MyRandom
    {
        [ThreadStatic]
        private static Random _random;

        public static Random Random
        {
            get
            {
                if (_random != null)
                    return _random;
                else
                {
                    lock (rngLock)
                    {
                        _random = new Random(masterRng.Next(int.MinValue, int.MaxValue));
                    }
                    return _random;
                }
            }
        }

        private static readonly object rngLock = new object();
        private static readonly Random masterRng = new Random();



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

    }
}
