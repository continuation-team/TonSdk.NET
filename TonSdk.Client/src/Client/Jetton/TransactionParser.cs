using TonSdk.Core.Boc;
using TonSdk.Core;
using System.Numerics;

namespace TonSdk.Client
{

    public class TransactionParser
    {
        public static IJettonTransaction ParseTransaction(TransactionsInformationResult transaction, uint decimals)
        {
            if (transaction.InMsg.MsgData.Text != null && transaction.InMsg.MsgData.Text.Length != 0) return null; // Not a jetton transaction

            CellSlice bodySlice = transaction.InMsg.MsgData.Body!.Parse();

            if (bodySlice.RemainderBits < 32) return null;
            uint operation = (uint)bodySlice.LoadUInt(32);

            try
            {
                switch (operation)
                {
                    case (uint)JettonOperation.TRANSFER:
                        {
                            return ParseTransferTransaction(bodySlice, transaction, decimals);
                        }
                    case (uint)JettonOperation.INTERNAL_TRANSFER:
                        {
                            return ParseInternalTransferTransaction(bodySlice, transaction, decimals);
                        }
                    case (uint)JettonOperation.BURN:
                        {
                            return ParseBurnTransaction(bodySlice, transaction, decimals);
                        }
                    default: return null;
                }
            }
            catch
            {
                return null;
            }
        }

        private static JettonTransfer ParseTransferTransaction(CellSlice bodySlice, TransactionsInformationResult transaction, uint decimals)
        {
            BigInteger queryId = bodySlice.LoadUInt(64);
            Coins amount = bodySlice.LoadCoins((int)decimals);

            Address source = transaction.InMsg.Source ?? null;
            Address destination = bodySlice.LoadAddress();

            bodySlice.LoadAddress();
            bodySlice.SkipBit();

            Coins forwardTonAmount = bodySlice.LoadCoins();
            CellSlice forwardPayload = bodySlice.LoadBit() ? bodySlice.LoadRef().Parse() : bodySlice;

            Cell data = null;
            string comment = null;
            if (forwardPayload.RemainderBits > 0 || forwardPayload.RemainderRefs > 0) data = forwardPayload.RestoreRemainder();
            if (forwardPayload.RemainderBits > 32)
            {
                uint op = (uint)forwardPayload.LoadUInt(32);
                if (op == 0)
                {
                    // TODO: save comment
                }
            }

            JettonTransfer jettonTransfer = new JettonTransfer()
            {
                Operation = JettonOperation.TRANSFER,
                QueryId = (ulong)queryId,
                Amount = amount,
                Source = source,
                Destination = destination,
                Comment = comment,
                Data = data,
                ForwardTonAmount = forwardTonAmount,
                Transaction = transaction
            };

            return jettonTransfer;
        }

        private static JettonTransfer ParseInternalTransferTransaction(CellSlice bodySlice, TransactionsInformationResult transaction, uint decimals)
        {
            BigInteger queryId = bodySlice.LoadUInt(64);
            Coins amount = bodySlice.LoadCoins((int)decimals);

            Address source = bodySlice.LoadAddress();
            Address destination = null;

            bodySlice.LoadAddress();

            Coins forwardTonAmount = bodySlice.LoadCoins();
            CellSlice forwardPayload = bodySlice.LoadBit() ? bodySlice.LoadRef().Parse() : bodySlice;

            Cell data = null;
            string comment = null;
            if (forwardPayload.RemainderBits > 0 || forwardPayload.RemainderRefs > 0) data = forwardPayload.RestoreRemainder();
            if (forwardPayload.RemainderBits > 32)
            {
                uint op = (uint)forwardPayload.LoadUInt(32);
                if (op == 0)
                {
                    // TODO: save comment
                }
            }

            JettonTransfer jettonTransfer = new JettonTransfer()
            {
                Operation = JettonOperation.INTERNAL_TRANSFER,
                QueryId = (ulong)queryId,
                Amount = amount,
                Source = source,
                Destination = destination,
                Comment = comment,
                Data = data,
                ForwardTonAmount = forwardTonAmount,
                Transaction = transaction
            };

            return jettonTransfer;
        }

        private static JettonBurn ParseBurnTransaction(CellSlice bodySlice, TransactionsInformationResult transaction, uint decimals)
        {
            BigInteger queryId = bodySlice.LoadUInt(64);
            Coins amount = bodySlice.LoadCoins((int)decimals);

            JettonBurn jettonBurn = new JettonBurn()
            {
                Operation = JettonOperation.BURN,
                QueryId = (ulong)queryId,
                Amount = amount,
                Transaction = transaction
            };

            return jettonBurn;
        }
    }
}


