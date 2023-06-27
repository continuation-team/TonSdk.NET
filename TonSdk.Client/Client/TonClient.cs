using TonSdk.Core;

namespace TonSdk.Client;

public class TonClientParameters : HttpApiParameters { }

public class TonClient : HttpApi
{
    public Wallet Wallet { get; private set; }
    public Jetton Jetton { get; private set; }
    public TonClient(TonClientParameters httpApiParameters) : base(httpApiParameters)
    {
        Wallet = new Wallet(this);
        Jetton = new Jetton(this);
    }

    public async Task<Coins> GetBalance(Address address)
    {
        return (await GetAddressInformation(address)).Balance;
    }

    public async Task<bool> IsContractDeployed(Address address)
    {
        return (await GetAddressInformation(address)).State == AccountState.Active;
    }
}
