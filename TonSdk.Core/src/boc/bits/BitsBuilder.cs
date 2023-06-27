using System.Collections;
using System.Numerics;

namespace TonSdk.Core.Boc;

public abstract class BitsBuilderImpl<T, U> where T : BitsBuilderImpl<T, U> {

    protected void CheckBitsOverflow(Bits bits) {
        if (bits.Length > RemainderBits) {
            throw new ArgumentException("Bits overflow");
        }
    }

    protected void CheckBitsOverflow(BitArray bits) {
        if (bits.Length > RemainderBits) {
            throw new ArgumentException("Bits overflow");
        }
    }

    protected void CheckBitsOverflow(int len) {
        if (len > RemainderBits) {
            throw new ArgumentException("Bits overflow");
        }
    }

    protected BitArray _data;
    protected int _bits_cnt = 0;

    public BitsBuilderImpl (int length = 1023) {
        _data = new BitArray(length);
    }

    protected BitsBuilderImpl (BitArray bits, int cnt) {
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

    public T StoreBits(Bits bits, bool needCheck = true) {
        if (needCheck) CheckBitsOverflow(bits);

        write(bits, _bits_cnt);
        _bits_cnt += bits.Length;

        return (T)this;
    }

    public T StoreBits(BitArray bits, bool needCheck = true) {
        if (needCheck) CheckBitsOverflow(bits);

        write(bits, _bits_cnt);
        _bits_cnt += bits.Length;

        return (T)this;
    }

    public T StoreBits(string s, bool needCheck = true) {
        var bits = new Bits(s);
        return StoreBits(bits, needCheck);
    }

    public T StoreBit(bool b, bool needCheck = true) {
        if (needCheck) CheckBitsOverflow(1);
        _data[_bits_cnt++] = b;
        return (T)this;
    }

    public T StoreUInt(UInt64 value, int size, bool needCheck = true) {
        var max = new BigInteger(1) << size;
        if (value >= max) {
            throw new ArgumentException("");
        }

        return storeNumber(value, size, needCheck);
    }

    public T StoreUInt(BigInteger value, int size, bool needCheck = true) {
        var max = new BigInteger(1) << size;
        if (value < 0 || value >= max) {
            throw new ArgumentException("");
        }

        return storeNumber(value, size, needCheck);
    }

    public T StoreInt(Int64 value, int size, bool needCheck = true) {
        var max = BigInteger.One << (size - 1);
        if (value < -max || value > max) {
            throw new ArgumentException("");
        }

        return storeNumber(value, size, needCheck);
    }

    public T StoreInt(BigInteger value, int size, bool needCheck = true) {
        var max = BigInteger.One << (size - 1);
        if (value < -max || value > max) {
            throw new ArgumentException("");
        }

        return storeNumber(value, size, needCheck);
    }

    public T StoreUInt32LE(uint value) {
        var bytes = BitConverter.GetBytes(value);
        var bits = new Bits(ref bytes);
        return StoreBits(bits);
    }

    private T storeNumber(BigInteger value, int size, bool needCheck) {
        return storeNumberInternal(value.ToByteArray(), size, needCheck);
    }

    private T storeNumber(UInt64 value, int size, bool needCheck) {
        return storeNumberInternal(BitConverter.GetBytes(value), size, needCheck);
    }

    private T storeNumber(Int64 value, int size, bool needCheck) {
        return storeNumberInternal(BitConverter.GetBytes(value), size, needCheck);
    }

    private T storeNumberInternal(byte[] bytes, int size, bool needCheck) {
        if (BitConverter.IsLittleEndian)
            Array.Reverse(bytes);

        BitArray bitArray = new Bits(ref bytes).Data;
        var change = size - bitArray.Count;

        if (change < 0) {
            bitArray.RightShift(-change);
            bitArray.Length = size;
        } else {
            var sgn = bitArray[0]; // Check sign bit
            bitArray.Length = size;
            bitArray.LeftShift(change);

            if (sgn) {
                var leadingArray = new BitArray(change, sgn);
                leadingArray.Length = size;
                bitArray.Or(leadingArray);
            }
        }

        var bits = new Bits(bitArray);
        return StoreBits(bits);
    }

    public T StoreAddress(Address? address) {
        if (address == null) {
            return StoreBits(new BitArray(2, false));
        }
        CheckBitsOverflow(267);
        StoreUInt(0b100, 3);
        StoreInt(address.GetWorkchain(), 8);
        StoreBits(new Bits(address.GetHash()));
        return (T)this;
    }

    public T StoreCoins(Coins coins) {
        return StoreVarUInt(coins.ToBigInt(), 16);
    }

    public T StoreVarUInt(BigInteger value, int length) {
        return StoreVarInt(value, length, false);
    }

    public T StoreVarInt(BigInteger value, int length) {
        return StoreVarInt(value, length, true);
    }

    protected T StoreVarInt(BigInteger value, int length, bool sgn) {
        int size = (int)Math.Ceiling(Math.Log(length, 2));
        var sizeBytes = (int)Math.Ceiling(BigInteger.Log(value, 2) / 8);
        var sizeBits = sizeBytes * 8;
        CheckBitsOverflow(sizeBits + size);
        return value == 0
            ? StoreUInt(0, size)
            : sgn
                ? StoreUInt((uint)sizeBytes, size).StoreInt(value, sizeBits)
                : StoreUInt((uint)sizeBytes, size).StoreUInt(value, sizeBits);

    }

    public abstract T Clone();

    public abstract U Build();


    protected void write(Bits newBits, int offset) {
        write(newBits.Data, offset);
    }

    protected void write(BitArray newBits, int offset) {
        var _newBits = (BitArray)newBits.Clone();
        _newBits.Length = _data.Length;
        _newBits.LeftShift(offset);
        _data.Or(_newBits);
    }
}

public class BitsBuilder : BitsBuilderImpl<BitsBuilder, Bits> {
    public BitsBuilder (int length = 1023) : base(length) { }

    private BitsBuilder (BitArray bits, int cnt) : base(bits, cnt) { }

    public override BitsBuilder Clone() {
        return new BitsBuilder((BitArray)_data.Clone(), _bits_cnt);
    }

    public override Bits Build() {
        return Data;
    }
}
