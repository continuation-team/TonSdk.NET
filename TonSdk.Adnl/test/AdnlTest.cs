using NUnit.Framework;
using TonSdk.Core.Crypto;

namespace TonSdk.Adnl.Tests;
public class AdnlTest
{
    private readonly string AdnlHost = "5.9.10.47";
    private readonly int AdnlPort = 19949;
    private readonly string AdnlPubKey = "n4VDnSCUuSpjnCyUk9e3QOOd6o0ItSWYbTnW3Wnn8wk=";

    private AdnlClientTcp client;

    [SetUp]
    public void SetUpTest()
    {
        client = new AdnlClientTcp(AdnlHost, AdnlPort, AdnlPubKey);
    }
    
    [Test]
    public async Task Test_ConnectionState()
    {
        Assert.That(AdnlClientState.Closed == client.State, Is.EqualTo(true));
        await client.Connect();
        Assert.That(AdnlClientState.Connecting == client.State, Is.EqualTo(true));
        
        while (client.State != AdnlClientState.Open)
        {
            // waiting
            continue;
        }
        
        Assert.That(AdnlClientState.Open == client.State, Is.EqualTo(true));
        
        client.End();
        Assert.That(AdnlClientState.Closed == client.State, Is.EqualTo(true));
    }

    [Test]
    public async Task Test_GetDataTest()
    {
        byte[]? data = null;
        client.DataReceived += (response) => data = response;
        await client.Connect();
        
        while (client.State != AdnlClientState.Open)
        {
            // waiting
            continue;
        }

        await client.Write(Utils.HexToBytes(
            "7af98bb435263e6c95d6fecb497dfd0aa5f031e7d412986b5ce720496db512052e8f2d100cdf068c7904345aad16000000000000"));
        
        while (data == null)
        {
            // waiting
            continue;
        }
        Assert.That(data.Length != 0, Is.EqualTo(true));
    }
}