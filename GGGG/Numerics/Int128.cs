using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace GGGG.Numerics
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text;

    namespace GGGG.Numerics
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct Int128
        {
            private ulong int0;
            private ulong int1;
            public static Int128 Zero;
            static Int128()
            {
                Zero = new Int128();

            }


            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public unsafe Int128 FastOr(Int128* other)
            {
                Int128 result = new Int128
                    {
                        int0 = this.int0 | other->int0,
                        int1 = this.int1 | other->int1
                    };

                return result;
            }


            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static Int128 operator |(Int128 left, Int128 right)
            {
                Int128 result = new Int128
                                    {
                                        int0 = left.int0 | right.int0,
                                        int1 = left.int1 | right.int1
                                    };
                return result;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static Int128 operator &(Int128 left, Int128 right)
            {
                Int128 result = new Int128
                                    {
                                        int0 = left.int0 & right.int0,
                                        int1 = left.int1 & right.int1
                                    };
                return result;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static Int128 operator ^(Int128 left, Int128 right)
            {
                Int128 result = new Int128
                                    {
                                        int0 = left.int0 ^ right.int0,
                                        int1 = left.int1 ^ right.int1
                                    };
                return result;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool FastEquals(Int128 other)
            {
                return this.int0 == other.int0 && this.int1 == other.int1;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public unsafe bool FastEquals(Int128* other)
            {
                return this.int0 == other->int0 && this.int1 == other->int1;
            }


            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(Int128 left, Int128 other)
            {

                return left.int0 == other.int0 && left.int1 == other.int1;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool IsBitOn(int bitPosition)
            {
                if (bitPosition >= 0 && bitPosition < 64)
                {
                    return (int0 & (1UL << bitPosition)) != 0;

                }

                if (bitPosition >= 64 && bitPosition < 128)
                {
                    return (int1 & (1UL << (bitPosition - 64))) != 0;

                }
                return false;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void TurnOnBit(int bitPosition)
            {
                if (bitPosition >= 0 && bitPosition < 64)
                {
                    int0 = int0 | (1UL << bitPosition);
                    return;
                }

                if (bitPosition >= 64 && bitPosition < 128)
                {
                    int1 = int1 | (1UL << (bitPosition - 64));
                    return;
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void TurnBitOff(int bitPosition)
            {
                if (bitPosition >= 0 && bitPosition < 64)
                {
                    int0 = (int0 & ~(1UL << bitPosition));
                    return;
                }

                if (bitPosition >= 64 && bitPosition < 128)
                {
                    int1 = (int1 & ~(1UL << (bitPosition - 64)));
                    return;
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(Int128 left, Int128 right)
            {
                return !(left == right);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool Equals(Int128 other)
            {
                return int0 == other.int0 && int1 == other.int1;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                return obj is Int128 && Equals((Int128)obj);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override int GetHashCode()
            {
                unchecked
                {
                    int hashCode = int0.GetHashCode();
                    hashCode = (hashCode * 397) ^ int1.GetHashCode();
                    return hashCode;
                }
            }

        }
    }
}
