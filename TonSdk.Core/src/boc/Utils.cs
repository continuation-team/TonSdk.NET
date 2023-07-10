
using System;

namespace TonSdk.Core.Boc {

    public static class BocUtils {
        public static byte reverseBits(this byte b) {
            byte r = 0;
            for (var i2 = 0; i2 < 8; i2++) {
                r <<= 1; // Shift the result to the left
                r |= (byte)(b & 1); // Write the least significant bit of the number to the result
                b >>= 1; // Shift the number to the right
            }

            return r;
        }

        public static int bitLength(this Int32 x) {
            return x == 0 ? 1 : 32 - LeadingZeroCount((uint)(x < 0 ? ~x : x));
        }

        public static int bitLength(this UInt32 x) {
            return x == 0 ? 1 : 32 - LeadingZeroCount(x);
        }

        private static int LeadingZeroCount(uint x) {
            if (x == 0) return 32;
            int count = 0;
            for (int i = 31; i >= 0; i--) {
                if ((x & (1u << i)) != 0) break;
                count++;
            }
            return count;
        }

        public static T[] slice<T>(this T[] source, int start, int end) {
            var l = end - start;
            T[] slice = new T[l];
            Array.Copy(source, start, slice, 0, l);
            return slice;
        }
    }
}
