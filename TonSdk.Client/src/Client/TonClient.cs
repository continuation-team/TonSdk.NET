using System.Threading.Tasks;
using TonSdk.Core;

namespace TonSdk.Client
{
    public class TonClientParameters : HttpApiParameters
    {
    }

    public class TonClient : HttpApi
    {
        public Wallet Wallet { get; private set; }
        public Jetton Jetton { get; private set; }
        public Nft Nft { get; private set; }
        public Dns Dns { get; private set; }

        /// <summary>
        /// Initializes a new instance of the TonClient class with the specified options.
        /// </summary>
        /// <param name="httpApiParameters">The Ton Client parameters.</param>
        public TonClient(TonClientParameters httpApiParameters) : base(httpApiParameters)
        {
            Wallet = new Wallet(this);
            Jetton = new Jetton(this);
            Nft = new Nft(this);
            Dns = new Dns(this);
        }

        /// <summary>
        /// Retrieves the balance of the specified address.
        /// </summary>
        /// <param name="address">The address to retrieve the balance for.</param>
        /// <returns>The task result contains the balance as a Coins instance.</returns>
        public async Task<Coins> GetBalance(Address address)
        {
            return (await GetAddressInformation(address)).Balance;
        }

        /// <summary>
        /// Checks if a contract is deployed at the specified address.
        /// </summary>
        /// <param name="address">The address to check.</param>
        /// <returns>The task result indicates whether a contract is deployed (true) or not (false) at the specified address.</returns>
        public async Task<bool> IsContractDeployed(Address address)
        {
            return (await GetAddressInformation(address)).State == AccountState.Active;
        }
    }
}