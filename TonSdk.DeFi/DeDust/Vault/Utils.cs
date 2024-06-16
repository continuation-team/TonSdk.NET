using TonSdk.Core;
using TonSdk.Core.Boc;

namespace TonSdk.DeFi.DeDust.Vault
{
    public class Utils
    {
        public static Cell? PackSwapParams(DeDustSwapParams swapParams)
        {
            return new CellBuilder()
                .StoreUInt(swapParams.Deadline ?? 0, 32)
                .StoreAddress(swapParams.RecipientAddress ?? null)
                .StoreAddress(swapParams.RecipientAddress ?? null)
                .StoreOptRef(swapParams.FulfillPayload ?? null)
                .StoreOptRef(swapParams.RejectPayload ?? null)
                .Build();
        }
    
        public static Cell? PackSwapSteps(DeDustSwapStep[] swapSteps)
        {
            Cell? nextRef = null;
            for (int i = swapSteps.Length - 1; i >= 0; i--)
            {
                nextRef = new CellBuilder()
                    .StoreAddress(swapSteps[i].PoolAddress)
                    .StoreUInt(0, 1)
                    .StoreCoins(swapSteps[i].Limit ?? new Coins(0))
                    .StoreOptRef(nextRef).Build();
            }

            return nextRef;
        }
    }

    public struct DeDustJettonSwapOptions
    {
        public Address PoolAddress { get; set; }
        public Coins? Limit { get; set; }
        public DeDustSwapStep[]? Next { get; set; }
        public DeDustSwapParams? SwapParams { get; set; }
    }

    public struct DeDustNativeSwapOptions
    {
        public ulong? QueryId { get; set; }
        public Coins Amount { get; set; }
        public Address PoolAddress { get; set; }
        public Coins? Limit { get; set; }
        public DeDustSwapStep[]? Next { get; set; }
        public DeDustSwapParams? SwapParams { get; set; }
    }

    public struct DeDustSwapStep 
    {
        public Address PoolAddress { get; set; }
        public Coins? Limit { get; set; }
    }

    public struct DeDustSwapParams
    {
        public uint? Deadline { get; set; }
        public Address? RecipientAddress { get; set; }
        public Address? ReferralAddress { get; set; }
        public Cell? FulfillPayload { get; set; }
        public Cell? RejectPayload { get; set; }
    }
}