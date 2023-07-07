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
}