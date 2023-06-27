
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Transactions;
using System;
using TonSdk.Core.Boc;
using System.Drawing;
using System.Xml.Linq;
using TonSdk.Client;
using TonSdk.Core;
using System.Numerics;

namespace TonSdk.Client;

public class TransactionParser
{
    public static IJettonTransaction? ParseTransaction(TransactionsInformationResult transaction, uint decimals)
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
                        return null;
                    }
                case (uint)JettonOperation.INTERNAL_TRANSFER:
                    {
                        return null;
                    }
                case (uint)JettonOperation.BURN:
                    {
                        return null;
                    }
                default: return null;
            }
        }
        catch
        {
            return null;
        }
    }

    //private JettonTransfer ParseTransferTransaction(CellSlice bodySlice, TransactionsInformationResult transaction, uint decimals)
    //{
    //    BigInteger queryId = bodySlice.LoadUInt(64);
    //    Coins amount = bodySlice.LoadCoins((int)decimals);

    //    Address? source = transaction.InMsg.Source ?? null;
    //    Address? destination = bodySlice.LoadAddress();

    //    bodySlice.LoadAddress();
    //    bodySlice.SkipBit();

    //    Coins forwardTonAmount = bodySlice.LoadCoins();
    //    CellSlice forwardPayload = bodySlice.LoadBit() ? bodySlice.LoadRefs(1)[0].Parse() : bodySlice;

    //    string? data;
    //    if(forwardPayload.Bits.Length > 0)
    //    {
    //        data = new CellBuilder()
    //    }
    //}
}

//    parseTransferTransaction(
//        bodySlice: Slice,
//        transaction: TonTransaction,
//        decimals: number,
//    ) : JettonTransfer {
//        const queryId = bodySlice.loadBigUint(64);
//        const amount = bodySlice.loadCoins(decimals);

//        const source = transaction.inMessage?.source ?? null;
//        const destination = bodySlice.loadAddress() as Address;

//        bodySlice.loadAddress(); // response_destination
//            bodySlice.skipDict(); // custom_payload

//            const forwardTonAmount = bodySlice.loadCoins();
//        const forwardPayload = bodySlice.loadBit()
//            ? bodySlice.loadRef().parse()
//            : bodySlice;

//        let comment;
//        let data;

//            if (forwardPayload.bits.length > 0) {
//                data = new Builder().storeSlice(forwardPayload).cell()
//                    .toString('base64', { has_index: false });
//            }
//            if (forwardPayload.bits.length >= 32) {
//                const op = forwardPayload.loadUint(32);
//                if (op === 0) {
//                    comment = forwardPayload.loadText();
//                }
//            }

//            return {
//    operation: JettonOperation.TRANSFER,
//                queryId,
//                amount,
//                source,
//                destination,
//                comment,
//                data,
//                forwardTonAmount,
//                transaction,
//            };
//        },
//}


