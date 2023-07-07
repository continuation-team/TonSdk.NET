﻿namespace TonSdk.Core.Tests;

public class AddressTest
{
    [Test]
    public void Test_ParsingExceptions()
    {
        Assert.DoesNotThrow(() => new Address(new Address("EQCD39VS5jcptHL8vMjEXrzGaRcCVYto7HUn4bpAOg8xqB2N")));
        Assert.DoesNotThrow(() => new Address(new Address(new Address("0:83dfd552e63729b472fcbcc8c45ebcc6691702558b68ec7527e1ba403a0f31a8"))));
        Assert.Throws<Exception>(() => new Address("cat"), "Address: can\t parse address. Unknown type.");
        Assert.Throws<Exception>(() => new Address(string.Empty), "Address: can\t parse address. Unknown type.");
    }
    
    [Test]
    public void Test_CheckAddress()
    {
        Assert.That(new Address("EQCD39VS5jcptHL8vMjEXrzGaRcCVYto7HUn4bpAOg8xqB2N").IsBounceable, Is.EqualTo(true));
        Assert.That(new Address("EQCD39VS5jcptHL8vMjEXrzGaRcCVYto7HUn4bpAOg8xqB2N").IsBounceable(false), Is.EqualTo(false));
        Assert.That(new Address("EQCD39VS5jcptHL8vMjEXrzGaRcCVYto7HUn4bpAOg8xqB2N").IsTestOnly, Is.EqualTo(false));
        Assert.That(new Address("EQCD39VS5jcptHL8vMjEXrzGaRcCVYto7HUn4bpAOg8xqB2N").IsTestOnly(true), Is.EqualTo(true));
        
        Assert.DoesNotThrow(() => new Address("EQCD39VS5jcptHL8vMjEXrzGaRcCVYto7HUn4bpAOg8xqB2N").GetWorkchain());
        Assert.DoesNotThrow(() => new Address("EQCD39VS5jcptHL8vMjEXrzGaRcCVYto7HUn4bpAOg8xqB2N").GetHash());
        Assert.DoesNotThrow(() => new Address("EQCD39VS5jcptHL8vMjEXrzGaRcCVYto7HUn4bpAOg8xqB2N").ToString());
        Assert.DoesNotThrow(() => new Address("EQCD39VS5jcptHL8vMjEXrzGaRcCVYto7HUn4bpAOg8xqB2N").ToString(AddressType.Base64, null));
    }

    [Test]
    public void Test_EqualsTool()
    {
        Assert.That(new Address("EQCD39VS5jcptHL8vMjEXrzGaRcCVYto7HUn4bpAOg8xqB2N").Equals(new Address("0:83dfd552e63729b472fcbcc8c45ebcc6691702558b68ec7527e1ba403a0f31a8")), Is.EqualTo(true));
        Assert.That(new Address("UQCD39VS5jcptHL8vMjEXrzGaRcCVYto7HUn4bpAOg8xqEBI").Equals(new Address("0:83dfd552e63729b472fcbcc8c45ebcc6691702558b68ec7527e1ba403a0f31a8")), Is.EqualTo(true));
        Assert.That(new Address("kQCD39VS5jcptHL8vMjEXrzGaRcCVYto7HUn4bpAOg8xqKYH").Equals(new Address("0:83dfd552e63729b472fcbcc8c45ebcc6691702558b68ec7527e1ba403a0f31a8")), Is.EqualTo(true));
        Assert.That(new Address("0QCD39VS5jcptHL8vMjEXrzGaRcCVYto7HUn4bpAOg8xqPvC").Equals(new Address("0:83dfd552e63729b472fcbcc8c45ebcc6691702558b68ec7527e1ba403a0f31a8")), Is.EqualTo(true));
        Assert.That(new Address("EQCD39VS5jcptHL8vMjEXrzGaRcCVYto7HUn4bpAOg8xqB2N").Equals(new Address("EQCbGQmLv8Ikp-R5JcgXRppiMLdghtd1qPzPxYyToKPdW4zr")), Is.EqualTo(false));
    }

    [Test]
    public void Test_InToString()
    {
        Assert.That(new Address("0:83dfd552e63729b472fcbcc8c45ebcc6691702558b68ec7527e1ba403a0f31a8").ToString(), Is.EqualTo("EQCD39VS5jcptHL8vMjEXrzGaRcCVYto7HUn4bpAOg8xqB2N"));
        Assert.That(new Address("EQCD39VS5jcptHL8vMjEXrzGaRcCVYto7HUn4bpAOg8xqB2N").ToString(), Is.EqualTo("EQCD39VS5jcptHL8vMjEXrzGaRcCVYto7HUn4bpAOg8xqB2N"));
        Assert.That(new Address("kQCD39VS5jcptHL8vMjEXrzGaRcCVYto7HUn4bpAOg8xqKYH").ToString(), Is.EqualTo("kQCD39VS5jcptHL8vMjEXrzGaRcCVYto7HUn4bpAOg8xqKYH"));
        Assert.That(new Address("0QCD39VS5jcptHL8vMjEXrzGaRcCVYto7HUn4bpAOg8xqPvC").ToString(), Is.EqualTo("0QCD39VS5jcptHL8vMjEXrzGaRcCVYto7HUn4bpAOg8xqPvC"));
        Assert.That(new Address("UQCD39VS5jcptHL8vMjEXrzGaRcCVYto7HUn4bpAOg8xqEBI").ToString(), Is.EqualTo("UQCD39VS5jcptHL8vMjEXrzGaRcCVYto7HUn4bpAOg8xqEBI"));
    }

    [Test]
    public void Test_OutToString()
    {
        Assert.That(new Address("EQCD39VS5jcptHL8vMjEXrzGaRcCVYto7HUn4bpAOg8xqB2N").ToString(AddressType.Raw), Is.EqualTo("0:83dfd552e63729b472fcbcc8c45ebcc6691702558b68ec7527e1ba403a0f31a8"));
        Assert.Throws<Exception>(() =>
            new Address("EQCD39VS5jcptHL8vMjEXrzGaRcCVYto7HUn4bpAOg8xqB2N").ToString(AddressType.Base64,
                new AddressStringifyOptions(true, true, true, 222)));  
        Assert.That(new Address("EQCD39VS5jcptHL8vMjEXrzGaRcCVYto7HUn4bpAOg8xqB2N").ToString(AddressType.Base64, new AddressStringifyOptions(true, false, true)), Is.EqualTo("EQCD39VS5jcptHL8vMjEXrzGaRcCVYto7HUn4bpAOg8xqB2N"));
        Assert.That(new Address("EQCD39VS5jcptHL8vMjEXrzGaRcCVYto7HUn4bpAOg8xqB2N").ToString(AddressType.Base64, new AddressStringifyOptions(true, false, true)), Is.EqualTo("EQCD39VS5jcptHL8vMjEXrzGaRcCVYto7HUn4bpAOg8xqB2N"));
        Assert.That(new Address("EQCD39VS5jcptHL8vMjEXrzGaRcCVYto7HUn4bpAOg8xqB2N").ToString(AddressType.Base64, new AddressStringifyOptions(true, false, false)), Is.EqualTo("EQCD39VS5jcptHL8vMjEXrzGaRcCVYto7HUn4bpAOg8xqB2N"));
        Assert.That(new Address("EQCD39VS5jcptHL8vMjEXrzGaRcCVYto7HUn4bpAOg8xqB2N").ToString(AddressType.Base64, new AddressStringifyOptions(false, false, true)), Is.EqualTo("UQCD39VS5jcptHL8vMjEXrzGaRcCVYto7HUn4bpAOg8xqEBI"));
        Assert.That(new Address("EQCD39VS5jcptHL8vMjEXrzGaRcCVYto7HUn4bpAOg8xqB2N").ToString(AddressType.Base64, new AddressStringifyOptions(true, true, false)), Is.EqualTo("kQCD39VS5jcptHL8vMjEXrzGaRcCVYto7HUn4bpAOg8xqKYH"));
        Assert.That(new Address("EQCD39VS5jcptHL8vMjEXrzGaRcCVYto7HUn4bpAOg8xqB2N").ToString(AddressType.Base64, new AddressStringifyOptions(false, true, false)), Is.EqualTo("0QCD39VS5jcptHL8vMjEXrzGaRcCVYto7HUn4bpAOg8xqPvC"));
        Assert.That(new Address("EQCD39VS5jcptHL8vMjEXrzGaRcCVYto7HUn4bpAOg8xqB2N").ToString(AddressType.Base64, new AddressStringifyOptions(false, false, false)), Is.EqualTo("UQCD39VS5jcptHL8vMjEXrzGaRcCVYto7HUn4bpAOg8xqEBI"));
    }
}
