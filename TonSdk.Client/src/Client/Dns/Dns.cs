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
        /// <returns>
        /// The wallet address associated with the domain, or null if the domain is not associated with a wallet address.
        /// </returns>
        public async Task<Address> GetWalletAddress(string domain)
        {
            var result = await ResolveAsync(domain, DnsUtils.DNS_CATEGORY_WALLET);
            if (!(result is Address)) return null;
            return new Address((Address)result);
        }

        private async Task<object> ResolveAsync(string domain, string category = null, bool oneStep = false)
        {
            return await DnsUtils.DnsResolve(client, await GetRootDnsAddress(), domain, category, oneStep);
        }

        /// <summary>
        /// Retrieves the root DNS address.
        /// </summary>
        /// <returns>The root DNS address.</returns>
        public async Task<Address> GetRootDnsAddress()
        {
            ConfigParamResult configParamResult = await client.GetConfigParam(4);
            if (configParamResult.Bytes.BitsCount != 256) throw new Exception($"Invalid ConfigParam 4 length {configParamResult.Bytes.BitsCount}");
            return new Address(-1, configParamResult.Bytes.Parse().LoadUInt(256));
        }
    }
}
