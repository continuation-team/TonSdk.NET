using TonSdk.Core.Boc.Utils;

namespace TonSdk.Core.Boc;

public class CellSlice : BitsSliceImpl<CellSlice, Cell> {

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

    public CellSlice SkipRefs(int size) {
        var refEnd = _refs_st + size;
        CheckRefsUnderflow(refEnd);
        _refs_st = refEnd;
        return this;
    }

    public Cell[] ReadRefs(int size) {
        var refEnd = _refs_st + size;
        CheckRefsUnderflow(refEnd);
        return _cell.refs.slice(_refs_st, refEnd);
    }

    public Cell[] LoadRefs(int size) {
        var refEnd = _refs_st + size;
        CheckRefsUnderflow(refEnd);
        var refs = _cell.refs.slice(_refs_st, refEnd);
        _refs_st = refEnd;
        return refs;
    }

    public override Cell Restore() {
        return _cell;
    }

}
