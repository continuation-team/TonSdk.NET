using TonSdk.Core.Boc;

namespace TonSdk.Core.Tests;


public class BitsTest {
    [Test]
    public void StringTest() {
        Assert.AreEqual("1111111", new Bits("x{FF_}").ToString("bin"));
        Assert.AreEqual("b{101010111100110111101111}", new Bits("ABCDEF").ToString("fiftBin"));
        Assert.AreEqual("A9F3C_", new Bits("b{10101001111100111}").ToString("hex"));
        Assert.AreEqual("x{D5ADE}", new Bits("11010101101011011110").ToString("fiftHex"));
    }
}
