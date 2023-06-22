using System.Collections;
using System.Numerics;

namespace TonSdk.Core.Boc;

public class BitsBuilder {
    private void CheckBitsOverflow(ref Bits bits) {
        if (bits.Length > RemainderBits) {
            throw new ArgumentException("Bits overflow");
        }
    }

    private void CheckBitsOverflow(ref BitArray bits) {
        if (bits.Length > RemainderBits) {
            throw new ArgumentException("Bits overflow");
        }
    }

    private void CheckBitsOverflow(Bits bits) {
        if (bits.Length > RemainderBits) {
            throw new ArgumentException("Bits overflow");
        }
    }

    private void CheckBitsOverflow(BitArray bits) {
        if (bits.Length > RemainderBits) {
            throw new ArgumentException("Bits overflow");
        }
    }

    private BitArray _data;
    private int _bits_cnt = 0;

    public BitsBuilder(int length = 1023) {
        _data = new BitArray(length);
    }

    private BitsBuilder(BitArray bits, int cnt) {
        _data = bits;
        _bits_cnt = cnt;
    }

    public int Length {
        get { return _data.Length; }
    }

    public int RemainderBits {
        get { return _data.Length - _bits_cnt; }
    }

    public Bits Data {
        get { return new Bits(_data.slice(0, _bits_cnt)); }
    }

    public BitsBuilder storeBits(Bits bitArray, bool needCheck = true) {
        if (needCheck) CheckBitsOverflow(ref bitArray);

        write(bitArray, _bits_cnt);
        _bits_cnt += bitArray.Length;

        return this;
    }

    public BitsBuilder storeBits(BitArray bitArray, bool needCheck = true) {
        if (needCheck) CheckBitsOverflow(ref bitArray);

        write(bitArray, _bits_cnt);
        _bits_cnt += bitArray.Length;

        return this;
    }

    public BitsBuilder storeBits(ref Bits bits, bool needCheck = true) {
        if (needCheck) CheckBitsOverflow(ref bits);

        write(bits, _bits_cnt);
        _bits_cnt += bits.Length;

        return this;
    }

    public BitsBuilder storeBits(ref BitArray bitArray, bool needCheck = true) {
        if (needCheck) CheckBitsOverflow(ref bitArray);

        write(bitArray, _bits_cnt);
        _bits_cnt += bitArray.Length;

        return this;
    }


    public BitsBuilder storeBits(string s, bool needCheck = true) {
        var bits = new Bits(s);
        return storeBits(ref bits, needCheck);
    }

    public BitsBuilder storeBit(bool b, bool needCheck = true) {
        var bits = new Bits(new BitArray(1, b));
        return storeBits(ref bits, needCheck);
    }

    public BitsBuilder storeInt(Int64 value, int size, bool needCheck = true) {
        var max = (long)1 << size - 1;
        if (value < -max || value > max) {
            throw new ArgumentException("");
        }

        return storeNumber(value, size, needCheck);
    }

    public BitsBuilder storeUInt(Int64 value, int size, bool needCheck = true) {
        if (value < 0 || value >= ((long)1 << size)) {
            throw new ArgumentException("");
        }

        return storeNumber(value, size, needCheck);
    }

    public BitsBuilder storeUInt(UInt64 value, int size, bool needCheck = true) {
        if (value >= ((ulong)1 << size)) {
            throw new ArgumentException("");
        }

        return storeNumber(value, size, needCheck);
    }

    public BitsBuilder storeBigInt(BigInteger value, int size, bool needCheck = true) {
        var max = new BigInteger(1) << (size - 1);
        if (value < -max || value > max) {
            throw new ArgumentException("");
        }

        return storeNumber(value, size, needCheck);
    }

    public BitsBuilder storeBigUInt(BigInteger value, int size, bool needCheck = true) {
        var max = new BigInteger(1) << size;
        if (value < 0 || value >= max) {
            throw new ArgumentException("");
        }

        return storeNumber(value, size, needCheck);
    }

    private BitsBuilder storeNumber(Int64 value, int size, bool needCheck) {
        var _value = (long)value;
        byte[] bytes = BitConverter.GetBytes(_value);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(bytes);
        BitArray bitArray = new Bits(ref bytes).Data;
        var change = size - bitArray.Count;
        if (change < 0) {
            bitArray.RightShift(-change);
            bitArray.Length = size;
        } else {
            bitArray.Length = size;
            bitArray.LeftShift(change);
        }
        var bits = new Bits(bitArray);
        return storeBits(ref bits);
    }

    private BitsBuilder storeNumber(UInt64 value, int size, bool needCheck) {
        byte[] bytes = BitConverter.GetBytes(value);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(bytes);
        BitArray bitArray = new Bits(ref bytes).Data;
        var change = size - bitArray.Count;
        if (change < 0) {
            bitArray.RightShift(-change);
            bitArray.Length = size;
        } else {
            bitArray.Length = size;
            bitArray.LeftShift(change);
        }
        var bits = new Bits(bitArray);
        return storeBits(ref bits);
    }

    private BitsBuilder storeNumber(BigInteger value, int size, bool needCheck) {
        byte[] bytes = value.ToByteArray();
        if (BitConverter.IsLittleEndian)
            Array.Reverse(bytes);
        BitArray bitArray = new Bits(ref bytes).Data;
        var change = size - bitArray.Count;
        if (change < 0) {
            bitArray.RightShift(-change);
            bitArray.Length = size;
        } else {
            bitArray.Length = size;
            bitArray.LeftShift(change);
        }
        var bits = new Bits(bitArray);
        return storeBits(ref bits);
    }

    public BitsBuilder storeUInt32LE(uint value) {
        // TODO add checks
        var bytes = BitConverter.GetBytes(value);
        var bits = new Bits(ref bytes);
        return storeBits(ref bits);
    }

    public BitsBuilder Clone() {
        return new BitsBuilder((BitArray)_data.Clone(), _bits_cnt);
    }

    public Bits Build() {
        return Data;
    }


    private void write(Bits newBits, int offset) {
        write(newBits.Data, offset);
    }

    private void write(BitArray newBits, int offset) {
        var _newBits = (BitArray)newBits.Clone();
        _newBits.Length = _data.Length;
        _newBits.LeftShift(offset);
        _data.Or(_newBits);
    }

}
