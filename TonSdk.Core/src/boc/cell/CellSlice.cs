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

    public CellSlice (Cell cell) : base(cell.bits) {
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

    public CellSlice SkipRef() {
        return SkipRefs(1);
    }

    public Cell ReadRef() {
        var refEnd = _refs_st + 1;
        CheckRefsUnderflow(refEnd);
        return _cell.refs[_refs_st];
    }

    public Cell LoadRef() {
        var refEnd = _refs_st + 1;
        CheckRefsUnderflow(refEnd);
        var _ref = _cell.refs[_refs_st];
        _refs_st = refEnd;
        return _ref;
    }

    public Cell RestoreRemainder() {
        return new Cell(Bits, Refs);
    }

    public override Cell Restore() {
        return _cell;
    }

}
