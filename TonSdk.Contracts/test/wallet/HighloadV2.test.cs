using NUnit.Framework;
using TonSdk.Contracts.Wallet;
using TonSdk.Core;
using TonSdk.Core.Block;
using TonSdk.Core.Boc;

namespace TonSdk.Contracts.Tests.wallet;

public class HighloadV2_test
{
    private const string dest_address = "kQCBvjU7mYLJQCIEtJGOiUWxmW0NI1Gn-1zzyTJ5zRBtLjlf";

    [Test]
    public void Test_ShouldSerializeManyTransferMessages()
    {
        var hmOptions = new HashmapOptions<int, WalletTransfer>()
        {
            KeySize = 16,
            Serializers = new HashmapSerializers<int, WalletTransfer>
            {
                Key = k => new BitsBuilder(16).StoreInt(k, 16).Build(),
                Value = v => new CellBuilder().StoreUInt(v.Mode, 8).StoreRef(v.Message.Cell).Build()
            },
            Deserializers = null
        };
        
        var hm = new HashmapE<int, WalletTransfer>(hmOptions);

        for (int i = 1; i < 100; i++)
        {
            hm.Set(i, CreateTestTransferMessage());
        }

        Assert.DoesNotThrow(() => hm.Serialize());
    }


    private WalletTransfer CreateTestTransferMessage()
    {
        return new WalletTransfer
        {
            Message = new InternalMessage(new()
            {
                Info = new IntMsgInfo(new()
                {
                    Dest = new Address(dest_address),
                    Value = Coins.FromNano(Random.Shared.Next(10000000, 1000000000)),
                    Bounce = false
                }),
            }),
            Mode = 1
        };
    }
}