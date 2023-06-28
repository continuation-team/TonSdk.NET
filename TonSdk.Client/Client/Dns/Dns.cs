using TonSdk.Core;

namespace TonSdk.Client;

public class Dns
{
    private readonly TonClient client;
    public Dns(TonClient client)
    {
        this.client = client;
    }

    public async Task<Address?> GetWalletAddress(string domain)
    {
        var result = await ResolveAsync(domain, DnsUtils.DNS_CATEGORY_WALLET);
        if (result is not Address) return null;
        return new Address((Address)result);
    }

    public async Task<object?> ResolveAsync(string domain, string? category = null, bool oneStep = false)
    {
        return await DnsUtils.DnsResolve(client, await GetRootDnsAddress(), domain, category, oneStep);
    }

    public async Task<Address> GetRootDnsAddress()
    {
        ConfigParamResult configParamResult = await client.GetConfigParam(4);
        if (configParamResult.Bytes.bits.Length != 256) throw new Exception($"Invalid ConfigParam 4 length {configParamResult.Bytes.bits.Length}");
        string addressHash = configParamResult.Bytes.bits.ToString("hex");
        return new Address($"-1:{addressHash}");
    }
}
