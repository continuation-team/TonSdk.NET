namespace TonSdk.Core.Boc;

public struct HashmapSerializers<K, V> {
    public Func<K, Bits> Key;
    public Func<V, Cell> Value;
}

public struct HashmapDeserializers<K, V> {
    public Func<Bits, K> Key;
    public Func<Cell, V> Value;
}

public class HashmapOptions<K, V> {
    public uint KeySize;
    public bool? Prefixed;
    public bool? NonEmpty;

    public HashmapSerializers<K, V>? Serializers;
    public HashmapDeserializers<K, V>? Deserializers;
}

public struct HmapNodeSer {
    public BitsSlice Key;
    public Cell Value;
}


public class Hashmap<K, V> {

    protected void CheckSerializers() {
        if (serializeKey == null || serializeValue == null) {
            throw new Exception("Serializers are not set");
        }
    }

    protected void CheckDeserializers() {
        if (deserializeKey == null || deserializeValue == null) {
            throw new Exception("Deserializers are not set");
        }
    }

    protected SortedDictionary<Bits, Cell> map;
    protected uint keySize;
    protected Func<K, Bits>? serializeKey;
    protected Func<V, Cell>? serializeValue;
    protected Func<Bits, K>? deserializeKey;
    protected Func<Cell, V>? deserializeValue;

    public Hashmap(HashmapOptions<K, V> opt) {
        switch (opt.KeySize) {
            case 0: throw new Exception("Key size is not set");
            // case > 955: throw new Exception("Key size is too big");
        }

        map = new SortedDictionary<Bits, Cell>();
        keySize = opt.KeySize;
        serializeKey = opt.Serializers?.Key;
        serializeValue = opt.Serializers?.Value;
        deserializeKey = opt.Deserializers?.Key;
        deserializeValue = opt.Deserializers?.Value;
    }

    public Hashmap<K, V> Set(K key, V value) {
        CheckSerializers();

        var k = serializeKey!(key);

        if (k.Length != keySize) throw new Exception("Wrong key size");

        map[k] = serializeValue!(value);

        return this;
    }

    public V? Get(K key) {
        CheckSerializers();
        CheckDeserializers();

        var k = serializeKey!(key);

        if (k.Length != keySize) throw new Exception("Wrong key size");

        return map.TryGetValue(k, out Cell? cell)
            ? deserializeValue!(cell)
            : default;
    }

    protected Cell? serialize() {
        var nodes = new List<HmapNodeSer>(map.Count);

        nodes.AddRange(map.Select(kvp => new HmapNodeSer() { Key = kvp.Key.Parse(), Value = kvp.Value }));

        return nodes.Count == 0 ? null : serializeEdge(nodes);
    }

    protected Cell serializeEdge(List<HmapNodeSer> nodes) {
        var edge = new CellBuilder();
        var label = serializeLabel(nodes);

        edge.StoreBits(label);

        // hmn_leaf#_ {X:Type} value:X = HashmapNode 0 X;
        if (nodes.Count == 1) {
            Cell leaf = serializeLeaf(nodes[0]);
            edge.StoreCellSlice(leaf.Parse());
        }

        // hmn_fork#_ {n:#} {X:Type} left:^(Hashmap n X) right:^(Hashmap n X) = HashmapNode (n + 1) X;
        if (nodes.Count > 1) {
            var (leftNodes, rightNodes) = serializeFork(nodes);
            Cell leftEdge = serializeEdge(leftNodes);

            edge.StoreRef(leftEdge);

            if (rightNodes.Count > 0) {
                Cell rightEdge = serializeEdge(rightNodes);

                edge.StoreRef(rightEdge);
            }
        }

        return edge.Build();
    }

    protected (List<HmapNodeSer>, List<HmapNodeSer>) serializeFork(List<HmapNodeSer> nodes) {
        var leftNodes = new List<HmapNodeSer>(nodes.Count);
        var rightNodes = new List<HmapNodeSer>(nodes.Count);

        foreach (var node in nodes) {
            if (node.Key.LoadBit()) rightNodes.Add(node);
            else leftNodes.Add(node);
        }

        return (leftNodes, rightNodes);
    }

    protected Cell serializeLeaf(HmapNodeSer node) {
        return node.Value;
    }

    protected Bits serializeLabel(List<HmapNodeSer> nodes) {
        static Bits getRepeated(Bits b) {
            if (b.Length == 0) return new Bits(0);

            var bs = b.Parse();
            var f = bs.LoadBit();

            var bb = new BitsBuilder().StoreBit(f);
            for (var i = 1; i < b.Length; i++) {
                if (bs.LoadBit() != f) return bb.Build();
                bb.StoreBit(f);
            }
            return bb.Build();
        }
        // Each label can always be serialized in at least two different fashions, using
        // hml_short or hml_long constructors. Usually the shortest serialization (and
        // in the case of a tie—the lexicographically smallest among the shortest) is
        // preferred and is generated by TVM hashmap primitives, while the other
        // variants are still considered valid.

        var first = nodes[0].Key;
        var last = nodes[^1].Key;

        // m = length at most possible bits of n (key)
        var m = first.RemainderBits;
        var sameBitsIndex = -1;

        for (var i = 0; i < m; i++) {
            if (first.ReadBit(i) != last.ReadBit(i)) {
                sameBitsIndex = i;
                break;
            }
        }

        var sameBitsLength = sameBitsIndex == -1 ? first.RemainderBits : sameBitsIndex;

        // hml_short$0 {m:#} {n:#} len:(Unary ~n) s:(n * Bit) = HmLabel ~n m;
        if (first.ReadBit() != last.ReadBit() || m == 0) {
            return serializeLabelShort();
        }

        var label = first.LoadBits(sameBitsLength);
        var repeated = getRepeated(label);
        var labelShort = serializeLabelShort();
        var labelLong = serializeLabelLong();
        var labelSame = nodes.Count > 0 && repeated.Length > 1
            ? serializeLabelSame()
            : null;

        


    }
}


public class HashmapE {

}
