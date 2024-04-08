using System;

namespace TonSdk.Core.Boc {

    public class CellSlice : BitsSliceImpl<CellSlice, Cell> {

        private void CheckRefsUnderflow(int refEnd) {
            if (refEnd > _refs_en) {
                throw new ArgumentException("CellSlice refs underflow");
            }
        }

        private Cell _cell;

        private int _refs_st = 0;
        private int _refs_en;

        public CellSlice(Cell cell) : base(cell.Bits) {
            _cell = cell;
            _refs_en = cell.RefsCount;
        }

        public CellSlice(Cell cell, int bits_st, int bits_en, int refs_st, int refs_en) : base(cell.Bits, bits_st,
            bits_en) {
            _cell = cell;
            _refs_st = refs_st;
            _refs_en = refs_en;
        }

        public int RemainderRefs {
            get { return _refs_en - _refs_st; }
        }

        public Cell[] Refs {
            get { return _cell.Refs.slice(_refs_st, _refs_en); }
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
            return _cell.Refs.slice(_refs_st, refEnd);
        }

        public Cell[] LoadRefs(int size) {
            var refEnd = _refs_st + size;
            CheckRefsUnderflow(refEnd);
            var refs = _cell.Refs.slice(_refs_st, refEnd);
            _refs_st = refEnd;
            return refs;
        }

        public CellSlice SkipRef() {
            return SkipRefs(1);
        }

        public Cell ReadRef() {
            var refEnd = _refs_st + 1;
            CheckRefsUnderflow(refEnd);
            return _cell.Refs[_refs_st];
        }

        public Cell LoadRef() {
            var refEnd = _refs_st + 1;
            CheckRefsUnderflow(refEnd);
            var _ref = _cell.Refs[_refs_st];
            _refs_st = refEnd;
            return _ref;
        }

        public CellSlice SkipOptRef() {
            var optRef = ReadOptRef();

            if (optRef != null) {
                SkipBit();
                SkipRef();
            }
            else {
                SkipBit();
            }

            return this;
        }

        public Cell? ReadOptRef() {
            var opt = ReadBit();
            return opt ? ReadRef() : null;
        }

        public Cell? LoadOptRef() {
            var optRef = ReadOptRef();
            SkipBit();
            if (optRef != null) SkipRef();
            return optRef;
        }

        public HashmapE<K, V> ReadDict<K, V>(HashmapOptions<K, V> opt) {
            return HashmapE<K, V>.Deserialize(this, opt, false);
        }

        public HashmapE<K, V> LoadDict<K, V>(HashmapOptions<K, V> opt) {
            return HashmapE<K, V>.Deserialize(this, opt);
        }

        public Cell RestoreRemainder() {
            return new Cell(Bits, Refs);
        }

        public override Cell Restore() {
            return _cell;
        }

        public CellSlice Clone() {
            return new CellSlice(_cell, _bits_st, _bits_en, _refs_st, _refs_en);
        }
    }
}
