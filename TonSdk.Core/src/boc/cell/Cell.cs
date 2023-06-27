using System.Collections;
using System.Runtime.CompilerServices;

namespace TonSdk.Core.Boc;


public class Cell {
    public readonly Bits   bits;
    public readonly Cell[] refs;
    public readonly CellType type;

    public readonly bool isExotic;
    public readonly int  refCount;
    public readonly int  fullData; // количество полных полубайт (4 бита)
    public readonly int  depth;
    private Bits? _bitsWithDescriptors;
    private Bits? _hash;

    public Bits BitsWithDescriptors {
        get { return _bitsWithDescriptors == null ? buildBitsWithDescriptors() : _bitsWithDescriptors; }
    }

    public Bits Hash {
        get { return _hash ?? calcHash(); }
    }


    public Cell (Bits _bits, Cell[] _refs, CellType _type = CellType.ORDINARY) {
        if (_bits.Length > CellTraits.max_bits) {
            throw new ArgumentException($"Bits should have at most {CellTraits.max_bits} bits.", nameof(bits));
        }

        if (_refs.Length > CellTraits.max_refs) {
            throw new ArgumentException($"Refs should have at most {CellTraits.max_refs} elements.", nameof(refs));
        }

        bits = _bits;
        refs = _refs;
        type = _type;
        refCount = _refs.Length;
        fullData = Math.Max((_bits.Length + 7) / 8 + _bits.Length / 8, 1);
        isExotic = type != CellType.ORDINARY;
        depth = refCount == 0 ? 0 : _refs.Max(cell => cell.depth) + 1;
    }

    public Cell (string bitString, params Cell[] refs) :
        this(new Bits(bitString), refs) { }

    public static Cell From(string bitsString) {
        return From(new Bits(bitsString));
    }

    public static Cell From(Bits bits) {
        return BagOfCells.DeserializeBoc(bits)[0];
    }

    private string toFiftHex(ushort indent = 1, int size = 0) {
        var output = new List<string> { string.Concat(Enumerable.Repeat(" ", indent * size)) + bits.ToString("fiftHex") };
        output.AddRange(refs.Select(cell => $"\n{cell.toFiftHex(indent, size + 1)}"));
        return string.Join("", output);
    }

    private string toFiftBin(ushort indent = 1, int size = 0) {
        var output = new List<string> { string.Concat(Enumerable.Repeat(" ", indent * size)) + bits.ToString("fiftBin") };

        foreach (var cell in refs) {
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
        var augmented = bits.Augment(8);
        var l = 16 + augmented.Length;
        var d1 = refCount + (isExotic ? 8 : 0); // + MaxLevel * 32;
        var d2 = fullData;
        var bb = new BitsBuilder(l)
                            .StoreUInt(d1, 8)
                            .StoreUInt(d2, 8)
                            .StoreBits(augmented);

        _bitsWithDescriptors = bb.Build();
        return _bitsWithDescriptors;
    }


    private Bits calcHash() {
        var bitsWithDescriptors = BitsWithDescriptors;
        var l = bitsWithDescriptors.Length + refCount * (16 + 256);
        var bb = new BitsBuilder(l).StoreBits(bitsWithDescriptors, false);
        for (var i = 0; i < refCount; i++) {
            bb.StoreUInt(refs[i].depth, 16);
        }
        for (var i = 0; i < refCount; i++) {
            bb.StoreBits(refs[i].Hash, false);
        }

        _hash = bb.Build().Hash();
        return _hash;
    }
}
