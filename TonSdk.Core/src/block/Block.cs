using System;
using System.Collections.Generic;
using System.Numerics;
using TonSdk.Core.Boc;
using TonSdk.Core.Crypto;

namespace TonSdk.Core.Block 
{
    public static class BlockUtils {
        public static void CheckUnderflow(CellSlice slice, int? needBits, int? needRefs) {
            if (needBits.HasValue && needBits < slice.RemainderBits) {
                throw new ArgumentException("Bits underflow");
            }

            if (needRefs.HasValue && needRefs > slice.RemainderRefs) {
                throw new ArgumentException("Refs underflow");
            }
        }
    }

    public abstract class BlockStruct<T> {
        protected T _data;
        protected Cell _cell;

        public T Data {
            get { return _data; }
        }

        public Cell Cell {
            get { return _cell; }
        }
    }


    public struct TickTockOptions {
        public bool Tick;
        public bool Tock;
    }

    public class TickTock : BlockStruct<TickTockOptions> {
        public TickTock(TickTockOptions opt) {
            _data = opt;
            _cell = new CellBuilder()
                .StoreBit(opt.Tick)
                .StoreBit(opt.Tock)
                .Build();
        }

        public static TickTock Parse(CellSlice slice) {
            BlockUtils.CheckUnderflow(slice, 2, null);
            return new TickTock(new TickTockOptions {
                Tick = slice.LoadBit(),
                Tock = slice.LoadBit()
            });
        }
    }

    public struct SimpleLibOptions {
        public bool Public;
        public Cell Root;
    }

    public class SimpleLib : BlockStruct<SimpleLibOptions> {
        public SimpleLib(SimpleLibOptions opt) {
            _data = opt;
            _cell = new CellBuilder()
                .StoreBit(opt.Public)
                .StoreRef(opt.Root)
                .Build();
        }

        public static SimpleLib Parse(CellSlice slice) {
            BlockUtils.CheckUnderflow(slice, 1, 1);
            return new SimpleLib(new SimpleLibOptions {
                Public = slice.LoadBit(),
                Root = slice.LoadRef()
            });
        }
    }

    public struct StateInitOptions {
        public byte? SplitDepth;
        public TickTock? Special;
        public Cell? Code;
        public Cell? Data;
        public HashmapE<BigInteger, SimpleLib>? Library;
    }

    public class StateInit : BlockStruct<StateInitOptions> {
        public StateInit(StateInitOptions opt) {
            if (opt.SplitDepth.HasValue && opt.SplitDepth.Value > 31) {
                throw new ArgumentException("Invalid split depth. Can be 0..31. TLB: `split_depth:(Maybe (## 5))`");
            }


            _data = opt;
            var builder = new CellBuilder();

            if (opt.SplitDepth.HasValue) {
                builder
                    .StoreBit(true)
                    .StoreUInt((uint)opt.SplitDepth, 5);
            }
            else {
                builder
                    .StoreBit(false);
            }

            if (opt.Special != null) {
                builder
                    .StoreBit(true)
                    .StoreCellSlice(opt.Special.Cell.Parse());
            }
            else {
                builder
                    .StoreBit(false);
            }

            var lib = opt.Library != null
                ? opt.Library.Build()
                : new CellBuilder().StoreBit(false).Build();
            builder
                .StoreOptRef(opt.Code)
                .StoreOptRef(opt.Data)
                .StoreCellSlice(lib.Parse());
            _cell = builder.Build();
        }

        public static StateInit Parse(CellSlice slice) {
            var _slice = slice.Clone();

            var maybeSplitDepth = _slice.LoadBit();
            byte? splitDepth = maybeSplitDepth ? (byte?)_slice.LoadUInt(5) : null;

            var maybeSpecial = _slice.LoadBit();

            TickTock? special = maybeSpecial ? TickTock.Parse(_slice) : null;

            var code = _slice.LoadOptRef();
            var data = _slice.LoadOptRef();
            var library = _slice.LoadDict(new HashmapOptions<BigInteger, SimpleLib>() {
                KeySize = 256,
                Deserializers = new HashmapDeserializers<BigInteger, SimpleLib>() {
                    Key = (kBits) => kBits.Parse().LoadUInt(256),
                    Value = (vCell) => SimpleLib.Parse(vCell.Parse())
                }
            });

            slice.SkipBits(slice.RemainderBits - _slice.RemainderBits);
            slice.SkipRefs(slice.RemainderRefs - _slice.RemainderRefs);

            return new StateInit(new StateInitOptions {
                SplitDepth = splitDepth,
                Special = special,
                Code = code,
                Data = data,
                Library = library
            });
        }
    }


    public class CommonMsgInfo : BlockStruct<object> {
        public static CommonMsgInfo Parse(CellSlice slice) {
            var _slice = slice.Clone();
            if (!_slice.LoadBit()) return IntMsgInfo.Parse(slice, true);
            if (!_slice.LoadBit()) return ExtInMsgInfo.Parse(slice, true);
            return ExtOutMsgInfo.Parse(slice, true);
            //throw new NotImplementedException("ExtOutMsgInfo is not implemented yet");
        }
    }

    public struct IntMsgInfoOptions {
        public bool? IhrDisabled;
        public bool Bounce;
        public bool? Bounced;
        public Address? Src;
        public Address? Dest;
        public Coins Value;
        public Coins? IhrFee;
        public Coins? FwdFee;
        public ulong? CreatedLt;
        public uint? CreatedAt;
    }

    public class IntMsgInfo : CommonMsgInfo {
        public IntMsgInfo(IntMsgInfoOptions opt) {
            _data = opt;
            _cell = new CellBuilder()
                .StoreBit(false) // int_msg_info$0
                .StoreBit(opt.IhrDisabled ?? true)
                .StoreBit(opt.Bounce)
                .StoreBit(opt.Bounced ?? false)
                .StoreAddress(opt.Src)
                .StoreAddress(opt.Dest)
                .StoreCoins(opt.Value)
                .StoreBit(false) // TODO: implement extracurrency collection
                .StoreCoins(opt.IhrFee ?? new Coins(0))
                .StoreCoins(opt.FwdFee ?? new Coins(0))
                .StoreUInt(opt.CreatedLt ?? 0, 64)
                .StoreUInt(opt.CreatedAt ?? 0, 32)
                .Build();
        }

        public static IntMsgInfo Parse(CellSlice slice, bool skipPrefix = false) {
            var _slice = slice.Clone();
            if (!skipPrefix) {
                var prefix = _slice.LoadBit();
                if (prefix) throw new ArgumentException("Invalid IntMsgInfo prefix. TLB: `int_msg_info$0`");
            }
            else {
                _slice.SkipBit();
            }

            var ihrDisabled = _slice.LoadBit();
            var bounce = _slice.LoadBit();
            var bounced = _slice.LoadBit();
            var src = _slice.LoadAddress();
            var dest = _slice.LoadAddress();
            if (dest == null) throw new Exception("Invalid dest address s");
            var value = _slice.LoadCoins();
            _slice.SkipOptRef();
            var ihrFee = _slice.LoadCoins();
            var fwdFee = _slice.LoadCoins();
            var createdLt = (ulong)_slice.LoadUInt(64);
            var createdAt = (uint)_slice.LoadUInt(32);

            slice.SkipBits(slice.RemainderBits - _slice.RemainderBits);
            slice.SkipRefs(slice.RemainderRefs - _slice.RemainderRefs);

            return new IntMsgInfo(new IntMsgInfoOptions {
                IhrDisabled = ihrDisabled,
                Bounce = bounce,
                Bounced = bounced,
                Src = src,
                Dest = dest,
                Value = value,
                IhrFee = ihrFee,
                FwdFee = fwdFee,
                CreatedLt = createdLt,
                CreatedAt = createdAt
            });
        }
    }

    public struct ExtInMsgInfoOptions {
        public Address? Src;
        public Address? Dest;
        public Coins? ImportFee;
    }
    

    public class ExtInMsgInfo : CommonMsgInfo {
        public ExtInMsgInfo(ExtInMsgInfoOptions opt) {
            _data = opt;
            _cell = new CellBuilder()
                .StoreBit(true).StoreBit(false) // ext_in_msg_info$10
                .StoreAddress(opt.Src)
                .StoreAddress(opt.Dest)
                .StoreCoins(opt.ImportFee ?? new Coins(0))
                .Build();
        }

        public static ExtInMsgInfo Parse(CellSlice slice, bool skipPrefix = false) {
            var _slice = slice.Clone();
            if (!skipPrefix) {
                var prefix = (byte)_slice.LoadInt(2);
                if (prefix != 0b10)
                    throw new ArgumentException("Invalid ExtInMsgInfo prefix. TLB: `ext_in_msg_info$10`");
            }
            else {
                _slice.SkipBits(2);
            }

            var src = _slice.LoadAddress();
            var dest = _slice.LoadAddress();
            Coins importFee;
            try
            {
                importFee = _slice.LoadCoins();
            }
            catch
            {
                importFee = new Coins(0);
            }

            slice.SkipBits(slice.RemainderBits - _slice.RemainderBits);
            slice.SkipRefs(slice.RemainderRefs - _slice.RemainderRefs);

            return new ExtInMsgInfo(new ExtInMsgInfoOptions {
                Src = src,
                Dest = dest,
                ImportFee = importFee
            });
        }
    };
    
    public struct ExtOutMsgInfoOptions {
        public Address Src;
        public Address? Dest;
        public ulong CreatedLt;
        public uint CreatedAt;
    }
    
    public class ExtOutMsgInfo : CommonMsgInfo {
        public ExtOutMsgInfo(ExtOutMsgInfoOptions opt) {
            _data = opt;
            _cell = new CellBuilder()
                .StoreBit(true).StoreBit(false) // ext_out_msg_info$11
                .StoreAddress(opt.Src)
                .StoreAddress(opt.Dest)
                .StoreUInt(opt.CreatedLt, 64)
                .StoreUInt(opt.CreatedAt, 32)
                .Build();
        }

        public static ExtOutMsgInfo Parse(CellSlice slice, bool skipPrefix = false) {
            var _slice = slice.Clone();
            if (!skipPrefix) {
                var prefix = (byte)_slice.LoadInt(2);
                if (prefix != 0b11)
                    throw new ArgumentException("Invalid ExtOutMsgInfo prefix. TLB: `ext_out_msg_info$11`");
            }
            else {
                _slice.SkipBits(2);
            }

            var src = _slice.LoadAddress();
            //if (src == null) throw new Exception("Invalid src address");
            var dest = _slice.LoadAddress();
            if (dest == null) 
                return new ExtOutMsgInfo(new ExtOutMsgInfoOptions() {
                Src = src,
                Dest = null,
                CreatedLt = 0,
                CreatedAt = 0
            });
            ulong createdLt = (ulong)_slice.LoadUInt(64);
            uint createdAt = (uint)_slice.LoadUInt(32);

            slice.SkipBits(slice.RemainderBits - _slice.RemainderBits);
            slice.SkipRefs(slice.RemainderRefs - _slice.RemainderRefs);

            return new ExtOutMsgInfo(new ExtOutMsgInfoOptions() {
                Src = src,
                Dest = dest,
                CreatedLt = createdLt,
                CreatedAt = createdAt
            });
        }
    };

    public struct MessageXOptions {
        public CommonMsgInfo Info;
        public StateInit? StateInit;
        public Cell? Body;
    }

    public class MessageX : BlockStruct<MessageXOptions> {

        private bool _signed;

        private Cell _signedCell;

        public bool Signed {
            get => _signed;
        }

        public Cell SignedCell {
            get => _signedCell;
        }

        public MessageX(MessageXOptions opt) {
            _data = opt;
            _cell = buildCell();
            _signed = false;
        }
        
        private Cell signCell(byte[]? privateKey = null, bool eitherSliceRef = false)
        {
            var builder = new CellBuilder();
            var body = KeyPair.Sign(_data.Body, privateKey);
            builder.StoreBytes(body);
            builder.StoreCellSlice(_data.Body.Parse());
            return builder.Build();
        }

        private Cell buildCell(byte[]? privateKey = null, bool eitherSliceRef = false) {
            var builder = new CellBuilder()
                .StoreCellSlice(_data.Info.Cell.Parse());
            var maybeStateInit = _data.StateInit != null;
            if (maybeStateInit) {
                builder.StoreBit(true);
                builder.StoreBit(false); // Either StateInit ^StateInit
                builder.StoreCellSlice(_data.StateInit!.Cell.Parse());
            }
            else {
                builder.StoreBit(false);
            }

            if (_data.Body != null) {
                var body = privateKey != null
                    ? signBody(privateKey, eitherSliceRef)
                    : _data.Body!;
                var eitherBody = _data.Body.BitsCount > builder.RemainderBits
                                 || _data.Body.RefsCount > builder.RemainderRefs;
                builder.StoreBit(eitherBody);
                if (!eitherBody) {
                    try
                    {
                        builder.StoreCellSlice(body.Parse());
                    }
                    catch (Exception e)
                    {
                        builder.StoreRef(body);
                    }
                }
                else {
                    builder.StoreRef(body);
                }
            }
            else {
                builder.StoreBit(false);
            }

            return builder.Build();
        }

        private Cell signBody(byte[] privateKey, bool eitherSliceRef) {
            var b = new CellBuilder()
                .StoreBytes(KeyPair.Sign(_data.Body, privateKey));
            if (!eitherSliceRef) {
                b.StoreCellSlice(_data.Body.Parse());
            }
            else {
                b.StoreRef(_data.Body);
            }

            return b.Build();
        }

        public MessageX Sign(byte[] privateKey, bool eitherSliceRef = false) {
            if (_data.Body == null) throw new Exception("MessageX body is empty");
            if (_signed) throw new Exception("MessageX already signed");
            _cell = buildCell(privateKey, eitherSliceRef);
            _signedCell = signCell(privateKey, eitherSliceRef);
            _signed = true;
            return this;
        }

        public static MessageX Parse(CellSlice slice) {
            var _slice = slice.Clone();
            var info = CommonMsgInfo.Parse(_slice);
            var maybeStateInit = _slice.LoadBit();
            var eitherStateInit = maybeStateInit && _slice.LoadBit();
            StateInit stateInit;
            try
            {
                stateInit = maybeStateInit
                    ? eitherStateInit
                        ? StateInit.Parse(_slice.LoadRef().Parse())
                        : StateInit.Parse(_slice)
                    : null;
            }
            catch (Exception e)
            {
                stateInit = null;
            }
            var eitherBody = _slice.LoadBit();
            var body = eitherBody
                ? _slice.RemainderRefs > 0 ? _slice.LoadRef() 
                    : _slice.RestoreRemainder() 
                : _slice.RestoreRemainder();

            slice.SkipBits(slice.RemainderBits - _slice.RemainderBits);
            slice.SkipRefs(slice.RemainderRefs - _slice.RemainderRefs);

            return new MessageX(new MessageXOptions {
                Info = info,
                StateInit = stateInit,
                Body = body
            });
        }
    }

    public struct ExternalInMessageOptions {
        public ExtInMsgInfo Info;
        public StateInit? StateInit;
        public Cell? Body;
    }

    public class ExternalInMessage : MessageX {
        public ExternalInMessage(ExternalInMessageOptions opt)
            : base(new MessageXOptions { Info = opt.Info, Body = opt.Body, StateInit = opt.StateInit }) { }

        public ExternalInMessage Sign(byte[] privateKey, bool eitherSliceRef = false) {
            return (ExternalInMessage)base.Sign(privateKey, eitherSliceRef);
        }
    }

    public struct InternalMessageOptions {
        public IntMsgInfo Info;
        public StateInit? StateInit;
        public Cell? Body;
    }

    public class InternalMessage : MessageX {
        public InternalMessage(InternalMessageOptions opt)
            : base(new MessageXOptions { Info = opt.Info, Body = opt.Body, StateInit = opt.StateInit }) { }

        public InternalMessage Sign(byte[] privateKey, bool eitherSliceRef = false) {
            return (InternalMessage)base.Sign(privateKey, eitherSliceRef);
        }
    }

    public class OutAction : BlockStruct<object> {
        public static OutAction Parse(CellSlice slice) {
            var prefix = (uint)slice.ReadUInt(32);
            return prefix switch {
                0x0ec3c86d => ActionSendMsg.Parse(slice, true),
                0xad4de08e => ActionSetCode.Parse(slice, true),
                0x36e6b809 => throw new NotImplementedException("ActionReserveCurrency"),
                0x26fa1dd4 => throw new NotImplementedException("ActionChangeLibrary"),
                _ => throw new ArgumentException("Invalid action prefix")
            };
        }
    }

    public struct ActionSendMsgOptions {
        public byte Mode;
        public MessageX OutMsg;
    }

    public class ActionSendMsg : OutAction {
        public ActionSendMsg(ActionSendMsgOptions opt) {
            _data = opt;
            _cell = new CellBuilder()
                .StoreUInt(0x0ec3c86d, 32)
                .StoreUInt(opt.Mode, 8)
                .StoreRef(opt.OutMsg.Cell)
                .Build();
        }

        public static ActionSendMsg Parse(CellSlice slice, bool skipPrefix = false) {
            BlockUtils.CheckUnderflow(slice, 40, 1);
            if (!skipPrefix) {
                var prefix = slice.LoadUInt(32);
                if (prefix != 0x0ec3c86d) throw new ArgumentException("Invalid action prefix");
            }
            else {
                slice.SkipBits(32);
            }

            return new ActionSendMsg(new ActionSendMsgOptions {
                Mode = (byte)slice.LoadUInt(8),
                OutMsg = MessageX.Parse(slice.LoadRef().Parse())
            });
        }
    }

    public struct ActionSetCodeOptions {
        public Cell NewCode;
    }

    public class ActionSetCode : OutAction {
        public ActionSetCode(ActionSetCodeOptions opt) {
            _data = opt;
            _cell = new CellBuilder()
                .StoreUInt(0xad4de08e, 32)
                .StoreRef(opt.NewCode)
                .Build();
        }

        public static ActionSetCode Parse(CellSlice slice, bool skipPrefix = false) {
            BlockUtils.CheckUnderflow(slice, 32, 1);
            if (!skipPrefix) {
                var prefix = slice.LoadUInt(32);
                if (prefix != 0xad4de08e) throw new ArgumentException("Invalid action prefix");
            }
            else {
                slice.SkipBits(32);
            }

            return new ActionSetCode(new ActionSetCodeOptions { NewCode = slice.LoadRef() });
        }
    }

    public struct OutListOptions {
        public OutAction[] Actions;
    }


    public class OutList : BlockStruct<OutListOptions> {
        public OutList(OutListOptions opt) {
            if (opt.Actions.Length > 255)
                throw new ArgumentException("Too many actions. May be from 0 to 255 (includes)");
            _data = opt;
            _cell = buildCell();
        }

        /*
        out_list_empty$_ = OutList 0;
        out_list$_ {n:#} prev:^(OutList n) action:OutAction
          = OutList (n + 1);
        */
        private Cell buildCell() {
            Cell actionList = new Cell(new Bits(0), Array.Empty<Cell>());

            foreach (var action in _data.Actions) {
                actionList = new CellBuilder()
                    .StoreRef(actionList)
                    .StoreCellSlice(action.Cell.Parse())
                    .Build();
            }

            return actionList;
        }

        public OutList Parse(CellSlice slice) {
            var _slice = slice.Clone();
            var actions = new List<OutAction>();
            while (_slice.RemainderRefs > 0) {
                var prev = _slice.LoadRef();
                actions.Add(OutAction.Parse(_slice));
                _slice = prev.Parse();
            }

            return new OutList(new OutListOptions { Actions = actions.ToArray() });
        }
    }
}
