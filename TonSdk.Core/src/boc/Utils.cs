using System.Numerics;

namespace TonSdk.Core.Boc;

public static class Utils {
    public static byte reverseBits(this byte b) {
        byte r = 0;
        for (var i2 = 0; i2 < 8; i2++) {
            r <<= 1;            // Shift the result to the left
            r |= (byte)(b & 1); // Write the least significant bit of the number to the result
            b >>= 1;            // Shift the number to the right
        }
        return r;
    }

    public static int bitLength(this Int32 x) {
        return x == 0 ? 1 : BitOperations.Log2((uint)x) + 1;
    }

    public static int bitLength(this UInt32 x) {
        return x == 0 ? 1 : BitOperations.Log2(x) + 1;
    }

    public static T[] slice<T>(this T[] source, int start, int end) {
        var l = end - start;
        T[] slice = new T[l];
        Array.Copy(source, start, slice, 0, l);
        return slice;
    }
}
