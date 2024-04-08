using TonSdk.Core;

namespace TonSdk.Client.Tests;
public class ClientTest
{
    
    TonClient client = new(TonClientType.HTTP_TONCENTERAPIV2, 
        new HttpParameters() { Endpoint = "https://toncenter.com/api/v2/jsonRPC", ApiKey = "0efdbc011a4c1c36fc74c3c4291bd9a1eccbf960b9113516fc62bc9e6a127d6d" });
    TonClient client_lite = new TonClient(TonClientType.LITECLIENT, new LiteClientParameters("5.9.10.47", 19949, "n4VDnSCUuSpjnCyUk9e3QOOd6o0ItSWYbTnW3Wnn8wk="));

    [Test]
    public async Task Test_AddressBalance()
    {
        Assert.That((await client.GetBalance(new Address("EQCD39VS5jcptHL8vMjEXrzGaRcCVYto7HUn4bpAOg8xqB2N"))) is Coins , Is.EqualTo(true));
        Assert.That((await client_lite.GetBalance(new Address("EQCD39VS5jcptHL8vMjEXrzGaRcCVYto7HUn4bpAOg8xqB2N"))) is Coins , Is.EqualTo(true));
    }

    [Test]
    public void Test_GetMasterchainInfo()
    {
        Assert.DoesNotThrowAsync(() => client.GetMasterchainInfo());
        Assert.DoesNotThrowAsync(() => client_lite.GetMasterchainInfo());
    }

    [Test]
    public async Task Test_Shards()
    {
        var masterchainInfo = await client.GetMasterchainInfo();
        Assert.DoesNotThrowAsync(() => client.Shards(masterchainInfo.Value.LastBlock.Seqno));
        var masterchainInfo2 = await client_lite.GetMasterchainInfo();
        Assert.DoesNotThrow(() => client_lite.Shards(masterchainInfo2.Value.LastBlock.Seqno));
    }

    [Test]
    public void Test_GetBlockTransactions()
    {
        Assert.DoesNotThrowAsync(() => client.GetBlockTransactions(0, -9223372036854775808, 14255576, count: 40));
        
        var last = client_lite.GetMasterchainInfo().Result.Value.LastBlock;
        
        Assert.DoesNotThrowAsync(() => client_lite.GetBlockTransactions(last.Workchain, last.Shard, last.Seqno, count: 10));
    }

    [Test]
    public async Task Test_IsContractDeployed()
    {
        Assert.That(await client.IsContractDeployed(new Address("EQCD39VS5jcptHL8vMjEXrzGaRcCVYto7HUn4bpAOg8xqB2N")), Is.EqualTo(true));
        Assert.That(await client.IsContractDeployed(new Address("EQCD39VS5jcptHL8vMjEXrzGaRcCVYto7HAn4bpAOg8xofto")), Is.EqualTo(false));
        Assert.That(await client_lite.IsContractDeployed(new Address("EQCD39VS5jcptHL8vMjEXrzGaRcCVYto7HUn4bpAOg8xqB2N")), Is.EqualTo(true));
        Assert.That(await client_lite.IsContractDeployed(new Address("EQCD39VS5jcptHL8vMjEXrzGaRcCVYto7HAn4bpAOg8xofto")), Is.EqualTo(false));
    }

    [Test]
    public async Task Test_WalletGetSeqno()
    {
        Assert.That(await client.Wallet.GetSeqno(new Address("EQCD39VS5jcptHL8vMjEXrzGaRcCVYto7HUn4bpAOg8xqB2N")) > 0, Is.EqualTo(true));
        Assert.That(await client.Wallet.GetSeqno(new Address("EQCD39VS5jcptHL8vMjEXrzGaRcCVYto7HAn4bpAOg8xofto")), Is.EqualTo(null));
        Assert.That(await client_lite.Wallet.GetSeqno(new Address("EQCD39VS5jcptHL8vMjEXrzGaRcCVYto7HUn4bpAOg8xqB2N")) > 0, Is.EqualTo(true));
        Assert.That(await client_lite.Wallet.GetSeqno(new Address("EQCD39VS5jcptHL8vMjEXrzGaRcCVYto7HAn4bpAOg8xofto")), Is.EqualTo(null));
    }

    [Test]
    public void Test_WalletGetSubwalletAndPluginList()
    {
        Assert.DoesNotThrowAsync(async Task() => await client.Wallet.GetSubwalletId(new Address("EQCD39VS5jcptHL8vMjEXrzGaRcCVYto7HUn4bpAOg8xqB2N")));
        Assert.DoesNotThrowAsync(async Task() => await client.Wallet.GetPluginList(new Address("EQCD39VS5jcptHL8vMjEXrzGaRcCVYto7HUn4bpAOg8xqB2N")));
        Assert.DoesNotThrowAsync(async Task() => await client_lite.Wallet.GetSubwalletId(new Address("EQCD39VS5jcptHL8vMjEXrzGaRcCVYto7HUn4bpAOg8xqB2N")));
        Assert.DoesNotThrowAsync(async Task() => await client_lite.Wallet.GetPluginList(new Address("EQCD39VS5jcptHL8vMjEXrzGaRcCVYto7HUn4bpAOg8xqB2N")));
    }

    [Test]
    public async Task Test_DnsGetWalletAddress()
    {
        Assert.That((await client_lite.Dns.GetWalletAddress("foundation.ton")).Equals(new Address("UQCD39VS5jcptHL8vMjEXrzGaRcCVYto7HUn4bpAOg8xqEBI")), Is.EqualTo(true));
        Assert.That((await client.Dns.GetWalletAddress("foundation.ton")).Equals(new Address("UQCD39VS5jcptHL8vMjEXrzGaRcCVYto7HUn4bpAOg8xqEBI")), Is.EqualTo(true));
    }

    [Test]
    public async Task Test_NftGetItemAddress()
    {
        Assert.That((await client.Nft.GetItemAddress(new Address("EQBiX_Sxuy5htTLg_BzUk1kw7FuI3zWvmnotiuvmnwqZpmUj"), 0)).Equals(new Address("EQCD4WUntH5862PfOQc1vsGAPZsjnm49Mxrj-23O01B_YaJ2")), Is.EqualTo(true));
        Assert.ThrowsAsync<Exception>(async Task() => await client.Nft.GetItemAddress(new Address("EQCD39VS5jcptHL8vMjEXrzGaRcCVYto7HAn4bpAOg8xofto"), 2), "Cannot retrieve nft address.");
        Assert.That((await client_lite.Nft.GetItemAddress(new Address("EQBiX_Sxuy5htTLg_BzUk1kw7FuI3zWvmnotiuvmnwqZpmUj"), 0)).Equals(new Address("EQCD4WUntH5862PfOQc1vsGAPZsjnm49Mxrj-23O01B_YaJ2")), Is.EqualTo(true));
    }

    [Test]
    public async Task Test_NftGetRoyaltyParams()
    {
        Assert.That((await client.Nft.GetRoyaltyParams(new Address("EQBiX_Sxuy5htTLg_BzUk1kw7FuI3zWvmnotiuvmnwqZpmUj"))).RoyaltyAddress.Equals(new Address("0:580637c2c42ca612ccc0a641b40f9dedfd08e3882dcdf72cc4bd0467194ea014")), Is.EqualTo(true));
        Assert.ThrowsAsync<Exception>(async Task () => await client.Nft.GetRoyaltyParams(new Address("EQCD39VS5jcptHL8vMjEXrzGaRcCVYto7HAn4bpAOg8xofto")), "Cannot retrieve nft collection royalty params.");
        Assert.That((await client_lite.Nft.GetRoyaltyParams(new Address("EQBiX_Sxuy5htTLg_BzUk1kw7FuI3zWvmnotiuvmnwqZpmUj"))).RoyaltyAddress.Equals(new Address("0:580637c2c42ca612ccc0a641b40f9dedfd08e3882dcdf72cc4bd0467194ea014")), Is.EqualTo(true));
    }

    [Test]
    public async Task Test_NftGetCollectionData()
    {
        Assert.That((await client.Nft.GetCollectionData(new Address("EQBiX_Sxuy5htTLg_BzUk1kw7FuI3zWvmnotiuvmnwqZpmUj"))).OwnerAddress.Equals(new Address("EQAkL6uxUwDxqI_4r7GwdzbpzoxiVRrT92WZ2ri84qYW8xPz")), Is.EqualTo(true));
        Assert.ThrowsAsync<Exception>(async Task () => await client.Nft.GetCollectionData(new Address("EQCD39VS5jcptHL8vMjEXrzGaRcCVYto7HAn4bpAOg8xofto")), "Cannot retrieve nft collection data.");
        Assert.That((await client_lite.Nft.GetCollectionData(new Address("EQBiX_Sxuy5htTLg_BzUk1kw7FuI3zWvmnotiuvmnwqZpmUj"))).OwnerAddress.Equals(new Address("EQAkL6uxUwDxqI_4r7GwdzbpzoxiVRrT92WZ2ri84qYW8xPz")), Is.EqualTo(true));
    }

    [Test]
    public async Task Test_NftGetNftItemData()
    {
        Assert.That((await client.Nft.GetNftItemData(new Address("EQBcPvGBUexUD5R0u5QAponoKDxmBPx0MsDa1nuoMPOqMx_n"))).OwnerAddress.Equals(new Address("EQAzkwiDLGRqr766B6ZPaBBHLOYiVZCZpgrHTL3cT8TAcALA")), Is.EqualTo(true));
        Assert.ThrowsAsync<Exception>(async Task () => await client.Nft.GetNftItemData(new Address("EQCD39VS5jcptHL8vMjEXrzGaRcCVYto7HAn4bpAOg8xofto")), "Cannot retrieve nft item data.");
        Assert.That((await client_lite.Nft.GetNftItemData(new Address("EQBcPvGBUexUD5R0u5QAponoKDxmBPx0MsDa1nuoMPOqMx_n"))).OwnerAddress.Equals(new Address("EQAzkwiDLGRqr766B6ZPaBBHLOYiVZCZpgrHTL3cT8TAcALA")), Is.EqualTo(true));
    }

    [Test]
    public async Task Test_JettonGetJettonData()
    {
        Assert.That((await client.Jetton.GetData(new Address("EQAvDfWFG0oYX19jwNDNBBL1rKNT9XfaGP9HyTb5nb2Eml6y"))).AdminAddress.Equals(new Address("0:bd871909f584158689cf86520c54c8efa23559973ddc4b0820cbd12c514832c2")), Is.EqualTo(true));
        //Assert.That((await client.Jetton.GetData(new Address("EQBlU_tKISgpepeMFT9t3xTDeiVmo25dW_4vUOl6jId_BNIj"))).Content.Symbol.Equals("KOTE"), Is.EqualTo(true));
        Assert.That((await client.Jetton.GetData(new Address("EQDCJL0iQHofcBBvFBHdVG233Ri2V4kCNFgfRT-gqAd3Oc86"))).Content.Symbol, Is.EqualTo("FNZ"));
        Assert.ThrowsAsync<Exception>(async Task () => await client.Jetton.GetData(new Address("EQCD39VS5jcptHL8vMjEXrzGaRcCVYto7HAn4bpAOg8xofto")), "Cannot retrieve jetton data.");
        Assert.That((await client_lite.Jetton.GetData(new Address("EQAvDfWFG0oYX19jwNDNBBL1rKNT9XfaGP9HyTb5nb2Eml6y"))).AdminAddress.Equals(new Address("0:bd871909f584158689cf86520c54c8efa23559973ddc4b0820cbd12c514832c2")), Is.EqualTo(true));
        Assert.That((await client_lite.Jetton.GetData(new Address("EQDCJL0iQHofcBBvFBHdVG233Ri2V4kCNFgfRT-gqAd3Oc86"))).Content.Symbol, Is.EqualTo("FNZ"));
    }

    [Test]
    public void Test_JettonGetBalance()
    {
        Assert.DoesNotThrowAsync(async Task () => await client.Jetton.GetBalance(new Address("EQARULUYsmJq1RiZ-YiH-IJLcAZUVkVff-KBPwEmmaQGH6aC")));
        Assert.DoesNotThrowAsync(async Task () => await client_lite.Jetton.GetBalance(new Address("EQARULUYsmJq1RiZ-YiH-IJLcAZUVkVff-KBPwEmmaQGH6aC")));
        Assert.ThrowsAsync<Exception>(async Task () => await client.Jetton.GetBalance(new Address("EQAvDfWFG0oYX19jwNDNBBL1rKNT9XfaGP9HyTb5nb2Eml6y")), "Cannot retrieve jetton wallet data.");
    }

    [Test]
    public void Test_JettonGetTransactions()
    {
        Assert.DoesNotThrowAsync(async Task () => await client.Jetton.GetTransactions(new Address("EQARULUYsmJq1RiZ-YiH-IJLcAZUVkVff-KBPwEmmaQGH6aC")));
        Assert.ThrowsAsync<Exception>(async Task () => await client.Jetton.GetTransactions(new Address("EQAvDfWFG0oYX19jwNDNBBL1rKNT9XfaGP9HyTb5nb2Eml6y")), "Cannot retrieve jetton wallet data.");
    }

    [Test]
    public async Task Test_JettonGetWalletAddress()
    {
        Assert.That((await client.Jetton.GetWalletAddress(new Address("EQBlqsm144Dq6SjbPI4jjZvA1hqTIP3CvHovbIfW_t-SCALE"), new Address("EQAEnqomwC3dg323OcdgUsvk3T38VvYawX8q6x38ulfnCn7b"))).Equals(new Address("EQA_d9IqxSQCSuwZIvH0RRSUMvWK4qrvl5ZH_nOHFH7Gxifq")), Is.EqualTo(true));
        Assert.That((await client.Jetton.GetWalletAddress(new Address("EQBlqsm144Dq6SjbPI4jjZvA1hqTIP3CvHovbIfW_t-SCALE"), new Address("EQAEnqomwC3dg323OcdgUsvk3T38VvYawX8q6x38ulfnCn7b"))).Equals(new Address("EQAEnqomwC3dg323OcdgUsvk3T38VvYawX8q6x38ulfnCn7b")), Is.EqualTo(false));
        Assert.That((await client_lite.Jetton.GetWalletAddress(new Address("EQBlqsm144Dq6SjbPI4jjZvA1hqTIP3CvHovbIfW_t-SCALE"), new Address("EQAEnqomwC3dg323OcdgUsvk3T38VvYawX8q6x38ulfnCn7b"))).Equals(new Address("EQA_d9IqxSQCSuwZIvH0RRSUMvWK4qrvl5ZH_nOHFH7Gxifq")), Is.EqualTo(true));
        Assert.That((await client_lite.Jetton.GetWalletAddress(new Address("EQBlqsm144Dq6SjbPI4jjZvA1hqTIP3CvHovbIfW_t-SCALE"), new Address("EQAEnqomwC3dg323OcdgUsvk3T38VvYawX8q6x38ulfnCn7b"))).Equals(new Address("EQAEnqomwC3dg323OcdgUsvk3T38VvYawX8q6x38ulfnCn7b")), Is.EqualTo(false));
    }
}
