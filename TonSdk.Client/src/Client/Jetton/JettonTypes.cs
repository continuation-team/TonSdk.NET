using TonSdk.Core;
using TonSdk.Core.Boc;

namespace TonSdk.Client
{

    public interface IJettonTransaction
    {
        JettonOperation Operation { get; set; }
        ulong QueryId { get; set; }
        Coins Amount { get; set; }
        TransactionsInformationResult Transaction { get; set; }
    }

    public struct JettonTransfer : IJettonTransaction
    {
        public JettonOperation Operation { get; set; }
        public ulong QueryId { get; set; }
        public Coins Amount { get; set; }
        public Coins ForwardTonAmount { get; set; }
        public Address Source { get; set; }
        public Address Destination { get; set; }
        public string Comment { get; set; }
        public Cell Data { get; set; }
        public TransactionsInformationResult Transaction { get; set; }
    }

    public struct JettonBurn : IJettonTransaction
    {
        public JettonOperation Operation { get; set; }
        public ulong QueryId { get; set; }
        public Coins Amount { get; set; }
        public TransactionsInformationResult Transaction { get; set; }
    }

    public enum JettonOperation : long
    {
        TRANSFER = 0xf8a7ea5,
        TRANSFER_NOTIFICATION = 0x7362d09c,
        INTERNAL_TRANSFER = 0x178d4519,
        EXCESSES = 0xd53276db,
        BURN = 0x595f07bc,
        BURN_NOTIFICATION = 0x7bdd97de,
    }
}