using static System.Runtime.InteropServices.JavaScript.JSType;
using TonSdk.Core.Boc;
using TonSdk.Core;
using Org.BouncyCastle.Utilities;

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

    //async getRootDnsAddress(): Promise<Address> {
    //    const cell = await this.client.getConfigParam({ configId: 4 });
    //    if (cell.bits.length !== 256) throw new Error(`Invalid ConfigParam 4 length ${ cell.bits.length}`);
    //    const addrHash = bitsToHex(cell.bits);
    //    return new Address(`-1:${ addrHash}`);
    //}

    //async resolve(
    //domain: string,
    //        category: string | undefined,
    //        oneStep = false,
    //    ): Promise < Cell | Address | MsgAddressExt | bigint | null > {
    //    return dnsResolve(this.client, await this.getRootDnsAddress(), domain, category, oneStep);
    //}

    //async getWalletAddress(domain: string): Promise < Address | MsgAddressExt > {
    //    const result = await this.resolve(domain, DNS_CATEGORY_WALLET);
    //    if (!(result instanceof Address)) return Address.NONE;
    //    return new Address(result, { bounceable: true });
    //}
}
