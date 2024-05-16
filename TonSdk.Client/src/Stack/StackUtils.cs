using System;
using System.Collections.Generic;
using System.Linq;
using TonSdk.Core.Boc;

namespace TonSdk.Client.Stack
{
    public class StackUtils
    {
        internal static string[][] PackInString(IStackItem[] stackItems)
        {
            var items = new List<string[]>();
            foreach (var stackItem in stackItems)
            {
                switch (stackItem)
                {
                    case VmStackCell cell:
                        items.Add(new [] { "tvm.Cell", cell.Value.ToString() });
                        break;
                    case VmStackInt value:
                        items.Add(new [] { "num", value.Value.ToString() });
                        break;
                    case VmStackTinyInt value:
                        items.Add(new [] { "num", value.Value.ToString() });
                        break;
                    case VmStackSlice slice:
                        items.Add(new [] { "tvm.Slice", slice.Value.RestoreRemainder().ToString() });
                        break;
                    case VmStackBuilder builder:
                        items.Add(new [] { "tvm.Cell", builder.Value.Build().ToString() });
                        break;
                }
            }
            return items.ToArray();
        }
        
        internal static StackJsonItem[] PackInStringV3(IStackItem[] stackItems)
        {
            var items = new List<StackJsonItem>();
            foreach (var stackItem in stackItems)
            {
                switch (stackItem)
                {
                    case VmStackCell cell:
                        items.Add(new StackJsonItem() { Type = "cell", Value = cell.Value.ToString() });
                        break;
                    case VmStackInt value:
                        items.Add(new StackJsonItem() { Type = "num", Value = value.Value.ToString() });
                        break;
                    case VmStackTinyInt value:
                        items.Add(new StackJsonItem() { Type = "num", Value = value.Value.ToString() });
                        break;
                    case VmStackSlice slice:
                        items.Add(new StackJsonItem() { Type = "slice", Value = slice.Value.RestoreRemainder().ToString() });
                        break;
                    case VmStackBuilder builder:
                        items.Add(new StackJsonItem() { Type = "cell", Value = builder.Value.Build().ToString() });
                        break;
                }
            }
            return items.ToArray();
        }
        
        public static Cell SerializeStack(IStackItem[] values)
        {
            // vm_stack#_ depth:(## 24) stack:(VmStackList depth) = VmStack;
            CellBuilder builder = new CellBuilder();
            builder.StoreUInt(values.Length, 24);
            SerializeStackList(builder, values);
            return builder.Build();
        }
        
        public static IStackItem[] DeserializeStack(string stack)
        {
            CellSlice slice = Cell.From(stack).Parse();
            uint depth = (uint)slice.LoadUInt(24);

            IStackItem[] stackList = new IStackItem[depth];
            
            for (int i = 0; i < depth; i++) {
                Cell rest = slice.LoadRef();
                IStackItem value = DeserializeStackValue(slice);

                stackList[depth - 1 - i] = value;
                
                slice = rest.Parse();
            }

            return stackList.ToArray();
        }

        private static void SerializeStackList(CellBuilder builder, IStackItem[] items)
        {
            if(items.Length == 0) return;
            
            // rest:^(VmStackList n)
            CellBuilder rest = new CellBuilder();
            SerializeStackList(rest, items.Take(items.Length - 1).ToArray());
            builder.StoreRef(rest.Build());

            SerializeStackValue(builder, items[items.Length - 1]);
        }

        private static void SerializeStackValue(CellBuilder builder, IStackItem stackItem)
        {
            switch (stackItem)
            {
                case VmStackNull vmStackNull:
                    builder.StoreUInt(0x00, 8);
                    break;
                case VmStackTinyInt item:
                    builder.StoreUInt(0x01, 8).StoreInt(item.Value, 64);
                    break;
                case VmStackInt item:
                    builder.StoreUInt(0x0100, 15).StoreInt(item.Value, 257);
                    break;
                case VmStackCell item:
                    builder.StoreUInt(0x03, 8).StoreRef(item.Value);
                    break;
                case VmStackSlice item:
                    builder
                        .StoreUInt(0x04, 8)
                        .StoreUInt(0, 10)
                        .StoreUInt(item.Value.Bits.Length, 10)
                        .StoreUInt(0, 3)
                        .StoreUInt(item.Value.Refs.Length, 3)
                        .StoreRef(new CellBuilder().StoreCellSlice(item.Value).Build());
                    break;
                case VmStackBuilder item:
                    builder.StoreUInt(0x05, 8).StoreRef(item.Value.Build());
                    break;
                case VmStackTuple item:
                    Cell? head = null;
                    Cell? tail = null;

                    for (int i = 0; i < item.Value.Length; i++)
                    {
                        (head, tail) = (tail, head);

                        if (i > 1)
                            head = new CellBuilder().StoreRef(tail!).StoreRef(head!).Build();

                        CellBuilder tailBuilder = new CellBuilder();
                        SerializeStackValue(tailBuilder, item.Value[i]);
                        tail = tailBuilder.Build();
                    }

                    builder.StoreUInt(0x07, 8).StoreUInt(item.Value.Length, 16);
                    if (head != null) builder.StoreRef(head);
                    if (tail != null) builder.StoreRef(tail);
                    break;
            }
        }

        private static IStackItem DeserializeStackValue(CellSlice slice)
        {
            uint type = (uint)slice.LoadUInt(8);

            switch (type)
            {
                // vm_stk_null#00 = VmStackValue;
                case 0x00: return new VmStackNull();
                // vm_stk_tinyint#01 value:int64 = VmStackValue;
                case 0x01: return new VmStackTinyInt(){ Value = (long)slice.LoadInt(64)};
                // vm_stk_int#0201_ value:int257 = VmStackValue;
                // vm_stk_nan#02ff = VmStackValue;    
                case 0x02:
                    if (slice.LoadUInt(7) != 0)
                    {
                        slice.LoadBit();
                        return new VmStackNull();
                    }
                    return new VmStackInt(){ Value = slice.LoadInt(257)};
                // vm_stk_cell#03 cell:^Cell = VmStackValue;
                case 0x03: return new VmStackCell() { Value = slice.LoadRef() };
                // _ cell:^Cell st_bits:(## 10) end_bits:(## 10) { st_bits <= end_bits }
                //   st_ref:(#<= 4) end_ref:(#<= 4) { st_ref <= end_ref } = VmCellSlice;
                // vm_stk_slice#04 _:VmCellSlice = VmStackValue;
                case 0x04: return new VmStackSlice() { Value = DeserializeStackValueSlice(slice) };
                // vm_stk_builder#05 cell:^Cell = VmStackValue;
                case 0x05:
                    Cell cell = slice.LoadRef();
                    return new VmStackBuilder() {Value = new CellBuilder().StoreCellSlice(new CellSlice(cell))};
                // vm_stk_cont#06 cont:VmCont = VmStackValue;
                case 0x06:
                    return new VmStackNull();
                // vm_stk_tuple#07 len:(## 16) data:(VmTuple len) = VmStackValue;
                case 0x07:
                    return  new VmStackTuple() {Value = DeserializeStackValueTuple(slice)};
                default: throw new Exception("Stack value type is not supported.");
            }
        }
        
        private static CellSlice DeserializeStackValueSlice(CellSlice slice) {
            Cell cell = slice.LoadRef();
            int startBits = (int)slice.LoadUInt(10);
            int endBits = (int)slice.LoadUInt(10);
            if (!(startBits <= endBits))
                throw new Exception($"Deserialization error: startBits {startBits}, endBits {endBits}");
            
            int startRefs = (int)slice.LoadUInt(3);
            int endRefs = (int)slice.LoadUInt(3);
            if (!(startRefs <= endRefs))
                throw new Exception($"Deserialization error: startRefs {startRefs}, endRefs {endRefs}");
            
            return new CellSlice(cell, startBits, endBits, startRefs, endRefs);
        }

        private static IStackItem[] DeserializeStackValueTuple(CellSlice slice)
        {
            // vm_tupref_nil$_ = VmTupleRef 0;
            // vm_tupref_single$_ entry:^VmStackValue = VmTupleRef 1;
            // vm_tupref_any$_ {n:#} ref:^(VmTuple (n + 2)) = VmTupleRef (n + 2);
            // vm_tuple_nil$_ = VmTuple 0;
            // vm_tuple_tcons$_ {n:#} head:(VmTupleRef n) tail:^VmStackValue = VmTuple (n + 1);
            // vm_stk_tuple#07 len:(## 16) data:(VmTuple len) = VmStackValue;

            uint length = (uint)slice.LoadUInt(16);
            IStackItem[] items = new IStackItem[length];

            if (length == 0) return Array.Empty<IStackItem>();
            
            if (length == 1) {
                Cell tailCell = slice.LoadRef();
                return new IStackItem[] { DeserializeStackValue(tailCell.Parse())};
            }

            CellSlice head = slice.LoadRef().Parse();
            CellSlice tail = slice.LoadRef().Parse();

            items[length - 1] = DeserializeStackValue(tail);
            
            for (int i = 0; i < length - 2; i++) {
                CellSlice current = head;
                head = current.LoadRef().Parse();
                tail = current.LoadRef().Parse();

                items[length - 2 - i] = DeserializeStackValue(tail);
            }

            items[0] = DeserializeStackValue(head);

            return items;
        }
    }
}