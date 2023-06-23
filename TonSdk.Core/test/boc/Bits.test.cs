
using System.Numerics;
using TonSdk.Core.Boc;

namespace TonSdk.Core.Tests;


public class BitsTest {
    [Test]
    public void FT_StringTest() { // FT - From/To
        Assert.AreEqual("1111111", new Bits("x{FF_}").ToString("bin"));
        Assert.AreEqual("b{101010111100110111101111}", new Bits("ABCDEF").ToString("fiftBin"));
        Assert.AreEqual("A9F3C_", new Bits("b{10101001111100111}").ToString("hex"));
        Assert.AreEqual("x{D5ADE}", new Bits("11010101101011011110").ToString("fiftHex"));
    }

    [Test]
    public void SRL_UintTest() { // SRL - Store/Read/Load
        long l = 1234;
        ulong ul = 1234;
        BigInteger bi = 1234;
        Assert.AreEqual(l, (long)(new BitsBuilder().StoreUInt(l, 200).Build().Parse().ReadUInt(200)));
        Assert.AreEqual(ul, (ulong)(new BitsBuilder().StoreUInt(ul, 200).Build().Parse().ReadUInt(200)));
        Assert.That(new BitsBuilder().StoreUInt(bi, 200).Build().Parse().ReadUInt(200), Is.EqualTo(bi));
        Assert.AreEqual(l, (long)(new BitsBuilder().StoreUInt(l, 200).Build().Parse().LoadUInt(200)));
        Assert.AreEqual(ul, (ulong)(new BitsBuilder().StoreUInt(ul, 200).Build().Parse().LoadUInt(200)));
        Assert.That(new BitsBuilder().StoreUInt(bi, 200).Build().Parse().LoadUInt(200), Is.EqualTo(bi));
    }

    [Test]
    public void SRL_IntTest() {
        long l = 1234;
        ulong ul = 1234;
        BigInteger bi = 1234;
        long nl = -1234;
        BigInteger nbi = -1234;
        Assert.AreEqual(l, (long)(new BitsBuilder().StoreInt(l, 200).Build().Parse().ReadInt(200)));
        Assert.AreEqual(ul, (ulong)(new BitsBuilder().StoreInt(ul, 200).Build().Parse().ReadInt(200)));
        Assert.That(new BitsBuilder().StoreInt(bi, 200).Build().Parse().ReadInt(200), Is.EqualTo(bi));
        Assert.AreEqual(l, (long)(new BitsBuilder().StoreInt(l, 200).Build().Parse().LoadInt(200)));
        Assert.AreEqual(ul, (ulong)(new BitsBuilder().StoreInt(ul, 200).Build().Parse().LoadInt(200)));
        Assert.That(new BitsBuilder().StoreInt(bi, 200).Build().Parse().LoadInt(200), Is.EqualTo(bi));
        Assert.AreEqual(nl, (long)(new BitsBuilder().StoreInt(nl, 200).Build().Parse().ReadInt(200)));
        Assert.That(new BitsBuilder().StoreInt(nbi, 200).Build().Parse().ReadInt(200), Is.EqualTo(nbi));
        Assert.AreEqual(nl, (long)(new BitsBuilder().StoreInt(nl, 200).Build().Parse().LoadInt(200)));
        Assert.That(new BitsBuilder().StoreInt(nbi, 200).Build().Parse().LoadInt(200), Is.EqualTo(nbi));
    }
    
}
