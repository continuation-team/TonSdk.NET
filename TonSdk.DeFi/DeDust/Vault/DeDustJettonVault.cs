using System;
using System.Numerics;
using System.Threading.Tasks;
using TonSdk.Client;
using TonSdk.Client.Stack;
using TonSdk.Core;
using TonSdk.Core.Boc;

namespace TonSdk.DeFi.DeDust.Vault
{
    public class DeDustJettonVault
    {
        public static readonly uint DepositLiquidity = 0x40e108d6;
        public static readonly uint Swap = 0xe3a0d482;
    
        private readonly Address _address;

        protected DeDustJettonVault(Address address)
        {
            _address = address;
        }
    
        public Address Address => _address;

        public static DeDustJettonVault CreateFromAddress(Address address) 
            => new DeDustJettonVault(address);
    
        public async Task<DeDustReadinessStatus> GetReadinessStatus(TonClient client)
        {
            try
            {
                var state = (await client.GetAddressInformation(_address)).Value.State;
                if (state != AccountState.Active)
                    return DeDustReadinessStatus.NotDeployed;
        
                var result = await client.RunGetMethod(_address, "is_ready", new IStackItem[] { });
                return (int)(BigInteger)result.Value.Stack[0] == 0 
                    ? DeDustReadinessStatus.NotReady 
                    : DeDustReadinessStatus.Ready;
            }
            catch (Exception e)
            {
                return await GetReadinessStatus(client);
            }
        
        }
    
        public static Cell CreateSwapPayload(DeDustJettonSwapOptions options)
        {
            return new CellBuilder()
                .StoreUInt(Swap, 32)
                .StoreAddress(options.PoolAddress)
                .StoreUInt(0, 1)
                .StoreCoins(options.Limit ?? new Coins(0))
                .StoreOptRef(options.Next == null ? null : Utils.PackSwapSteps(options.Next))
                .StoreRef(Utils.PackSwapParams(options.SwapParams ?? new DeDustSwapParams()))
                .Build();
        }
    }
}