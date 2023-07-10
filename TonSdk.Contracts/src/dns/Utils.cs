using System.Numerics;
using TonSdk.Core;
using TonSdk.Core.Boc;

namespace TonSdk.Contracts.Dns {
    public static class DnsOperations {
        public const uint CHANGE_DNS_RECORD = 0x1a0f3d8c;
    }

    public static class DnsCategory {
        public const string NEXT_RESOLVER = "dns_next_resolver";
        public const string WALLET = "wallet";
        public const string SITE = "site";
    }

    public static class DnsUtils {
        public static BigInteger CategoryToBigInt(string? category) {
            if (string.IsNullOrEmpty(category)) {
                return BigInteger.Zero;
            }

            return new BitsBuilder(category.Length * 8)
                .StoreString(category)
                .Build().Hash().Parse()
                .LoadUInt(256);
        }

        public static Cell CreateSmartContractAddressRecord(Address smartContractAddress) {
            return new CellBuilder()
                .StoreUInt(0x9fd3,
                    16) // https://github.com/ton-blockchain/ton/blob/7e3df93ca2ab336716a230fceb1726d81bac0a06/crypto/block/block.tlb#L827
                .StoreAddress(smartContractAddress)
                .StoreUInt(0, 8)
                .Build();
        }

        public static Cell CreateAdnlAddressRecord(BigInteger adnlAddress) {
            return new CellBuilder()
                .StoreUInt(0xad01,
                    16) // https://github.com/ton-blockchain/ton/blob/7e3df93ca2ab336716a230fceb1726d81bac0a06/crypto/block/block.tlb#L821
                .StoreUInt(adnlAddress, 256)
                .StoreUInt(0, 8)
                .Build();
        }

        public static Cell CreateNextResolverRecord(Address smartContractAddress) {
            return new CellBuilder()
                .StoreUInt(0xba93,
                    16) // https://github.com/ton-blockchain/ton/blob/7e3df93ca2ab336716a230fceb1726d81bac0a06/crypto/block/block.tlb#L819
                .StoreAddress(smartContractAddress)
                .Build();
        }
    }
}
