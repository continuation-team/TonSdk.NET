
using System.Numerics;
using TonSdk.Core.Boc;
using TonSdk.Core.Crypto;

namespace TonSdk.Core.Tests;


public class BitsTest {
    [Test]
    public void FT_StringTest() { // FT - From/To
        Assert.Equals("1111111", new Bits("x{FF_}").ToString("bin"));
        Assert.Equals("b{101010111100110111101111}", new Bits("ABCDEF").ToString("fiftBin"));
        Assert.Equals("A9F3C_", new Bits("b{10101001111100111}").ToString("hex"));
        Assert.Equals("x{D5ADE}", new Bits("11010101101011011110").ToString("fiftHex"));
    }

    [Test]
    public void SRL_BitsTest() {
        // Store
        var b = new BitsBuilder(24).StoreBits(new Bits("ABCDEF")).Build();

        // Check
        Assert.Equals("ABCDEF", b.ToString("hex"));

        // Parse
        var bs = b.Parse();

        // Read
        Assert.Equals("ABC", bs.ReadBits(12).ToString("hex"));

        // Check
        Assert.Equals(24, bs.Bits.Length);

        // Load
        Assert.Equals("ABCDE", bs.LoadBits(20).ToString("hex"));

        // Check
        Assert.Equals(4, bs.Bits.Length);
        Assert.Equals("F", bs.Bits.ToString("hex"));
    }

    [Test]
    public void SRL_UintTest() { // SRL - Store/Read/Load
        long l = 1234;
        ulong ul = 1234;
        BigInteger bi = 1234;

        // Store
        var b_l = new BitsBuilder(239).StoreUInt(l, 239).Build();
        var b_ul = new BitsBuilder(239).StoreUInt(ul, 239).Build();
        var b_bi = new BitsBuilder(239).StoreUInt(bi, 239).Build();

        // Check
        Assert.Equals("0000000000000000000000000000000000000000000000000000000009A5_", b_l.ToString("hex"));
        Assert.Equals("0000000000000000000000000000000000000000000000000000000009A5_", b_ul.ToString("hex"));
        Assert.Equals("0000000000000000000000000000000000000000000000000000000009A5_", b_bi.ToString("hex"));

        // Parse
        var bs_l = b_l.Parse();
        var bs_ul = b_ul.Parse();
        var bs_bi = b_bi.Parse();

        // Read
        Assert.Equals(19, (uint)bs_l.ReadUInt(233));
        Assert.Equals(19, (uint)bs_ul.ReadUInt(233));
        Assert.Equals(19, (uint)bs_bi.ReadUInt(233));

        // Check
        Assert.Equals(239, bs_l.RemainderBits);
        Assert.Equals(239, bs_ul.RemainderBits);
        Assert.Equals(239, bs_bi.RemainderBits);

        // Load
        Assert.Equals(77, (uint)bs_l.LoadUInt(235));
        Assert.Equals(77, (uint)bs_ul.LoadUInt(235));
        Assert.Equals(77, (uint)bs_bi.LoadUInt(235));

        // Check
        Assert.Equals(4, bs_l.RemainderBits);
        Assert.Equals(4, bs_ul.RemainderBits);
        Assert.Equals(4, bs_bi.RemainderBits);

        Assert.Equals("2", bs_l.Bits.ToString("hex"));
        Assert.Equals("2", bs_ul.Bits.ToString("hex"));
        Assert.Equals("2", bs_bi.Bits.ToString("hex"));
    }

    [Test]
    public void SRL_IntTest() {
        long l = 1234;
        ulong ul = 1234;
        BigInteger bi = 1234;
        long nl = -1234;
        BigInteger nbi = -1234;

        // Store
        var b_l = new BitsBuilder(239).StoreInt(l, 239).Build();
        var b_ul = new BitsBuilder(239).StoreInt(ul, 239).Build();
        var b_bi = new BitsBuilder(239).StoreInt(bi, 239).Build();
        var b_nl = new BitsBuilder(239).StoreInt(nl, 239).Build();
        var b_nbi = new BitsBuilder(239).StoreInt(nbi, 239).Build();

        // Check
        Assert.Equals("0000000000000000000000000000000000000000000000000000000009A5_", b_l.ToString("hex"));
        Assert.Equals("0000000000000000000000000000000000000000000000000000000009A5_", b_ul.ToString("hex"));
        Assert.Equals("0000000000000000000000000000000000000000000000000000000009A5_", b_bi.ToString("hex"));
        Assert.Equals("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF65D_", b_nl.ToString("hex"));
        Assert.Equals("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF65D_", b_nbi.ToString("hex"));

        // Parse
        var bs_l = b_l.Parse();
        var bs_ul = b_ul.Parse();
        var bs_bi = b_bi.Parse();
        var bs_nl = b_nl.Parse();
        var bs_nbi = b_nbi.Parse();

        // Read
        Assert.Equals(19, (int)bs_l.ReadInt(233));
        Assert.Equals(19, (int)bs_ul.ReadInt(233));
        Assert.Equals(19, (int)bs_bi.ReadInt(233));
        Assert.Equals(-20, (int)bs_nl.ReadInt(233));
        Assert.Equals(-20, (int)bs_nbi.ReadInt(233));

        // Check
        Assert.Equals(239, bs_l.RemainderBits);
        Assert.Equals(239, bs_ul.RemainderBits);
        Assert.Equals(239, bs_bi.RemainderBits);
        Assert.Equals(239, bs_nl.RemainderBits);
        Assert.Equals(239, bs_nbi.RemainderBits);

        // Load
        Assert.Equals(77, (int)bs_l.LoadInt(235));
        Assert.Equals(77, (int)bs_ul.LoadInt(235));
        Assert.Equals(77, (int)bs_bi.LoadInt(235));
        Assert.Equals(-78, (int)bs_nl.LoadInt(235));
        Assert.Equals(-78, (int)bs_nbi.LoadInt(235));

        // Check
        Assert.Equals(4, bs_l.RemainderBits);
        Assert.Equals(4, bs_ul.RemainderBits);
        Assert.Equals(4, bs_bi.RemainderBits);
        Assert.Equals(4, bs_nl.RemainderBits);
        Assert.Equals(4, bs_nbi.RemainderBits);

        Assert.Equals("2", bs_l.Bits.ToString("hex"));
        Assert.Equals("2", bs_ul.Bits.ToString("hex"));
        Assert.Equals("2", bs_bi.Bits.ToString("hex"));
        Assert.Equals("E", bs_nl.Bits.ToString("hex"));
        Assert.Equals("E", bs_nbi.Bits.ToString("hex"));
    }

    [Test]
    public void Extreme_IntUintTest() {
        var u_min = new BigInteger(0);
        var u_max = (new BigInteger(1) << 256) - 1;
        var i_min = new BigInteger(-1) << 255;
        var i_max = (new BigInteger(1) << 255) - 1;

        // Store
        var b_u_min = new BitsBuilder(256).StoreUInt(u_min, 256).Build();
        var b_u_max = new BitsBuilder(256).StoreUInt(u_max, 256).Build();
        var b_i_min = new BitsBuilder(256).StoreInt(i_min, 256).Build();
        var b_i_max = new BitsBuilder(256).StoreInt(i_max, 256).Build();

        // Check
        Assert.Equals("0000000000000000000000000000000000000000000000000000000000000000", b_u_min.ToString("hex"));
        Assert.Equals("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF", b_u_max.ToString("hex"));
        Assert.Equals("8000000000000000000000000000000000000000000000000000000000000000", b_i_min.ToString("hex"));
        Assert.Equals("7FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF", b_i_max.ToString("hex"));

        // Parse
        var bs_u_min = b_u_min.Parse();
        var bs_u_max = b_u_max.Parse();
        var bs_i_min = b_i_min.Parse();
        var bs_i_max = b_i_max.Parse();

        // Read
        Assert.Equals(u_min, bs_u_min.ReadUInt(256));
        Assert.Equals(u_max, bs_u_max.ReadUInt(256));
        Assert.Equals(i_min, bs_i_min.ReadInt(256));
        Assert.Equals(i_max, bs_i_max.ReadInt(256));
    }
}
