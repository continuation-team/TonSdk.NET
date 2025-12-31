using System;
using System.Collections.Generic;
using System.Linq;
using JustCRC32C;

namespace TonSdk.Core.Boc {
    public class BagOfCells {

        private const uint BOC_CONSTRUCTOR = 0xb5ee9c72;

        private struct BocHeader {
            public bool HasIdx;
            public bool HasCrc32C;
            public bool HasCacheBits;
            public byte Flags;
            public byte SizeBytes;
            public byte OffsetBytes;
            public uint CellsNum;
            public uint RootsNum;
            public uint AbsentNum;
            public ulong TotalCellsSize;
            public uint[] RootList;
            public Bits CellsData;
        }

        private struct RawCell {
            public Cell? Cell;
            public CellType Type;
            public CellBuilder Builder;
            public ulong[] Refs;
        }

        private static BocHeader deserializeHeader(Bits headerBits) {
            var hs = headerBits.Parse();
            if ((uint)hs.LoadUInt(32) != BOC_CONSTRUCTOR) {
                throw new Exception("Unknown BOC constructor");
            }

            var hasIdx = hs.LoadBit();
            var hasCrc32C = hs.LoadBit();
            var hasCacheBits = hs.LoadBit();
            var flags = (byte)hs.LoadUInt(2);
            if (flags != 0) throw new Exception("Unknown flags");
            var sizeBytes = (byte)hs.LoadUInt(3);
            if (sizeBytes > 4) throw new Exception("Invalid size");
            var offsetBytes = (byte)hs.LoadUInt(8);
            if (offsetBytes > 8) throw new Exception("Invalid offset");
            var cellsNum = (uint)hs.LoadUInt(sizeBytes * 8);
            var rootsNum = (uint)hs.LoadUInt(sizeBytes * 8);
            if (rootsNum < 1) throw new Exception("Invalid rootsNum");
            var absentNum = (uint)hs.LoadUInt(sizeBytes * 8);
            if (rootsNum + absentNum > cellsNum) throw new Exception("Invalid absentNum");
            var totalCellsSize = (ulong)hs.LoadUInt(offsetBytes * 8);

            var calcRemainderBits = (rootsNum * sizeBytes * 8)
                                    + (totalCellsSize * 8)
                                    + (hasIdx ? cellsNum * offsetBytes * 8 : 0)
                                    + (ulong)(hasCrc32C ? 32 : 0);

            if ((ulong)hs.RemainderBits != calcRemainderBits) {
                throw new Exception("Invalid BOC size");
            }

            var rootList = new uint[rootsNum];
            for (var i = 0; i < rootsNum; i++) {
                rootList[i] = (uint)hs.LoadUInt(sizeBytes * 8);
            }

            if (hasIdx) hs.SkipBits((int)(cellsNum * offsetBytes * 8));

            var cellsData = hs.LoadBits((int)(totalCellsSize * 8));

            if (hasCrc32C) {
                var crc32bits = headerBits.Slice(0, -32);
                //Console.WriteLine(hs.RemainderBits);
                var crc32c = (uint)hs.LoadUInt32LE();
                var crc32c_calc = Crc32C.Calculate(crc32bits.ToBytes());
                //Console.WriteLine(crc32c);
                //Console.WriteLine(crc32c_calc);
                if (crc32c != crc32c_calc) {
                    throw new Exception("Invalid CRC32C");
                }
            }

            return new BocHeader() {
                HasIdx = hasIdx,
                HasCrc32C = hasCrc32C,
                HasCacheBits = hasCacheBits,
                Flags = flags,
                SizeBytes = sizeBytes,
                OffsetBytes = offsetBytes,
                CellsNum = cellsNum,
                RootsNum = rootsNum,
                AbsentNum = absentNum,
                TotalCellsSize = totalCellsSize,
                RootList = rootList,
                CellsData = cellsData
            };
        }

        private static RawCell deserializeCell(BitsSlice dataSlice, ushort refIndexSize) {
            if (dataSlice.RemainderBits < 2) {
                throw new Exception("BOC not enough bytes to encode cell descriptors");
            }

            var refsDescriptor = (uint)dataSlice.LoadUInt(8);
            var level = refsDescriptor >> 5;
            var totalRefs = refsDescriptor & 7;
            var hasHashes = (refsDescriptor & 16) != 0;
            var isExotic = (refsDescriptor & 8) != 0;
            var isAbsent = totalRefs == 7 && hasHashes;

            if (isAbsent) {
                throw new Exception("BoC can't deserialize absent cell");
            }

            if (totalRefs > 4) {
                throw new Exception($"BoC cell can't has more than 4 refs {totalRefs}");
            }

            var bitsDescriptor = (uint)dataSlice.LoadUInt(8);
            var isAugmented = (bitsDescriptor & 1) != 0;
            var dataSize = (bitsDescriptor >> 1) + (isAugmented ? 1 : 0);
            var hashesSize = hasHashes ? (level + 1) * 32 : 0;
            var depthSize = hasHashes ? (level + 1) * 2 : 0;

            if (dataSlice.RemainderBits < hashesSize + depthSize + dataSize + refIndexSize * totalRefs) {
                throw new Exception("BoC not enough bytes to encode cell data");
            }

            if (hasHashes) dataSlice.SkipBits((int)(hashesSize + depthSize));

            var data = isAugmented
                ? dataSlice.LoadBits((int)dataSize * 8).Rollback(8)
                : dataSlice.LoadBits((int)dataSize * 8);

            if (isExotic && data.Length < 8) {
                throw new Exception("BoC not enough bytes for an exotic cell type");
            }

            var type = isExotic ? (CellType)(int)data.Slice(0, 8).Parse().LoadInt(8) : CellType.ORDINARY;
            
            if (isExotic && type == CellType.ORDINARY)
                throw new Exception("BoC an exotic cell can't be of ordinary type");

            var refs = new ulong[totalRefs];
            for (var i = 0; i < totalRefs; i++) {
                refs[i] = (ulong)dataSlice.LoadUInt(refIndexSize * 8);
            }

            return new RawCell() {
                Type = type,
                Builder = new CellBuilder(data.Length).StoreBits(data),
                Refs = refs
            };
        }

        public static Cell[] DeserializeBoc(Bits data) {
            var headers = deserializeHeader(data);
            var rawCells = new RawCell[headers.CellsNum];

            var cellsDataSlice = headers.CellsData.Parse();

            for (var i = 0; i < headers.CellsNum; i++) {
                rawCells[i] = deserializeCell(cellsDataSlice, headers.SizeBytes);
            }

            for (var i = (int)(headers.CellsNum - 1); i >= 0; i--) {
                foreach (var refIndex in rawCells[i].Refs) {
                    var rawRefCell = rawCells[refIndex];
                    if (refIndex < (ulong)i) {
                        throw new Exception("Topological order is broken");
                    }

                    rawCells[i].Builder.StoreRef(rawRefCell.Builder.Build());
                }

                rawCells[i].Cell = rawCells[i].Builder.Build();
            }

            return headers.RootList.Select(i => rawCells[i].Cell!).ToArray();
        }



        private static Bits serializeCell(Cell cell, Dictionary<Bits, int> cellsIndex, int refSize) {
            var ret = cell.BitsWithDescriptors;
            refSize *= 8;
            var l = ret.Length + cell.RefsCount * refSize;
            var b = new BitsBuilder(l).StoreBits(ret);
            foreach (var refCell in cell.Refs) {
                var refHash = refCell.Hash;
                var refIndex = cellsIndex[refHash];
                b.StoreUInt(refIndex, refSize);
            }
            return b.Build();
        }


        public static Bits SerializeBoc(
            Cell root,
            bool hasIdx = false,
            bool hasCrc32C = true
        ) {
            return SerializeBoc(new[] { root }, hasIdx, hasCrc32C);
        }


        private static (List<(Bits, Cell)> sortedCells, Dictionary<Bits, int> hashToIndex)
            TopologicalSort(Cell[] roots) {

            // List of already sorted vertices of the graph
            var sortedCells = new List<(Bits, Cell)>();
            // Dictionary that maps the cell hash to its index in the sorted list
            var hashToIndex = new Dictionary<Bits, int>(new BitsEqualityComparer());

            // Recursive function for graph traversal and topological sorting
            void VisitCell(Cell cell) {
                foreach (var neighbor in cell.Refs) {
                    if (!hashToIndex.ContainsKey(neighbor.Hash)) {
                        VisitCell(neighbor);
                    }
                }

                // Check that the cell is not yet added to the list of sorted cells
                if (!hashToIndex.ContainsKey(cell.Hash)) {
                    // Add the cell to the beginning of the list of sorted cells
                    sortedCells.Insert(0, (cell.Hash, cell));
                    // Shift the already added cells one position to the right
                    for (var i = 1; i < sortedCells.Count; i++) {
                        hashToIndex[sortedCells[i].Item2.Hash]++;
                    }

                    // Add the cell to the hashToIndex dictionary
                    hashToIndex[cell.Hash] = 0;
                }
            }

            // Perform traversal and topological sorting for each vertex of the graph
            for (var i = roots.Length - 1; i > -1; i--) {
                // foreach (var rootCell in roots) {
                var rootCell = roots[i];
                foreach (var cell in rootCell.Refs) {
                    VisitCell(cell);
                }

                VisitCell(rootCell);
            }

            return (sortedCells, hashToIndex);
        }



        public static Bits SerializeBoc(
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
                dataBuilder.StoreBits(serializedCell);
                totalSize += serializedCell.Length / 8;
                offsets[i] = totalSize;
            }

            var dataBits = dataBuilder.Build();
            var dataBytesSize = Math.Max((dataBits.Length + 7) / 8, 1) ; 
            var offsetBytes = Math.Max((dataBytesSize.bitLength() + 7) / 8, 1);
            
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
                .StoreUInt(BOC_CONSTRUCTOR, 32, false) // serialized_boc#b5ee9c72
                .StoreBit(hasIdx, false) // has_idx:(## 1)
                .StoreBit(hasCrc32C, false) // has_crc32c:(## 1)
                .StoreBit(hasCacheBits, false) // has_cache_bits:(## 1)
                .StoreUInt(flags, 2, false) // flags:(## 2) { flags = 0 }
                .StoreUInt(sBytes, 3, false) // size:(## 3) { size <= 4 }
                .StoreUInt(offsetBytes, 8, false) // off_bytes:(## 8) { off_bytes <= 8 }
                .StoreUInt(cellsNum, sBytes * 8, false) // cells:(##(size * 8))
                .StoreUInt(roots.Length, sBytes * 8) // roots:(##(size * 8)) { roots >= 1 }
                .StoreUInt(0, sBytes * 8, false) // ??? absent:(##(size * 8)) { roots + absent <= cells }
                .StoreUInt(dataBits.Length / 8, offsetBytes * 8, false); // tot_cells_size:(##(off_bytes * 8))

            foreach (var _rootCell in roots) {
                bocBuilder.StoreUInt(indexHashmap[_rootCell.Hash], sBytes * 8,
                    false); // root_list:(roots * ##(size * 8))
            }

            if (hasIdx) {
                foreach (var offset in offsets) {
                    bocBuilder.StoreUInt(offset, offsetBytes * 8, false); // index:has_idx?(cells * ##(off_bytes * 8))
                }
            }

            bocBuilder.StoreBits(dataBits, false); // cell_data:(tot_cells_size * [ uint8 ])

            if (hasCrc32C) {
                var crc32c = Crc32C.Calculate(bocBuilder.Build().Augment().ToBytes());
                bocBuilder.StoreUInt32LE(crc32c); // crc32c:has_crc32c?uint32
            }

            return bocBuilder.Build();
        }
    }
}
