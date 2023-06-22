﻿using TonSdk.Core.Boc.Utils;

namespace TonSdk.Core.Boc;

public class CellSlice : BitsSlice {

    private void CheckRefsUnderflow(int refEnd) {
        if (refEnd > _refs_en) {
            throw new ArgumentException("CellSlice refs underflow");
        }
    }

    private Cell _cell;

    private int _refs_st = 0;
    private int _refs_en;

    public CellSlice (ref Cell cell) : base(cell.bits) {
        _cell = cell;
        _refs_en = cell.refCount;
    }

    public int RemainderRefs {
        get { return _refs_en - _refs_st; }
    }

    public Cell[] Refs {
        get { return _cell.refs.slice(_refs_st, _refs_en); }
    }

    public CellSlice skipRefs(int size) {
        var refEnd = _refs_st + size;
        CheckRefsUnderflow(refEnd);
        _refs_st = refEnd;
        return this;
    }

    public Cell[] readRefs(int size) {
        var refEnd = _refs_st + size;
        CheckRefsUnderflow(refEnd);
        return _cell.refs.slice(_refs_st, refEnd);
    }

    public Cell[] loadRefs(int size) {
        var refEnd = _refs_st + size;
        CheckRefsUnderflow(refEnd);
        var refs = _cell.refs.slice(_refs_st, refEnd);
        _refs_st = refEnd;
        return refs;
    }

}
