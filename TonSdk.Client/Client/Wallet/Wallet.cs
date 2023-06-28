
using Newtonsoft.Json;
using System.Numerics;
using TonSdk.Core;

namespace TonSdk.Client;

public class Wallet
{
    private readonly TonClient client;

    public Wallet(TonClient client)
    {
        this.client = client;
    }

    public async Task<uint?> GetSeqno(Address address)
    {
        RunGetMethodResult runGetMethodResult = await client.RunGetMethod(address, "seqno");
        if (runGetMethodResult.ExitCode != 0 && runGetMethodResult.ExitCode != 1) return null;
        return (uint)(BigInteger)runGetMethodResult.Stack[0];
    }

    public async Task<uint?> GetSubwalletId(Address address)
    {
        RunGetMethodResult runGetMethodResult = await client.RunGetMethod(address, "get_subwallet_id");
        if (runGetMethodResult.ExitCode != 0 && runGetMethodResult.ExitCode != 1) return null;
        return (uint)(BigInteger)runGetMethodResult.Stack[0];
    }

    public async Task<object[]?> GetPluginList(Address address)
    {
        RunGetMethodResult runGetMethodResult = await client.RunGetMethod(address, "get_plugin_list");
        if (runGetMethodResult.ExitCode != 0 && runGetMethodResult.ExitCode != 1) return null;
        Console.WriteLine(JsonConvert.SerializeObject(runGetMethodResult.Stack));
        return runGetMethodResult.Stack;
    }
}
