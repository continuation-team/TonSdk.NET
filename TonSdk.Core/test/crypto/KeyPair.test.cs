using TonSdk.Core.Boc;
using TonSdk.Core.Crypto;

namespace TonSdk.Core.Tests;

public class KeyPairTest
{
    [Test]
    public void Test_KeyPairSign()
    {
        Cell cell = new CellBuilder().StoreAddress(new Address("EQA_d9IqxSQCSuwZIvH0RRSUMvWK4qrvl5ZH_nOHFH7Gxifq")).Build();
        Assert.That(Utils.BytesToHex(KeyPair.Sign(cell, new Bits().ToBytes())), Is.EqualTo("7FDE98874905677ED0AC1F013BF6B5B50B091C6D171F2423BF8C83489B78563E9A7545591D629F79CB6019D360B7B9388A8D2BF19E43D07651F1517767439B09"));
    }
}