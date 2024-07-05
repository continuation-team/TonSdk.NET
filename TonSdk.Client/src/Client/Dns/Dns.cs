using System;
using System.Threading.Tasks;
using TonSdk.Core;

namespace TonSdk.Client
{
    public class Dns
    {
        private readonly TonClient client;
        public Dns(TonClient client)
        {
            this.client = client;
        }

        /// <summary>
        /// Retrieves the wallet address associated with the specified domain.
        /// </summary>
        /// <param name="domain">The domain to resolve the wallet address for.</param>
        /// <param name="block">Can be provided to fetch in specific block, requires LiteClient (optional).</param>
        /// <returns>
        /// The wallet address associated with the domain, or null if the domain is not associated with a wallet address.
        /// </returns>
        public async Task<Address> GetWalletAddress(string domain, BlockIdExtended? block = null)
        {
            var result = await ResolveAsync(domain, DnsUtils.DNS_CATEGORY_WALLET, false, block);
            if (!(result is Address)) return null;
            return new Address((Address)result);
        }

        private async Task<object> ResolveAsync(string domain, string category = null, bool oneStep = false, BlockIdExtended? block = null)
        {
            return await DnsUtils.DnsResolve(client, await GetRootDnsAddress(), domain, category, oneStep, block);
        }

        /// <summary>
        /// Retrieves the root DNS address.
        /// </summary>
        /// <returns>The root DNS address.</returns>
        public async Task<Address> GetRootDnsAddress()
        {
            if (client.GetClientType() == TonClientType.LITECLIENT || client.GetClientType() == TonClientType.HTTP_TONCENTERAPIV3)
                return new Address("Ef_lZ1T4NCb2mwkme9h2rJfESCE0W34ma9lWp7-_uY3zXDvq");
            ConfigParamResult? configParamResult = await client.GetConfigParam(4);
            if (configParamResult.Value.Bytes.BitsCount != 256) throw new Exception($"Invalid ConfigParam 4 length {configParamResult.Value.Bytes.BitsCount}");
            return new Address(-1, configParamResult.Value.Bytes.Parse().LoadBytes(32));
        }
    }
}
