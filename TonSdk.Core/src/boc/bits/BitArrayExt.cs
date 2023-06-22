/*
using System.Collections;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using TonSdk.Core.Boc;


namespace TonSdk.Core.bits;


public static class BitArrayExt {
    private static char[] hexSymbols = {
        '0', '1', '2', '3', '4', '5', '6', '7',
        '8', '9', 'A', 'B', 'C', 'D', 'E', 'F'
    };


    public static T[] getCopyTo<T>(this BitArray bits, T[] to) {
        bits.CopyTo(to, 0);
        return to;
    }

    public static bool[] toBools(this BitArray bits) {
        return bits.getCopyTo(new bool[bits.Length]);
    }


    public static BitArray reverse(this BitArray bits, bool inplace = true) {
        var _bits = inplace ? bits : (BitArray)bits.Clone();
        var temp = (BitArray)_bits.Clone();
        var l = _bits.Length;
        for (int i = 0, j = l - 1; i < l; i++, j--) {
            _bits[i] = temp[j];
            // _bits.Set(i, temp[i]);
        }
        return _bits;
    }


    public static BitArray write(this BitArray bits, BitArray newBits, int offset, bool inplace = true) {
        var _bits = inplace ? bits : (BitArray)bits.Clone();
        var l = newBits.Length;
        var _newBits = newBits;
        _newBits.Length = bits.Length;
        _newBits.LeftShift(offset);
        _bits.Or(_newBits);
        _newBits.RightShift(offset);
        _newBits.Length = l;
        return _bits; // (на цикле не быстрее)
    }


    public static BitArray concat(this BitArray bits, BitArray newBits, bool inplace = false) {
        var _bits = inplace ? bits : (BitArray)bits.Clone();
        var offset = _bits.Length;
        _bits.Length += newBits.Length;
        return _bits.write(newBits, offset);
    }


    public static BitArray slice(this BitArray bits, int start, int end, bool inplace = false) {
        var ret = inplace ? bits : (BitArray)bits.Clone();
        ret.RightShift(start);
        ret.Length = end - start;
        return ret;
    }


    public static string toBitString(this BitArray bits) {
        var bools = new bool[bits.Length];
        bits.CopyTo(bools, 0);
        var chars = Array.ConvertAll(bools, b => b ? '1' : '0');
        return new string(chars);
    }


    public static BitArray fromBinaryString(string bitString, bool needCheck = true) {
        if (needCheck) bitString.checkIsBinaryString();
        var bits = new BitArray(bitString.Length);
        for (int i = 0; i < bitString.Length; i++) {
            bits.Set(i, bitString[i] == '1');
        }
        return bits;
    }


    public static string toHexString(this BitArray bits) {
        var length = bits.Length;
        var areDivisible = length % 4 == 0;
        var augmented = areDivisible ? bits : bits.augment(4, false);
        var charCount = augmented.Length / 4;
        var hexChars = new char[charCount + (areDivisible ? 0 : 1)];
        for (var i = 0; i < charCount; i++) {
            var value = 0;
            for (var j = 0; j < 4; j++) {
                var index = i * 4 + j;
                var bit = index <= length && augmented.Get(index);
                if (bit) {
                    value |= 1 << (3 - j);
                }
            }
            hexChars[i] = hexSymbols[value];
        }
        if (!areDivisible) {
            hexChars[^1] = '_';
        }
        return new string(hexChars);
    }


    public static BitArray fromHexString(string hexString, bool needCheck = true) {
        static BitArray Parse(string h) {
            var bits = new BitArray(h.Length * 4);
            for (int i = 0; i < h.Length; i++) {
                byte b = Convert.ToByte(h.Substring(i, 1), 16);
                for (int j = 0; j < 4; j++) {
                    bits.Set(i * 4 + j, (b & (1 << (3 - j))) != 0);
                }
            }
            return bits;
        }

        if (needCheck) hexString.checkIsHexString();

        var _hexString = hexString;
        var partialEnd = _hexString[^1] == '_';

        if (!partialEnd) {
            return Parse(_hexString);
        }

        _hexString = _hexString.Substring(0, _hexString.Length - 1);
        var bits = Parse(_hexString);

        int lastTrueIndex = -1;
        for (int i = bits.Length - 1; i >= 0; i--) {
            if (bits[i]) {
                lastTrueIndex = i;
                break;
            }
        }
        bits.Length = lastTrueIndex;
        return bits;
    }


    public static string toFiftHex(this BitArray bits) {
        return $"x{{{bits.toHexString()}}}";
    }


    public static BitArray fromFiftHex(string fiftHex, bool needCheck = true) {
        if (needCheck) fiftHex.checkIsFiftHex();
        return fromHexString(fiftHex.Substring(2, fiftHex.Length - 3), false);
    }


    public static string toFiftBits(this BitArray bits) {
        return $"b{{{bits.toBitString()}}}";
    }


    public static BitArray fromFiftBits(string fiftBits, bool needCheck = true) {
        if (needCheck) fiftBits.checkIsFiftBinary();
        return fromBinaryString(fiftBits.Substring(2, fiftBits.Length - 3), false);
    }


    public static string toString(this BitArray bits) {
        var sb = new char[bits.Length * 3];
        // заменил string builder на char[], стало быстрее
        sb[0] = '[';
        for (int i = 0; i < bits.Length; i++) {
            int p = (i + 1) * 3; // pointer
            sb[p - 2] = bits[i] ? '1' : '0';
            if (i != bits.Length - 1) {
                sb[p - 1] = ',';
                sb[p] = ' ';
            }
        }
        sb[sb.Length - 1] = ']';
        return new string(sb);
    }


    public static BitArray augment(this BitArray bits, int divider = 8, bool inplace = true) {
        var _bits = inplace ? bits : (BitArray)bits.Clone();
        if (divider != 4 && divider != 8) {
            throw new ArgumentException("Invalid divider. Can be (4 | 8)", nameof(divider));
        }

        var l = _bits.Length;
        var newL = (l + divider - 1) / divider * divider;
        if (l == newL) return _bits;

        _bits.Length = newL;
        _bits[^(newL - l)] = true;

        return _bits;
    }


    public static byte[] toBytes(this BitArray bits, bool needReverse = true) {
        var bytes = bits.getCopyTo(new byte[(bits.Length + 7) / 8]);

        if (!BitConverter.IsLittleEndian || !needReverse) return bytes;

        for (var i = 0; i < bytes.Length; i++) {
            bytes[i] = bytes[i].reverseBits();
        }

        return bytes;
    }


    public static BitArray fromBytes(byte[] bytes) {
        var _bytes = (byte[])bytes.Clone();

        for (var i = 0; i < _bytes.Length; i++) {
            _bytes[i] = _bytes[i].reverseBits();
        }

        return new BitArray(_bytes);
    }


    public static BitArray hash(this BitArray bits) {
        return fromBytes(SHA256.HashData(bits.toBytes()));
    }

    public static BitArray fromString(string s) {
        BitArray bits;
        if (s.isBinaryString()) {
            bits = fromBinaryString(s);
        } else if (s.isHexString()) {
            bits = fromHexString(s);
        } else if (s.isFiftBinary()) {
            bits = fromFiftBits(s);
        } else if (s.isFiftHex()) {
            bits = fromFiftHex(s);
        } else {
            throw new ArgumentException("Unknown string type, supported: bitString, hexString, fiftBits, fiftHex");
        }
        return bits;
    }

    public static void Test() {
        // var b = new BitBuilder().storeUInt(1844674407370u, 64).Bits.toFiftHex();
        // var b = fromFiftBits("b{10100100111}");
        // var fh = b.toFiftHex();
        // fh.print();
        // fromFiftHex(fh).toFiftBits().print();
        // b.reverse().toFiftBits().print();
        //

        Console.WriteLine(fromHexString("FF_").toFiftBits());
        //
        // fromBinaryString("11111111111").toHexString().print();

    }
}
*/
