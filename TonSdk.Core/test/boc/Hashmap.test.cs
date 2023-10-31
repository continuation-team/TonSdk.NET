using TonSdk.Core.Boc;

namespace TonSdk.Core.Tests;

public class HashmapTest
{
    [Test]
    public void ShouldSerializeDictionary()
    {
        var hmOptions = new HashmapOptions<int, int>()
        {
            KeySize = 16,
            Serializers = new HashmapSerializers<int, int>
            {
                Key = k => new BitsBuilder(16).StoreInt(k, 16).Build(),
                Value = v => new CellBuilder().StoreUInt(v, 32).Build()
            },
            Deserializers = null
        };

        var hm = new HashmapE<int, int>(hmOptions);

        for (int i = 1; i < 100; i++)
        {
            hm.Set(i, Random.Shared.Next(1, 50000));
        }

        Assert.DoesNotThrow(() => hm.Serialize());
    }
}
