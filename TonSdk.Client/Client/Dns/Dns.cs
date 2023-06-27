using static System.Runtime.InteropServices.JavaScript.JSType;
using TonSdk.Core.Boc;
using TonSdk.Core;

namespace TonSdk.Client;

public class Dns
{
    private readonly TonClient client;
    public Dns(TonClient client)
    {
        this.client = client;
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
