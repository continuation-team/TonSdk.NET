using TonSdk.Core;
using TonSdk.Core.Boc;

namespace TonSdk.Contracts.Jetton {
    public static class JettonOperation {
        public const uint TRANSFER = 0xf8a7ea5;
        public const uint TRANSFER_NOTIFICATION = 0x7362d09c;
        public const uint INTERNAL_TRANSFER = 0x178d4519;
        public const uint EXCESSES = 0xd53276db;
        public const uint BURN = 0x595f07bc;
        public const uint BURN_NOTIFICATION = 0x7bdd97de;
    }

    public struct JettonTransferOptions {
        public ulong? QueryId;
        public Coins Amount;
        public Address Destination;
        public Address? ResponseDestination;
        public Coins? ForwardAmount;
        public Cell? ForwardPayload;
        public Cell? CustomPayload; // custom_payload:(Maybe ^Cell) << always null
    }

    public struct JettonBurnOptions {
        public ulong? QueryId;
        public Coins Amount;
        public Address? ResponseDestination;
        public Cell? CustomPayload; // custom_payload:(Maybe ^Cell) << always null
    }


    public static class JettonWallet {
        public static Cell CreateTransferRequest(JettonTransferOptions opt) {
            /*
                 transfer#f8a7ea5 query_id:uint64 amount:(VarUInteger 16) destination:MsgAddress
                 response_destination:MsgAddress custom_payload:(Maybe ^Cell)
                 forward_ton_amount:(VarUInteger 16) forward_payload:(Either Cell ^Cell)
                 = InternalMsgBody;
             */

            var builder = new CellBuilder()
                .StoreUInt(JettonOperation.TRANSFER, 32)
                .StoreUInt(opt.QueryId ?? SmcUtils.GenerateQueryId(60), 64)
                .StoreCoins(opt.Amount)
                .StoreAddress(opt.Destination)
                .StoreAddress(opt.ResponseDestination)
                .StoreOptRef(opt.CustomPayload)
                .StoreCoins(opt.ForwardAmount ?? new Coins(0));

            bool isForwardPayloadNull = opt.ForwardPayload == null;
            bool isBitsCountExceeded = opt.ForwardPayload?.BitsCount > builder.RemainderBits;
            bool isRefsCountExceeded = opt.ForwardPayload?.RefsCount > builder.RemainderRefs;

            if (isForwardPayloadNull || !(isBitsCountExceeded || isRefsCountExceeded)) {
                builder.StoreBit(false);
                if (!isForwardPayloadNull) builder.StoreCellSlice(opt.ForwardPayload!.Parse());
            } else {
                builder.StoreBit(true).StoreRef(opt.ForwardPayload!);
            }


            return builder.Build();
        }

        public static Cell CreateBurnRequest(JettonBurnOptions opt) {
            /*
                burn#595f07bc query_id:uint64 amount:(VarUInteger 16)
                response_destination:MsgAddress custom_payload:(Maybe ^Cell)
                = InternalMsgBody;
            */

            return new CellBuilder()
                .StoreUInt(JettonOperation.BURN, 32)
                .StoreUInt(opt.QueryId ?? SmcUtils.GenerateQueryId(60), 64)
                .StoreCoins(opt.Amount)
                .StoreAddress(opt.ResponseDestination)
                .StoreOptRef(opt.CustomPayload)
                .Build();
        }
    }
}
