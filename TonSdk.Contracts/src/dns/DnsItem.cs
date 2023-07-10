using System.Numerics;
using TonSdk.Core;
using TonSdk.Core.Boc;

namespace TonSdk.Contracts.Dns {
    public struct DnsChangeContentOptions {
        public string Category;
        public Cell? Value;
        public ulong? QueryId;
    }

    public class DnsItem {
        public static Cell CreateChangeContentRequest(DnsChangeContentOptions opt) {
            var builder = new CellBuilder()
                .StoreUInt(DnsOperations.CHANGE_DNS_RECORD, 32)
                .StoreUInt(opt.QueryId ?? SmcUtils.GenerateQueryId(60), 64)
                .StoreUInt(DnsUtils.CategoryToBigInt(opt.Category), 64);
            if (opt.Value != null) {
                builder.StoreRef(opt.Value);
            }

            return builder.Build();
        }

        public static Cell CreateChangeWalletRequest(Address address, ulong? queryId = null) {
            return CreateChangeContentRequest(new DnsChangeContentOptions() {
                Category = DnsCategory.WALLET,
                Value = DnsUtils.CreateSmartContractAddressRecord(address),
                QueryId = queryId,
            });
        }

        public static Cell CreateChangeSiteRequest(BigInteger adnlAddress, ulong? queryId = null) {
            return CreateChangeContentRequest(new DnsChangeContentOptions() {
                Category = DnsCategory.SITE,
                Value = DnsUtils.CreateAdnlAddressRecord(adnlAddress),
                QueryId = queryId,
            });
        }

        public static Cell CreateChangeNextResolverRequest(Address address, ulong? queryId = null) {
            return CreateChangeContentRequest(new DnsChangeContentOptions() {
                Category = DnsCategory.NEXT_RESOLVER,
                Value = DnsUtils.CreateNextResolverRecord(address),
                QueryId = queryId,
            });
        }
    }
}
