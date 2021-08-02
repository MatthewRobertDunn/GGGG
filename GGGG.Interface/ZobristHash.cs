using System;
using GGGG.Interface.Twister;

namespace GGGG.Interface
{
    public class ZobristHash
    {
        public const int BoardSize = 21;
        protected Int64 HashKey;

        protected static Int64[] HashValues = InitializeHash();

        /// <summary>
        /// Gets the hash value.
        /// </summary>
        /// <value>The hash value.</value>
        public Int64 HashValue
        {
            get
            {
                return HashKey;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZobristHash"/> class.
        /// </summary>
        public ZobristHash()
        {
            HashKey = 0;
        }

        public ZobristHash(long key)
        {
            HashKey = key;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZobristHash"/> class.
        /// </summary>
        /// <param name="lZobristHash">Zobrist Hash.</param>
        protected ZobristHash(ZobristHash lZobristHash)
        {
            HashKey = lZobristHash.HashKey;
        }

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns></returns>
        public ZobristHash Clone()
        {
            return new ZobristHash(this);
        }

        /// <summary>
        /// Initializes the hash table.
        /// </summary>
        /// <returns></returns>
        protected static Int64[] InitializeHash()
        {
            var myRandom = new MersenneTwister(5836942);

            Int64[] lHashValues = new Int64[BoardSize * BoardSize * 4 + 2];

            for (int i = 0; i < lHashValues.Length; i++)
            {
                Int64 lRand = 0;

                while ((lRand == 0) || (lRand == ~0))
                {
                    ulong rand1 = (ulong)myRandom.Next();
                    ulong rand2 = (ulong)myRandom.Next();
                    lRand =  (long)(rand1 << 32 | rand2);
                }

                lHashValues[i] = lRand;
            }


            return lHashValues;
        }

        protected static Int64 GetValue(BoardSquares color, int index)
        {
            return HashValues[2 + index + (BoardSize * BoardSize * (int)color)];
        }

        public void Delta(BoardSquares color, int index)
        {
            HashKey = HashKey ^ GetValue(color, index);
        }

        public void Mark()
        {
            //if (color == BoardSquares.Black)
            HashKey = HashKey ^ HashValues[0];
            //else
            //HashKey = HashKey ^ HashValues[1];
        }

        public static bool operator ==(ZobristHash l, ZobristHash r)
        {
            if (object.ReferenceEquals(l, r))
                return true;
            else if (object.ReferenceEquals(l, null) ||
                     object.ReferenceEquals(r, null))
                return false;

            return (l.HashKey == r.HashKey);
        }

        public static bool operator !=(ZobristHash l, ZobristHash r)
        {
            return !(l == r);
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            return (((ZobristHash)obj) == this);
        }

        public int CompareTo(object obj)
        {
            ZobristHash lZobristHash = (ZobristHash)obj;

            if (lZobristHash == this)
                return 0;
            else
                return -1;
        }

        public override int GetHashCode()
        {
            return Convert.ToInt32(HashKey % Int32.MaxValue);
        }

        public override string ToString()
        {
            return HashKey.ToString();
        }
    }
}
