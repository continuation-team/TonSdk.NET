using System;
using System.Collections.Generic;
using TonSdk.Client;
using TonSdk.Core;
using TonSdk.Core.Block;
using TonSdk.Core.Boc;

namespace TonSdk.Client.Parsers
{
    public class Utils
    {
        public static TransactionsInformationResult ParseTransactionCell(Cell transaction, int workchain = 0)
        {
            var tx = new TransactionsInformationResult();
            var slice = transaction.Parse();
            slice.SkipBits(4);
            
            tx.Address = new Address(workchain, slice.LoadBytes(32));
            tx.TransactionId.Hash = transaction.Hash.ToString();
            tx.TransactionId.Lt = (ulong)slice.LoadUInt(64);

            tx.PrevTransactionId = new TransactionId()
            {
                Hash = Convert.ToBase64String(slice.LoadBytes(32)),
                Lt = (ulong)slice.LoadUInt(64)
            };
            
            tx.UTime = (uint)slice.LoadUInt(32);
            tx.OutMsgCount = (int)slice.LoadUInt(15);

            tx.OrigAccountStatus = ConvertAccountStateFromBits((byte)slice.LoadUInt(2));
            tx.EndAccountStatus = ConvertAccountStateFromBits((byte)slice.LoadUInt(2));

            var firstRefSlice = slice.LoadRef().Parse();
            var inMsg = firstRefSlice.LoadOptRef();
            if (inMsg != null)
                tx.InMsg = ParseRawMessage(inMsg);
            
            var hmOptions = new HashmapOptions<uint, CellSlice>()
            {
                KeySize = 15,
                Serializers = new HashmapSerializers<uint, CellSlice>
                {
                    Key = k => new BitsBuilder(15).StoreUInt(k, 15).Build(),
                    Value = v => new CellBuilder().Build()
                },
                Deserializers = new HashmapDeserializers<uint, CellSlice>
                {
                    Key = k => (uint)k.Parse().LoadUInt(15),
                    Value = v => v.Parse()
                }
            };
            var outMsgMap = firstRefSlice.LoadDict(hmOptions);
            var msgList = new List<RawMessage>();

            for (uint i = 0; i < outMsgMap.Count; i++)
            {
                var msg = outMsgMap.Get(i);
                if (msg != null)
                    msgList.Add(ParseRawMessage(msg.LoadRef()));
            }

            tx.OutMsgs = msgList.ToArray();
            return tx;
        }

        public static RawMessage ParseRawMessage(Cell msgCell, bool ignoreCheck = false)
        {
            var message = new RawMessage
            {
                Hash = msgCell.Hash.ToString()
            };
            
            MessageX msg;
            try
            {
                msg = MessageX.Parse(msgCell.Parse());
            }
            catch (AddressTypeNotSupportedError e)
            {
                return message;
            }
            
            
                
            switch (msg.Data.Info.Data)
            {
                case IntMsgInfoOptions msgIntIn:
                {
                    var cmnMsgInfo = msg.Data.Info.Cell.Parse();
                    cmnMsgInfo.LoadBit();
                    cmnMsgInfo.LoadBit();
                    cmnMsgInfo.LoadBit();
                    cmnMsgInfo.LoadBit();
                    message.MsgData.InitState = msg.Data.StateInit == null ? "" : msg.Data.StateInit.Cell.ToString("base64");
                    message.MsgData.Body = msg.Data.Body;
                    message.OpCode = msg.Data.Body != null && msg.Data.Body.BitsCount >= 32
                        ? $"0x{(uint)msg.Data.Body.Parse().LoadUInt(32):x}"
                        : "";
                    message.Source = cmnMsgInfo.LoadAddress();
                    message.Destination = cmnMsgInfo.LoadAddress();

                    message.Value = msgIntIn.Value ?? new Coins(0);
                    message.IhrFee = msgIntIn.IhrFee ?? new Coins(0);
                    message.FwdFee = msgIntIn.FwdFee ?? new Coins(0);
                    message.CreatedLt = msgIntIn.CreatedLt ?? 0;
                    break;
                }
                case ExtInMsgInfoOptions msgExtIn:
                    message.Source = msgExtIn.Src;
                    message.Destination = msgExtIn.Dest;
                    message.MsgData.InitState = msg.Data.StateInit == null ? "" : msg.Data.StateInit.Cell.ToString("base64");
                    message.MsgData.Body = msg.Data.Body;
                    break;
                case ExtOutMsgInfoOptions msgExtOut:
                    message.Source = msgExtOut.Src;
                    message.Destination = msgExtOut.Dest;
                    message.CreatedLt = msgExtOut.CreatedLt;
                    break;
            }

            return message;
        }
        
        private static AccountState ConvertAccountStateFromBits(byte prefix)
        {
            return prefix switch
            {
                0b00 => AccountState.Uninit,
                0b01 => AccountState.Frozen,
                0b10 => AccountState.Active,
                0b11 => AccountState.NonExist,
                _ => default
            };
        }
    }
}