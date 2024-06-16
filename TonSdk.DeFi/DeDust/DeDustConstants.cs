using TonSdk.Core;

namespace TonSdk.DeFi.DeDust
{
    public class DeDustConstants
    {
        public static readonly Address MainNetFactory = new Address("EQBfBWT7X2BHg9tXAxzhz2aKiNTU1tpt5NsiK0uSDW_YAJ67");
    }

    public enum  DeDustAssetType 
    {
        Native = 0b0000,
        Jetton = 0b0001,
    }

    public enum DeDustReadinessStatus 
    {
        NotDeployed,
        NotReady,
        Ready,
    }
}