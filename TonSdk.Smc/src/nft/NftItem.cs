using TonSdk.Core;
using TonSdk.Core.Boc;

namespace TonSdk.Contracts.nft;

public static class NftOperations {
    public const uint TRANSFER = 0x5fcc3d14;
}

public struct NftTransferOptions {
    public ulong? QueryId;
    public Address NewOwner;
    public Address? ResponseDestination;
    public Coins? ForwardAmount;
    public Cell? ForwardPayload;
    public Cell? CustomPayload; // custom_payload:(Maybe ^Cell) << always null
}

public static class NftItem {
    public static Cell CreateTransferRequest(NftTransferOptions opt) {
        /*
            transfer#5fcc3d14 query_id:uint64 new_owner:MsgAddress response_destination:MsgAddress
            custom_payload:(Maybe ^Cell) forward_amount:(VarUInteger 16) forward_payload:(Either Cell ^Cell)
            = InternalMsgBody;
        */

        var builder = new CellBuilder()
            .StoreUInt(NftOperations.TRANSFER, 32)
            .StoreUInt(opt.QueryId ?? SmcUtils.GenerateQueryId(60), 64)
            .StoreAddress(opt.NewOwner)
            .StoreAddress(opt.ResponseDestination)
            .StoreOptRef(opt.CustomPayload)
            .StoreCoins(opt.ForwardAmount ?? new Coins(0));

        bool isForwardPayloadNull = opt.ForwardPayload == null;
        bool isBitsCountExceeded = opt.ForwardPayload?.BitsCount > builder.RemainderBits;
        bool isRefsCountExceeded = opt.ForwardPayload?.RefsCount > builder.RemainderRefs;

        if (isForwardPayloadNull || (isBitsCountExceeded || isRefsCountExceeded)) {
            builder.StoreBit(false);
            if (!isForwardPayloadNull) builder.StoreCellSlice(opt.ForwardPayload!.Parse());
        } else {
            builder.StoreBit(true).StoreRef(opt.ForwardPayload!);
        }

        return builder.Build();
    }

}
