using System.Collections;
using System.Numerics;
using System.Runtime.CompilerServices;
using TonSdk.Core.Boc.Utils;

namespace TonSdk.Core.Boc;

public class CellBuilder : BitsBuilder {
    private void CheckRefsOverflow(ref Cell[] refs) {
        if (_ref_en + refs.Length > CellTraits.max_refs) {
            throw new ArgumentException("CellBuilder refs overflow");
        }
    }

    private void CheckRefsOverflow(int refs_cnt) {
        if (_ref_en + refs_cnt > CellTraits.max_refs) {
            throw new ArgumentException("CellBuilder refs overflow");
        }
    }

    private Cell[] _refs;

    private int _ref_en = 0;

    public CellBuilder () {
        _refs = new Cell[CellTraits.max_refs];
    }

    public Cell[] Refs {
        get { return _refs.Length == _ref_en ? _refs :_refs.slice(0, _ref_en); }
    }



    public CellBuilder storeRefs(ref Cell[] refs, bool needCheck = true) {
        if (needCheck) CheckRefsOverflow(ref refs);

        foreach (var cell in refs) {
            _refs[_ref_en++] = cell;
        }

        return this;
    }


    public CellBuilder storeRef(ref Cell cell, bool needCheck = true) {
        if (needCheck) CheckRefsOverflow(1);

        _refs[_ref_en++] = cell;

        return this;
    }


    // public CellBuilder storeInt(Int64 value, int size) {
    //     storeInt(value, size);
    //     return this;
    // }
    //
    //
    // public CellBuilder storeUInt(UInt64 value, int size) {
    //     _bits.storeUInt(value, size);
    //     return this;
    // }
    //
    //
    // public CellBuilder storeBigInt(BigInteger value, int size) {
    //     _bits.storeBigInt(value, size);
    //     return this;
    // }
    //
    //
    //
    // public CellBuilder storeBigUInt(BigInteger value, int size) {
    //     _bits.storeBigUInt(value, size);
    //     return this;
    // }
    //
    //
    //
    // public CellBuilder storeBytes(ref byte[] bytes) {
    //     BitArray bits = new BitArray(bytes);
    //     return storeBits(ref bits);
    // }
    //
    //
    // public CellBuilder storeBuilder(CellBuilder b) {
    //     return storeObject(b);
    // }
    //
    //
    // public CellBuilder storeSlice(CellSlice s) {
    //     return storeObject(s);
    // }


    public Cell finalize() {
        return new Cell(Data, Refs);
    }

    // public Cell endExoticCell() {
    //
    // }
}


/*

 */
