using TonSdk.Client;
using TonSdk.Core;

namespace TonSdk.Client.Tests;
public class ClientTest
{
    TonClient client = new(new() { Endpoint = "https://toncenter.com/api/v2/jsonRPC", ApiKey = "0efdbc011a4c1c36fc74c3c4291bd9a1eccbf960b9113516fc62bc9e6a127d6d" });

    [Test]
    public async Task Test_AddressBalance()
    {
        Assert.That((await client.GetBalance(new Address("EQCD39VS5jcptHL8vMjEXrzGaRcCVYto7HUn4bpAOg8xqB2N"))) is Coins , Is.EqualTo(true));
    }

    [Test]
    public async Task Test_IsContractdeployed()
    {
        Assert.That(await client.IsContractDeployed(new Address("EQCD39VS5jcptHL8vMjEXrzGaRcCVYto7HUn4bpAOg8xqB2N")), Is.EqualTo(true));
        Assert.That(await client.IsContractDeployed(new Address("EQCD39VS5jcptHL8vMjEXrzGaRcCVYto7HAn4bpAOg8xofto")), Is.EqualTo(false));
    }

    [Test]
    public async Task Test_WalletGetSeqno()
    {
        Assert.That(await client.Wallet.GetSeqno(new Address("EQCD39VS5jcptHL8vMjEXrzGaRcCVYto7HUn4bpAOg8xqB2N")) > 0, Is.EqualTo(true));
        Assert.That(await client.Wallet.GetSeqno(new Address("EQCD39VS5jcptHL8vMjEXrzGaRcCVYto7HAn4bpAOg8xofto")), Is.EqualTo(null));
    }

    [Test]
    public async Task Test_NftGetItemAddress()
    {
        Assert.That((await client.Nft.GetItemAddress(new Address("EQBiX_Sxuy5htTLg_BzUk1kw7FuI3zWvmnotiuvmnwqZpmUj"), 0)).Equals(new Address("EQCD4WUntH5862PfOQc1vsGAPZsjnm49Mxrj-23O01B_YaJ2")), Is.EqualTo(true));
        Assert.ThrowsAsync<Exception>(async Task() => await client.Nft.GetItemAddress(new Address("EQCD39VS5jcptHL8vMjEXrzGaRcCVYto7HAn4bpAOg8xofto"), 2), "Cannot retrieve nft address.");
    }


}