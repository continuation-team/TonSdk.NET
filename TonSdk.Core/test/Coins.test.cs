using System;

namespace TonSdk.Core.Tests;

public class CoinsTest
{
    [Test]
    public void Test_ParsingExceptions()
    {
        Assert.DoesNotThrow(() => new Coins(10));
        Assert.DoesNotThrow(() => new Coins(10.5));
        Assert.DoesNotThrow(() => new Coins(10.5f));
        Assert.DoesNotThrow(() => new Coins(10.5d));
        Assert.DoesNotThrow(() => new Coins("10.5"));
        Assert.DoesNotThrow(() => new Coins("10,5"));
        Assert.DoesNotThrow(() => new Coins(10, new CoinsOptions(false, 10)));
        Assert.DoesNotThrow(() => new Coins(20.555, new CoinsOptions(false, 10)));
        Assert.Throws<Exception>(() => new Coins(null), "Invalid value");
        Assert.Throws<Exception>(() => new Coins(string.Empty), "Invalid value");
        Assert.Throws<Exception>(() => new Coins("cat"), "Invalid value");
        Assert.Throws<Exception>(() => new Coins("10.5d"), "Invalid value");
        Assert.Throws<Exception>(() => new Coins(20.555, new CoinsOptions(false, 0)),
            "Invalid Coins value, decimals places \"3\" can't be greater than selected \"0\"");
    }

    [Test]
    public void Test_CoinsToCoinsEqual()
    {
        Assert.That(new Coins("10").Eq(new Coins(new Coins("10"))), Is.EqualTo(true));
        Assert.That(new Coins("10").Eq(new Coins(new Coins("10.5"))), Is.EqualTo(false));
        Assert.That(new Coins("10").Eq(new Coins(new Coins(10))), Is.EqualTo(true));
        Assert.That(new Coins("10").Eq(new Coins(new Coins(10.2))), Is.EqualTo(false));
        Assert.That(new Coins("10.5").Eq(new Coins(new Coins(10.5))), Is.EqualTo(true));
        Assert.That(new Coins(10).Eq(new Coins(new Coins(10))), Is.EqualTo(true));
    }

    [Test]
    public void Test_NanoToCoinsEqual()
    {
        Assert.That(new Coins("20.555").Eq(new Coins(20_555_000_000, new CoinsOptions(true, 9))), Is.EqualTo(true));
        Assert.That(new Coins("10").Eq(new Coins(10_000_000_000, new CoinsOptions(true, 9))), Is.EqualTo(true));
    }

    [Test]
    public void Test_AnyToCoinsEqual()
    {
        Assert.That(new Coins("10").Eq(new Coins(10)), Is.EqualTo(true));
        Assert.That(new Coins("10").Eq(new Coins(10.5)), Is.EqualTo(false));
        Assert.That(new Coins("10").Eq(new Coins(10)), Is.EqualTo(true));
        Assert.That(new Coins("10").Eq(new Coins(10.2)), Is.EqualTo(false));
        Assert.That(new Coins("10.5").Eq(new Coins(10.5)), Is.EqualTo(true));
        Assert.That(new Coins(10).Eq(new Coins(10)), Is.EqualTo(true));
        Assert.Throws<Exception>(() => new Coins(10, new CoinsOptions(false, 222)));
    }

    [Test]
    public void Test_AddOperation()
    {
        Assert.That(new Coins("10").Add(new Coins(10)).Eq(new Coins(20)), Is.EqualTo(true));
        Assert.That(new Coins("10").Add(new Coins(10.5)).Eq(new Coins(20.5)), Is.EqualTo(true));
        Assert.That(new Coins(10).Add(new Coins(10)).Eq(new Coins(20)), Is.EqualTo(true));
        Assert.Throws<Exception>(() => new Coins(10).Add(new Coins(10.0, new CoinsOptions(false, 10))));
    }

    [Test]
    public void Test_CheckCoins()
    {
        Assert.That(new Coins("10").IsNegative, Is.EqualTo(false));
        Assert.That(new Coins("10").IsPositive, Is.EqualTo(true));
        Assert.That(new Coins(0).IsZero, Is.EqualTo(true));
        Assert.DoesNotThrow(() => new Coins(10).ToBigInt());
        var d1 = new Coins("10,641462085").ToDecimal();
        var d2=decimal.Parse("10,641462085");
        Assert.That(new Coins("10,641462085").ToDecimal() ,Is.EqualTo(decimal.Parse("10,641462085")));
    }

    [Test]
    public void Test_SubOperation()
    {
        Assert.That(new Coins("10").Sub(new Coins(10)).Eq(new Coins(0)), Is.EqualTo(true));
        Assert.That(new Coins("10").Sub(new Coins(1)).Eq(new Coins(9)), Is.EqualTo(true));
        Assert.That(new Coins(10).Sub(new Coins(10)).Eq(new Coins(0)), Is.EqualTo(true));
    }

    [Test]
    public void Test_DivOperation()
    {
        Assert.That(new Coins("10").Div(1).Eq(new Coins(10)), Is.EqualTo(true));
        Assert.That(new Coins("10").Div(2.5).Eq(new Coins(4)), Is.EqualTo(true));
        Assert.That(new Coins(10).Div(10).Eq(new Coins(1)), Is.EqualTo(true));
    }

    [Test]
    public void Test_MulOperation()
    {
        Assert.That(new Coins("10").Mul(10).Eq(new Coins(100)), Is.EqualTo(true));
        Assert.That(new Coins("10").Mul(1).Eq(new Coins(10)), Is.EqualTo(true));
        Assert.That(new Coins(10).Mul(10).Eq(new Coins(100)), Is.EqualTo(true));
        Assert.Throws<Exception>(() => new Coins(10).Mul('s').Eq(new Coins(100)));
        Assert.Throws<Exception>(() => new Coins(10).Mul("sss").Eq(new Coins(100)));
    }

    [Test]
    public void Test_GreaterOperation()
    {
        Assert.That(new Coins("10").Gt(new Coins(10)), Is.EqualTo(false));
        Assert.That(new Coins("10").Gt(new Coins(10.5)), Is.EqualTo(false));
        Assert.That(new Coins(10).Gt(new Coins(1)), Is.EqualTo(true));
    }

    [Test]
    public void Test_GreaterOrEqualOperation()
    {
        Assert.That(new Coins("10").Gte(new Coins(10)), Is.EqualTo(true));
        Assert.That(new Coins("10").Gte(new Coins(10.5)), Is.EqualTo(false));
        Assert.That(new Coins(10).Gte(new Coins(1)), Is.EqualTo(true));
    }

    [Test]
    public void Test_LessOperation()
    {
        Assert.That(new Coins("10").Lt(new Coins(10)), Is.EqualTo(false));
        Assert.That(new Coins("10").Lt(new Coins(10.5)), Is.EqualTo(true));
        Assert.That(new Coins(10).Lt(new Coins(1)), Is.EqualTo(false));
    }

    [Test]
    public void Test_LessOrEqualOperation()
    {
        Assert.That(new Coins("10").Lte(new Coins(10)), Is.EqualTo(true));
        Assert.That(new Coins("10").Lte(new Coins(10.5)), Is.EqualTo(true));
        Assert.That(new Coins(10).Lte(new Coins(1)), Is.EqualTo(false));
    }

    [Test]
    public void Test_ToNanoString()
    {
        Assert.That(new Coins("-1.23").ToNano(), Is.EqualTo("-1230000000"));
        Assert.That(new Coins("0").ToNano(), Is.EqualTo("0"));
        Assert.That(new Coins("10.23").ToNano(), Is.EqualTo("10230000000"));
        Assert.That(new Coins("-1,23").ToNano(), Is.EqualTo("-1230000000"));
        Assert.That(new Coins("10,23").ToNano(), Is.EqualTo("10230000000"));
    }

    [Test]
    public void Test_FromNanoCoins()
    {
        Assert.That(Coins.FromNano(9007199254740992).ToString(), Is.EqualTo("9007199,254740992"));
        Assert.That(Coins.FromNano("9007199254740992").ToString(), Is.EqualTo("9007199,254740992"));
    }

    [Test]
    public void Test_LittleDoubleNumber()
    {
        Assert.DoesNotThrow(() =>
        {
            var coins = new Coins(0.00000001);
        });
    }
}