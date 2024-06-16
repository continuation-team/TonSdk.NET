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
                return new []{ (BigInteger)result.Value.Stack[0], (BigInteger)result.Value.Stack[1]};
            }
            catch (Exception e)
            {
                return await GetReserves(client);
            }
            
        }
    }

    public enum DeDustPoolType : int
    {
        Volatile = 0,
        Stable = 1,
    }
}