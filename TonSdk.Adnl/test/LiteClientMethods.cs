using TonSdk.Core;
using TonSdk.Adnl.LiteClient;
using NUnit.Framework;

public class LiteClientMethods
{
    private const string AdnlHost = "84478511";
    private const int AdnlPort = 19949;
    private const string AdnlPubKey = "n4VDnSCUuSpjnCyUk9e3QOOd6o0ItSWYbTnW3Wnn8wk=";

    private LiteClient _client;
    
    [SetUp]
    public void SetUpTest()
    {
        _client = new LiteClient(AdnlHost, AdnlPort, AdnlPubKey);
    }

    [Test]
    public async Task Test_RunSmcMethod()
    {
        await _client.Connect();
        Address destination = new Address("UQAiqHfH96zGIC38oNRs1AWHRyn3rsjT1zOiAYfjQ4NKN_Pp");
        RunSmcMethodResult result = await _client.RunSmcMethod(destination, "seqno", Array.Empty<byte>(), new RunSmcOptions()
        {
            Result = true
        });
        Assert.That(result.ExitCode == 0, Is.EqualTo(true));
    }
    
    [Test]
    public async Task Test_LookUpBlock()
    {
        await _client.Connect();
        byte[] lookUpBlock = await _client.LookUpBlock(new BlockId(0, 8000000000000000, 40314741));
        Assert.That(lookUpBlock.Length > 0, Is.EqualTo(true));
    }
    
    [Test]
    public async Task Test_ListBlockTransactions()
    {
        await _client.Connect();

        BlockIdExtended block = (await _client.GetMasterChainInfo()).LastBlockId;
        ListBlockTransactionsResult result = await _client.ListBlockTransactions(block, 20);
        ListBlockTransactionsExtendedResult resultExtended = await _client.ListBlockTransactionsExtended(block, 20);
        Assert.That(result != null, Is.EqualTo(true));
        Assert.That(resultExtended != null, Is.EqualTo(true));
    }
    
    [Test]
    public async Task Test_GetTransactions()
    {
        await _client.Connect();
        Address destination = new Address("UQAiqHfH96zGIC38oNRs1AWHRyn3rsjT1zOiAYfjQ4NKN_Pp");
        byte[] transaction = await _client.GetTransactions(20, destination, 42365579000038, "1d128ba7348de8be8103c1cff2bfb14ea9a507f97df460de08fa9873bd572a9b");
        Assert.That(transaction.Length != 0, Is.EqualTo(true));
    }
}