using System;

namespace TonSdk.Core.Boc {
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

        public int RemainderRefs {
            get { return CellTraits.max_refs - _ref_en; }
        }

        public CellBuilder(int length = 1023) : base(length) {
            _refs = new Cell[CellTraits.max_refs];
        }

        public Cell[] Refs {
            get { return _refs.Length == _ref_en ? _refs : _refs.slice(0, _ref_en); }
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

        public CellBuilder StoreCellSlice(CellSlice bs, bool needCheck = true) {
            if (needCheck) {
                CheckBitsOverflow(bs.RemainderBits);
                CheckRefsOverflow(bs.RemainderRefs);
            }

            StoreBits(bs.Bits, false);
            var r = bs.Refs;
            return StoreRefs(ref r, false);
        }

        public CellBuilder StoreOptRef(Cell? cell, bool needCheck = true) {
            bool opt = cell != null;
            if (needCheck) {
                CheckBitsOverflow(1);
                if (opt) CheckRefsOverflow(1);
            }

            if (opt) StoreRef(cell!, false);
            return StoreBit(opt, false);
        }

        public CellBuilder StoreDict<K, V>(HashmapE<K, V> hashmap, bool needCheck = true) {
            return StoreCellSlice(hashmap.Build().Parse(), needCheck);
        }

        public override Cell Build() {
            return new Cell(Data, Refs);
        }

        public override CellBuilder Clone() {
            throw new NotImplementedException();
        }
    }
}
