using System.Collections;
using System.Runtime.CompilerServices;
using JustCRC32C;
using TonSdk.Core.Boc.Utils;

namespace TonSdk.Core.Boc;

public class BagOfCells {
    // private Cell[] _cells;
    //
    // public BagOfCells(Cell[] cells) {
    //     _cells = cells;
    // }
    //
    // public BagOfCells(Cell cell) : this(new[] { cell }) {
    // }


    private static Bits serializeCell(Cell cell, Dictionary<Bits, int> cellsIndex, int refSize) {
        var ret = cell.BitsWithDescriptors;
        refSize *= 8;
        var l = ret.Length + cell.refCount * refSize;
        var b = new BitsBuilder(l).storeBits(ref ret);
        foreach (var refCell in cell.refs) {
            var refHash = refCell.Hash;
            var refIndex = cellsIndex[refHash];
            b.storeUInt(refIndex, refSize);
        }
        return b.Build();
    }


    public static Bits serializeBoc(
        Cell root,
        bool hasIdx = false,
        bool hasCrc32C = true
    ) {
        return serializeBoc(new[] { root }, hasIdx, hasCrc32C);
    }


    private static (List<(Bits, Cell)> sortedCells, Dictionary<Bits, int> hashToIndex)
        TopologicalSort(Cell[] roots) {

        // Список уже отсортированных вершин графа
        var sortedCells = new List<(Bits, Cell)>();
        // Словарь, который отображает хеш ячейки на ее индекс в отсортированном списке
        var hashToIndex = new Dictionary<Bits, int>(new BitsEqualityComparer());

        // Рекурсивная функция обхода графа и топологической сортировки
        void VisitCell(Cell cell) {
            foreach (var neighbor in cell.refs) {
                if (!hashToIndex.ContainsKey(neighbor.Hash)) {
                    VisitCell(neighbor);
                }
            }
            // // Проверяем, что ячейка еще не добавлена в список отсортированных ячеек
            if (!hashToIndex.ContainsKey(cell.Hash)) {
            // Добавляем ячейку в начало списка отсортированных ячеек
            sortedCells.Insert(0, (cell.Hash, cell));
            // Сдвигаем уже добавленные ячейки на одну позицию вправо
            for (int i = 1; i < sortedCells.Count; i++) {
                hashToIndex[sortedCells[i].Item2.Hash]++;
            }
            // Добавляем ячейку в словарь hashToIndex
            hashToIndex[cell.Hash] = 0;
            }
        }

        // Выполняем обход и топологическую сортировку для каждой вершины графа
        for (var i = roots.Length - 1; i > -1; i--) {
            // foreach (var rootCell in roots) {
            var rootCell = roots[i];
            foreach (var cell in rootCell.refs) {
                VisitCell(cell);
            }
            VisitCell(rootCell);
        }

        return (sortedCells, hashToIndex);
    }


    public static Bits serializeBoc(
        Cell[] roots,
        bool hasIdx = false,
        bool hasCrc32C = true
        // bool hasCacheBits = false    // always false
        // uint flags = 0               // always 0
    ) {
        const bool hasCacheBits = false;
        const uint flags = 0;
        var (sortedCells, indexHashmap) = TopologicalSort(roots);

        var cellsNum = sortedCells.Count;
        var sBytes = (cellsNum.bitLength() + 7) / 8;

        var offsets = new int[cellsNum];
        var totalSize = 0;
        var dataBuilder = new BitsBuilder(cellsNum * (16 + 1024 + sBytes * 8 * 4));
        for (var i = 0; i < cellsNum; i++) {
            var serializedCell = serializeCell(sortedCells[i].Item2, indexHashmap, sBytes);
            dataBuilder.storeBits(serializedCell);
            totalSize += serializedCell.Length / 8;
            offsets[i] = totalSize;
        }

        var dataBits = dataBuilder.Build();
        var offsetBytes = (dataBits.Length.bitLength() + 7) / 8;

        /*
          serialized_boc#b5ee9c72 has_idx:(## 1) has_crc32c:(## 1)
                                  has_cache_bits:(## 1) flags:(## 2) { flags = 0 }
                                  size:(## 3) { size <= 4 }
                                  off_bytes:(## 8) { off_bytes <= 8 }
                                  cells:(##(size * 8))
                                  roots:(##(size * 8)) { roots >= 1 }
                                  absent:(##(size * 8)) { roots + absent <= cells }
                                  tot_cells_size:(##(off_bytes * 8))
                                  root_list:(roots * ##(size * 8))
                                  index:has_idx?(cells * ##(off_bytes * 8))
                                  cell_data:(tot_cells_size * [ uint8 ])
                                  crc32c:has_crc32c?uint32
                                  = BagOfCells;
         */
        var l = 32 + 1 + 1 + 1 + 2 + 3 + 8 + (sBytes * 8) + (sBytes * 8) + (sBytes * 8) + (offsetBytes * 8) +
                (roots.Length * sBytes * 8) + ((hasIdx ? 1 : 0) * cellsNum * (offsetBytes * 8)) + dataBits.Length + ((hasCrc32C ? 1 : 0) * 32);
        var bocBuilder = new BitsBuilder(l)
            .storeUInt(0xb5ee9c72, 32, false) // serialized_boc#b5ee9c72
            .storeBit(hasIdx, false) // has_idx:(## 1)
            .storeBit(hasCrc32C, false) // has_crc32c:(## 1)
            .storeBit(hasCacheBits, false) // has_cache_bits:(## 1)
            .storeUInt(flags, 2, false) // flags:(## 2) { flags = 0 }
            .storeUInt(sBytes, 3, false) // size:(## 3) { size <= 4 }
            .storeUInt(offsetBytes, 8, false) // off_bytes:(## 8) { off_bytes <= 8 }
            .storeUInt(cellsNum, sBytes * 8, false) // cells:(##(size * 8))
            .storeUInt(roots.Length, sBytes * 8) // roots:(##(size * 8)) { roots >= 1 }
            .storeUInt(0, sBytes * 8, false) // ??? absent:(##(size * 8)) { roots + absent <= cells }
            .storeUInt(dataBits.Length / 8, offsetBytes * 8, false); // tot_cells_size:(##(off_bytes * 8))

        foreach (var _rootCell in roots) {
            bocBuilder.storeUInt(indexHashmap[_rootCell.Hash], sBytes * 8,
                false); // root_list:(roots * ##(size * 8))
        }

        if (hasIdx) {
            foreach (var offset in offsets) {
                bocBuilder.storeUInt(offset, offsetBytes * 8, false); // index:has_idx?(cells * ##(off_bytes * 8))
            }
        }

        bocBuilder.storeBits(dataBits, false); // cell_data:(tot_cells_size * [ uint8 ])

        if (hasCrc32C) {
            var crc32c = Crc32C.Calculate(bocBuilder.Build().augment().toBytes());
            bocBuilder.storeUInt32LE(crc32c); // crc32c:has_crc32c?uint32
        }

        return bocBuilder.Build();
    }

    // private static void parseBoc(BitArray bocBits) {
    //
    // }

    // public static Cell[] deserializeBoc(string bocString) {
    //     return deserializeBoc(BitArrayExt.fromString(bocString));
    // }
    //
    // public static Cell[] deserializeBoc(BitArray bocBits) {
    //
    // }

    public static void Test() {
        var dc = new Cell("x{C_}",
            new Cell("x{0AAAAA}"),
            new Cell("x{FF_}",
                new Cell("x{0AAAAA}")));
        var dc2 = new Cell("x{F_}",
            new Cell("x{0AAAAA}"),
            new Cell("x{FF_}",
                new Cell("x{0AAAAA}"),new Cell("x{0AAAAA}"),new Cell("x{0AAAAA}")));
        var dc3 = new Cell("x{A_}",
            new Cell("x{0AAAAA}"),
            new Cell("x{FF_}",
                new Cell("x{0AAAAA}"),new Cell("x{0AAAAA}"),dc));

        // dc2.toFiftBits().print();
        // BitArrayExt.fromFiftHex("x{FF_}").toHexString().print();

        var (sortedCells, hashToIndex) = TopologicalSort(new[]{dc, dc2, dc3});

        // foreach (var (hash, cell) in sortedCells) {
        //     Console.WriteLine($"{hash.toHexString()} | {hashToIndex[hash]} | {cell.Bits.toFiftHex()}");
        // }
        // serializeBoc(new[]{ dc }, hasCrc32C:true, hasIdx:true).toFiftHex().print();

        // BitArrayExt.fromHexString("50d7f591").toBitString().print();
        // BitArrayExt.fromHexString("0DB08B5F").toBitString().print();
        // B5EE9C7201010301000E000200C002010101FF0200060AAAAA
        // b5ee9c7241010301000e000201c002010101ff0200060aaaaa
        // B5EE9C72010206030000200300010200F005040200A005020301FF0505030200C005040301FF05050500060AAAAA

        // BitArrayExt.fromHexString("4AB15039").toBitString().print();
        // B5EE9C7241010301000E000201C002010102FF0200060AAAAA945B8539
        // b5ee9c7241010301000e000201c002010102ff0200060aaaaa3950b14a
        // BitArrayExt.fromHexString("3950b14a").toBitString().print();
    }
}
