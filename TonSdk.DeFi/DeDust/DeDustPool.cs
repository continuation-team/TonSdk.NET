using System;
using System.Numerics;
using System.Threading.Tasks;
using TonSdk.Client;
using TonSdk.Client.Stack;
using TonSdk.Core;
using TonSdk.Core.Boc;

namespace TonSdk.DeFi.DeDust
{
    public class DeDustPool
    {
        private readonly Address _address;

        protected DeDustPool(Address address)
        {
            _address = address;
        }

        public static DeDustPool CreateFromAddress(Address address)
            => new DeDustPool(address);

        public Address Address => _address;
        
        public async Task<DeDustReadinessStatus> GetReadinessStatus(TonClient client)
        {
            try
            {
                var state = (await client.GetAddressInformation(_address)).Value.State;
                if(state != AccountState.Active)
                    return DeDustReadinessStatus.NotDeployed;
            
                var reserves = await GetReserves(client);
                return reserves[0] > BigInteger.Zero && reserves[1] > BigInteger.Zero 
                    ? DeDustReadinessStatus.Ready 
                    : DeDustReadinessStatus.NotReady;
            }
            catch (Exception e)
            {
                return await GetReadinessStatus(client);
            }
        }

        private async Task<BigInteger[]> GetReserves(ITonClient client)
        {
            try
            {
                var result = await client.RunGetMethod(_address, "get_reserves", Array.Empty<IStackItem>());
                return client.GetClientType() == TonClientType.LITECLIENT 
                    ? new []{ (BigInteger)((VmStackTinyInt)result.Value.StackItems[0]).Value, ((VmStackTinyInt)result.Value.StackItems[1]).Value} 
                    : new []{ (BigInteger)result.Value.Stack[0], (BigInteger)result.Value.Stack[1]};
            }
            catch (Exception e)
            {
                return await GetReserves(client);
            }
            
        }
        
        public async Task<DeDustAsset[]> GetAssets(ITonClient client)
        {
            try
            {
                var result = await client.RunGetMethod(_address, "get_assets", Array.Empty<IStackItem>());
                if (client.GetClientType() == TonClientType.LITECLIENT)
                {
                    var asset1 = ((VmStackSlice)result.Value.StackItems[0]).Value;
                    var asset2 = ((VmStackSlice)result.Value.StackItems[1]).Value;
                    return new []{ DeDustAsset.FromSlice(asset1), DeDustAsset.FromSlice(asset2)};
                }
                else
                {
                    var asset1 = (Cell)result.Value.Stack[0];
                    var asset2 = (Cell)result.Value.Stack[1];
                    return new []{ DeDustAsset.FromSlice(asset1.Parse()), DeDustAsset.FromSlice(asset2.Parse())};
                }
                
            }
            catch (Exception e)
            {
                return await GetAssets(client);
            }
            
        }
    }

    public enum DeDustPoolType : int
    {
        Volatile = 0,
        Stable = 1,
    }
}