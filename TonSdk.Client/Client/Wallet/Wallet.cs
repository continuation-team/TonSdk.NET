
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

    /// <summary>
    /// Retrieves the sequence number (seqno) of the specified address.
    /// </summary>
    /// <param name="address">The address for which to retrieve the sequence number.</param>
    /// <returns>The sequence number of the address, or null if the retrieval failed or the sequence number is not available.</returns>
    public async Task<uint?> GetSeqno(Address address)
    {
        RunGetMethodResult runGetMethodResult = await client.RunGetMethod(address, "seqno");
        if (runGetMethodResult.ExitCode != 0 && runGetMethodResult.ExitCode != 1) return null;
        return (uint)(BigInteger)runGetMethodResult.Stack[0];
    }

    /// <summary>
    /// Retrieves the subwallet ID of the specified address.
    /// </summary>
    /// <param name="address">The address for which to retrieve the subwallet ID.</param>
    /// <returns>The subwallet ID of the address, or null if the retrieval failed or the subwallet ID is not available.</returns>
    public async Task<uint?> GetSubwalletId(Address address)
    {
        RunGetMethodResult runGetMethodResult = await client.RunGetMethod(address, "get_subwallet_id");
        if (runGetMethodResult.ExitCode != 0 && runGetMethodResult.ExitCode != 1) return null;
        return (uint)(BigInteger)runGetMethodResult.Stack[0];
    }

    /// <summary>
    /// Retrieves the list of plugins associated with the specified address.
    /// </summary>
    /// <param name="address">The address for which to retrieve the plugin list.</param>
    /// <returns>
    /// The list of plugins associated with the address, or null if the retrieval failed or the list is not available.
    /// </returns>
    public async Task<object[]?> GetPluginList(Address address)
    {
        RunGetMethodResult runGetMethodResult = await client.RunGetMethod(address, "get_plugin_list");
        if (runGetMethodResult.ExitCode != 0 && runGetMethodResult.ExitCode != 1) return null;
        Console.WriteLine(JsonConvert.SerializeObject(runGetMethodResult.Stack));
        return runGetMethodResult.Stack;
    }
}
