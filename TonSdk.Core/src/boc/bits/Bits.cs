using System.Collections;
using System.Security.Cryptography;
using TonSdk.Core.Boc.Utils;

namespace TonSdk.Core.Boc;


public class Bits {
    private static char[] hexSymbols = {
        '0', '1', '2', '3', '4', '5', '6', '7',
        '8', '9', 'A', 'B', 'C', 'D', 'E', 'F'
    };

    private BitArray _data;

    public int Length {
        get { return _data.Length; }
    }

    public BitArray Data {
        get { return _data; }
    }


    public Bits(int length = 1023) {
        _data = new BitArray(length);
    }

    public Bits(ref BitArray b) {
        _data = b;
    }

    public Bits(BitArray b): this(ref b) { }

    public Bits(ref string s) : this(fromString(s)) { }
    public Bits(string s) : this(ref s) { }

    public Bits(ref byte[] bytes) {
        for (var i = 0; i < bytes.Length; i++) {
            bytes[i] = bytes[i].reverseBits();
        }
        _data = new BitArray(bytes);
    }

    public Bits(byte[] bytes) : this(ref bytes) { }

    public Bits Slice(int start, int end) {
        var ret = (BitArray)_data.Clone();
        ret.RightShift(start);
        ret.Length = end - start;
        return new Bits(ret);
    }

    public Bits Augment(int divider = 8) {
        BitArray _bits = (BitArray)_data.Clone();
        if (divider != 4 && divider != 8) {
            throw new ArgumentException("Invalid divider. Can be (4 | 8)", nameof(divider));
        }

        var l = _bits.Length;
        var newL = (l + divider - 1) / divider * divider;
        if (l == newL) return this;

        _bits.Length = newL;
        _bits[^(newL - l)] = true;

        return new Bits(_bits);
    }

    private static BitArray fromString(string s) {
        static BitArray fromBinaryString(string bitString) {
            var bits = new BitArray(bitString.Length);
            for (int i = 0; i < bitString.Length; i++) {
                bits.Set(i, bitString[i] == '1');
            }
            return bits;
        }
        static BitArray fromHexString(string hexString) {
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
        static BitArray fromFiftBinary(string fiftBits) {
            return fromBinaryString(fiftBits.Substring(2, fiftBits.Length - 3));
        }
        static BitArray fromFiftHex(string fiftHex) {
            return fromHexString(fiftHex.Substring(2, fiftHex.Length - 3));
        }
        static BitArray fromBase64(string base64, bool url = false) {
            if (url) {
                base64 = base64.Replace('-', '+').Replace('_', '/');
            }
            while (base64.Length % 4 != 0) {
                base64 += "=";
            }
            var bytes = Convert.FromBase64String(base64);
            for (var i = 0; i < bytes.Length; i++) {
                bytes[i] = bytes[i].reverseBits();
            }
            return new BitArray(bytes);
        }

        BitArray bits;
        if (s.isBinaryString()) {
            bits = fromBinaryString(s);
        } else if (s.isHexString()) {
            bits = fromHexString(s);
        } else if (s.isBase64()) {
            bits = fromBase64(s);
        } else if (s.isBase64url()) {
            bits = fromBase64(s, true);
        } else if (s.isFiftBinary()) {
            bits = fromFiftBinary(s);
        } else if (s.isFiftHex()) {
            bits = fromFiftHex(s);
        } else {
            throw new ArgumentException("Unknown string type, supported: binary, hex, fiftBinary, fiftHex");
        }
        return bits;
    }

    public Bits Hash() {
        var hashBytes = SHA256.HashData(ToBytes());
        return new Bits(ref hashBytes);
    }

    public T[] GetCopyTo<T>(T[] to) {
        _data.CopyTo(to, 0);
        return to;
    }

    public byte[] ToBytes(bool needReverse = true) {
        var bytes = GetCopyTo(new byte[(_data.Length + 7) / 8]);

        if (!BitConverter.IsLittleEndian || !needReverse) return bytes;

        for (var i = 0; i < bytes.Length; i++) {
            bytes[i] = bytes[i].reverseBits();
        }

        return bytes;
    }

    public override string ToString() {
        return ToString("base64");
    }

    public string ToString(string mode) {
        string toBinaryString(bool fift = false) {
            var bools = new bool[_data.Length];
            Data.CopyTo(bools, 0);
            var chars = Array.ConvertAll(bools, b => b ? '1' : '0');
            var newStr = new string(chars);
            return fift ? $"b{{{newStr}}}" : newStr;
        }
        string toHexString(bool fift = false) {
            var _bits = new Bits(Data);
            var length = _data.Length;
            var areDivisible = length % 4 == 0;
            var augmented = areDivisible ? _bits.Data : _bits.Augment(4).Data;
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
            var newStr = new string(hexChars);
            return fift ? $"x{{{newStr}}}" : newStr;
        }
        string toBase64(bool url = false) {
            var bytes = ToBytes();
            var base64 = Convert.ToBase64String(bytes);
            return url
                ? base64.TrimEnd('=').Replace('+', '-').Replace('/', '_')
                : base64;
        }

        return mode switch {
            "bin" => toBinaryString(),
            "hex" => toHexString(),
            "fiftBin" => toBinaryString(true),
            "fiftHex" => toHexString(true),
            "base64" => toBase64(),
            "base64url" => toBase64(true),
            _ => throw new ArgumentException("Unknown mode, supported: bin, hex, fiftBin, fiftHex, base64, base64url")
        };
    }

    public virtual Bits Clone() {
        return new Bits(_data);
    }

    public BitsSlice Parse() {
        return new BitsSlice(this);
    }

    public BitArray Unwrap() {
        return _data;
    }

}
