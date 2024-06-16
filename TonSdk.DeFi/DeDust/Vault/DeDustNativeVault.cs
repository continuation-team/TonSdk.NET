using System;
using System.Threading.Tasks;
using TonSdk.Client;
using TonSdk.Contracts;
using TonSdk.Core;
using TonSdk.Core.Boc;

namespace TonSdk.DeFi.DeDust.Vault
{
    public class DeDustNativeVault
    {
        public static readonly uint DepositLiquidity = 0xd55e4686;
        public static readonly uint Swap = 0xea06185d;
    
        private readonly Address _address;

        protected DeDustNativeVault(Address address)
        {
            _address = address;
        }
    
        public Address Address => _address;

        public static DeDustNativeVault CreateFromAddress(Address address) 
            => new DeDustNativeVault(address);
    
        public async Task<DeDustReadinessStatus> GetReadinessStatus(TonClient client)
        {
            try
            {
                var state = (await client.GetAddressInformation(_address)).Value.State;
                return state != 
                       AccountState.Active 
                    ? DeDustReadinessStatus.NotDeployed 
                    : DeDustReadinessStatus.Ready;
            }
            catch (Exception e)
            {
                return await GetReadinessStatus(client);
            }
        }

        public static Cell CreateSwapBody(DeDustNativeSwapOptions options)
        {
            return new CellBuilder()
                .StoreUInt(Swap, 32)
                .StoreUInt(options.QueryId ?? SmcUtils.GenerateQueryId(60), 64)
                .StoreCoins(options.Amount)
                .StoreAddress(options.PoolAddress)
                .StoreUInt(0, 1)
                .StoreCoins(options.Limit ?? new Coins(0))
                .StoreOptRef(options.Next == null ? null : Utils.PackSwapSteps(options.Next))
                .StoreRef(Utils.PackSwapParams(options.SwapParams ?? new DeDustSwapParams()))
                .Build();
        }
    }
}