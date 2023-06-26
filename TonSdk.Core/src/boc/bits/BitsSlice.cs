using System.Collections;
using System.Numerics;

namespace TonSdk.Core.Boc;


public abstract class BitsSliceImpl<T, U> where T : BitsSliceImpl<T, U> {
    protected void CheckBitsUnderflow(int bitEnd) {
        if (bitEnd > _bits_en) {
            throw new ArgumentException("Bits underflow");
        }
    }
    protected bool CheckBitsUnderflowQ(int bitEnd) {
        return bitEnd > _bits_en;
    }

    protected void CheckSize(int size) {
        if (size < 0) {
            throw new ArgumentException("Invalid size. Must be >= 0", nameof(size));
        }
    }

    protected Bits _bits;
    protected int _bits_st = 0;
    protected int _bits_en;

    public int RemainderBits {
        get { return _bits_en - _bits_st; }
    }

    public Bits Bits {
        get { return new Bits(_bits.Data.slice(_bits_st, _bits_en)); }
    }

    public BitsSliceImpl (BitArray bits) {
        _bits = new Bits(bits);
        _bits_en = _bits.Length;
    }

    public BitsSliceImpl (Bits bits) {
        _bits = bits;
        _bits_en = _bits.Length;
    }


    public T SkipBits(int size) {
        CheckSize(size);
        var bitEnd = _bits_st + size;
        CheckBitsUnderflow(bitEnd);
        _bits_st = bitEnd;
        return (T)this;
    }

    public Bits ReadBits(int size) {
        CheckSize(size);
        var bitEnd = _bits_st + size;
        CheckBitsUnderflow(bitEnd);
        return new Bits(_bits.Data.slice(_bits_st, bitEnd));
    }

    public Bits LoadBits(int size) {
        CheckSize(size);
        var bitEnd = _bits_st + size;
        CheckBitsUnderflow(bitEnd);
        var bits = _bits.Data.slice(_bits_st, bitEnd);
        _bits_st = bitEnd;
        return new Bits(bits);
    }

    public T SkipBit() {
        return SkipBits(1);
    }

    public bool ReadBit() {
        var bitEnd = _bits_st + 1;
        CheckBitsUnderflow(bitEnd);
        return _bits.Data[_bits_st];
    }

    public bool LoadBit() {
        var bitEnd = _bits_st + 1;
        CheckBitsUnderflow(bitEnd);
        var bit = _bits.Data[_bits_st];
        _bits_st = bitEnd;
        return bit;
    }

    public BigInteger ReadUInt(int size) {
        CheckSize(size);
        var bitEnd = _bits_st + size;
        CheckBitsUnderflow(bitEnd);
        return _unsafeReadBigInteger(size);
    }

    public BigInteger LoadUInt(int size) {
        CheckSize(size);
        var bitEnd = _bits_st + size;
        CheckBitsUnderflow(bitEnd);
        var result = _unsafeReadBigInteger(size);
        _bits_st = bitEnd;
        return result;
    }

    public BigInteger ReadInt(int size) {
        CheckSize(size);
        var bitEnd = _bits_st + size;
        CheckBitsUnderflow(bitEnd);
        return _unsafeReadBigInteger(size, true);
    }

    public BigInteger LoadInt(int size) {
        CheckSize(size);
        var bitEnd = _bits_st + size;
        CheckBitsUnderflow(bitEnd);
        var result = _unsafeReadBigInteger(size, true);
        _bits_st = bitEnd;
        return result;
    }

    public BigInteger LoadUInt32LE() {
        var bytes = LoadBits(32).ToBytes();
        return new BigInteger(bytes);
    }

    public abstract U Restore();

    private BigInteger _unsafeReadBigInteger(int size, bool sgn = false) {
        BigInteger result = 0;

        for (int i = 0; i < size; i++) {
            if (_bits.Data[_bits_st + i]) {
                result |= BigInteger.One << (size - 1 - i);
            }
        }

        // Check if the most significant bit is set (which means the number is negative)
        if (sgn & (result & (BigInteger.One << (size - 1))) != 0) {
            // If the number is negative, apply two's complement
            result -= BigInteger.One << size;
        }

        return result;
    }
}



public class BitsSlice : BitsSliceImpl<BitsSlice, Bits> {
    public BitsSlice (BitArray bits) : base(bits) { }

    public BitsSlice (Bits bits) : base(bits) { }

    public override Bits Restore() {
        return _bits;
    }

}
