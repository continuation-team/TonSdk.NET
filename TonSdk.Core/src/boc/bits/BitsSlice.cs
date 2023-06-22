using System.Collections;

namespace TonSdk.Core.Boc;


public class BitsSlice {
    public void CheckBitsUnderflow(int bitEnd) {
        if (bitEnd > _bits_en) {
            throw new ArgumentException("Bits underflow");
        }
    }
    public bool CheckBitsUnderflowQ(int bitEnd) {
        return bitEnd > _bits_en;
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

    public BitsSlice (ref BitArray bits) {
        _bits = new Bits(bits);
        _bits_en = _bits.Length;
    }

    public BitsSlice (ref Bits bits) {
        _bits = bits;
        _bits_en = _bits.Length;
    }

    public BitsSlice (BitArray bits) {
        _bits = new Bits(bits);
        _bits_en = _bits.Length;
    }

    public BitsSlice (Bits bits) {
        _bits = bits;
        _bits_en = _bits.Length;
    }


    public BitsSlice skipBits(int size) {
        var bitEnd = _bits_st + size;
        CheckBitsUnderflow(bitEnd);
        _bits_st = bitEnd;
        return this;
    }

    public Bits readBits(int size) {
        var bitEnd = _bits_st + size;
        CheckBitsUnderflow(bitEnd);
        return new Bits(_bits.Data.slice(_bits_st, bitEnd));
    }

    public Bits loadBits(int size) {
        var bitEnd = _bits_st + size;
        CheckBitsUnderflow(bitEnd);
        var bits = _bits.Data.slice(_bits_st, bitEnd);
        _bits_st = bitEnd;
        return new Bits(bits);
    }

    public Bits Restore() {
        return _bits;
    }
}
