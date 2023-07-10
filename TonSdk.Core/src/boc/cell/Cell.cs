using System;
using System.Collections.Generic;
using System.Linq;

namespace TonSdk.Core.Boc {
    public class Cell {
        public readonly Bits Bits;
        public readonly Cell[] Refs;
        public readonly CellType Type;

        public readonly bool IsExotic;
        public readonly int RefsCount;
        public readonly int BitsCount;
        public readonly int FullData;
        public readonly int Depth;
        private Bits? _bitsWithDescriptors;
        private Bits? _hash;

        public Bits BitsWithDescriptors {
            get { return _bitsWithDescriptors == null ? buildBitsWithDescriptors() : _bitsWithDescriptors; }
        }

        public Bits Hash {
            get { return _hash ?? calcHash(); }
        }


        public Cell(Bits _bits, Cell[] _refs, CellType _type = CellType.ORDINARY) {
            if (_bits.Length > CellTraits.max_bits) {
                throw new ArgumentException($"Bits should have at most {CellTraits.max_bits} bits.", nameof(Bits));
            }

            if (_refs.Length > CellTraits.max_refs) {
                throw new ArgumentException($"Refs should have at most {CellTraits.max_refs} elements.", nameof(Refs));
            }

            Bits = _bits;
            Refs = _refs;
            Type = _type;
            RefsCount = _refs.Length;
            BitsCount = _bits.Length;
            FullData = (_bits.Length + 7) / 8 + _bits.Length / 8;
            IsExotic = Type != CellType.ORDINARY;
            Depth = RefsCount == 0 ? 0 : _refs.Max(cell => cell.Depth) + 1;
        }

        public Cell(string bitString, params Cell[] refs) :
            this(new Bits(bitString), refs) { }

        public static Cell From(string bitsString) {
            return From(new Bits(bitsString));
        }

        public static Cell From(Bits bits) {
            return BagOfCells.DeserializeBoc(bits)[0];
        }

        private string toFiftHex(ushort indent = 1, int size = 0) {
            var output = new List<string>
                { string.Concat(Enumerable.Repeat(" ", indent * size)) + Bits.ToString("fiftHex") };
            output.AddRange(Refs.Select(cell => $"\n{cell.toFiftHex(indent, size + 1)}"));
            return string.Join("", output);
        }

        private string toFiftBin(ushort indent = 1, int size = 0) {
            var output = new List<string>
                { string.Concat(Enumerable.Repeat(" ", indent * size)) + Bits.ToString("fiftBin") };

            foreach (var cell in Refs) {
                output.Add($"\n{cell.toFiftBin(indent, size + 1)}");
            }

            return String.Join("", output);
        }

        public CellSlice Parse() {
            return new CellSlice(this);
        }

        public override string ToString() {
            return ToString("base64");
        }

        public string ToString(string mode) {
            return mode switch {
                "hex" => Serialize().ToString("hex"),
                "fiftBin" => toFiftBin(),
                "fiftHex" => toFiftHex(),
                "base64" => Serialize().ToString("base64"),
                "base64url" => Serialize().ToString("base64url"),
                _ => throw new ArgumentException("Unknown mode, supported: hex, fiftBin, fiftHex, base64, base64url")
            };
        }

        public Bits Serialize(
            bool hasIdx = false,
            bool hasCrc32C = true
        ) {
            return BagOfCells.SerializeBoc(this, hasIdx, hasCrc32C);
        }


        private Bits buildBitsWithDescriptors() {
            var augmented = Bits.Augment(8);
            var l = 16 + augmented.Length;
            var d1 = RefsCount + (IsExotic ? 8 : 0); // + MaxLevel * 32;
            var d2 = FullData;
            var bb = new BitsBuilder(l)
                .StoreUInt(d1, 8)
                .StoreUInt(d2, 8)
                .StoreBits(augmented);

            _bitsWithDescriptors = bb.Build();
            return _bitsWithDescriptors;
        }


        private Bits calcHash() {
            var bitsWithDescriptors = BitsWithDescriptors;
            var l = bitsWithDescriptors.Length + RefsCount * (16 + 256);
            var bb = new BitsBuilder(l).StoreBits(bitsWithDescriptors, false);
            for (var i = 0; i < RefsCount; i++) {
                bb.StoreUInt(Refs[i].Depth, 16);
            }

            for (var i = 0; i < RefsCount; i++) {
                bb.StoreBits(Refs[i].Hash, false);
            }

            _hash = bb.Build().Hash();
            return _hash;
        }
    }
}
