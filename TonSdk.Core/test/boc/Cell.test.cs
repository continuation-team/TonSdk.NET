using TonSdk.Core.Boc;

namespace TonSdk.Core.Tests.Boc;

public class CellTest
{
    [Test]
    public void SerializeTest()
    {
        // Arrange
        const int sampleValue = 0x32fab9;
        var builder = new CellBuilder();

        // Store the same 32-bit value 8 times (total: 256 bits)
        for (var i = 0; i < 8; i++)
        {
            builder.StoreUInt(sampleValue, 32);
        }

        // Expected BoC produced by @ton/core (JavaScript):
        // b.toBoc().toString("hex")
        const string expectedBoc =
            "b5ee9c724101010100220000400032fab90032fab90032fab90032fab90032fab90032fab90032fab90032fab979fc9ee0";

        // Act
        var cell = builder.Build();
        var boc = cell.ToString("hex").ToLowerInvariant();

        // Assert
        Assert.That(boc, Is.EqualTo(expectedBoc));
    }

    [Test]
    public void DeserializeTest()
    {
        // Arrange
        const string boc =
            "b5ee9c724101010100220000400032fab90032fab90032fab90032fab90032fab90032fab90032fab90032fab979fc9ee0";

        const int sampleValue = 0x32fab9;

        // Act
        var cell = Cell.From(new Bits(Convert.FromHexString(boc)));
        var slice = cell.Parse();

        // Assert
        // 8 values * 32 bits each = 256 bits remaining
        Assert.That(slice.RemainderBits, Is.EqualTo(32 * 8));

        for (var i = 0; i < 8; i++)
        {
            var value = (int)slice.LoadUInt(32);
            Assert.That(value, Is.EqualTo(sampleValue));
        }
    }
}