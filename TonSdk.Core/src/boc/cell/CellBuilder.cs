using System.Collections;
using System.Numerics;
using System.Runtime.CompilerServices;
using TonSdk.Core.Boc.Utils;

namespace TonSdk.Core.Boc;

public class CellBuilder : BitsBuilderImpl<CellBuilder, Cell> {
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

    public CellBuilder (int length = 1023) : base(length) {
        _refs = new Cell[CellTraits.max_refs];
    }

    public Cell[] Refs {
        get { return _refs.Length == _ref_en ? _refs :_refs.slice(0, _ref_en); }
    }

    public CellBuilder StoreRefs(ref Cell[] refs, bool needCheck = true) {
        if (needCheck) CheckRefsOverflow(ref refs);

        foreach (var cell in refs) {
            _refs[_ref_en++] = cell;
        }

        return this;
    }

    public CellBuilder StoreRef(Cell cell, bool needCheck = true) {
        if (needCheck) CheckRefsOverflow(1);

        _refs[_ref_en++] = cell;

        return this;
    }

    public override Cell Build() {
        return new Cell(Data, Refs);
    }

    public override CellBuilder Clone() {
        throw new NotImplementedException();
    }

    // public Cell endExoticCell() {
    //
    // }
}


/*

 */
