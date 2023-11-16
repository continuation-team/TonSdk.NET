﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Globalization;
using System.Linq;
using System.Numerics;
using TonSdk.Core;
using TonSdk.Core.Boc;
using static TonSdk.Client.Transformers;

namespace TonSdk.Client
{
    internal static class Transformers
    {
        internal static string[] PackRequestStack(object element)
        {
            if (element is Cell)
            {
                return new string[] { "tvm.Cell", ((Cell)element).ToString() };
            }

            if (element is BigInteger || element is uint || element is int || element is long || element is ulong)
            {
                return new string[] { "num", element.ToString()! };
            }

            if (element is Coins)
            {
                return new string[] { "num", ((Coins)element).ToNano() };
            }

            if (element is CellSlice)
            {
                return new string[] { "tvm.Slice", ((CellSlice)element).RestoreRemainder().ToString()! };
            }

            if (element is Address)
            {
                return new string[] { "tvm.Slice", ((Address)element).ToBOC() };
            }

            // TODO: Message Layout
            throw new Exception($"Unknown type of element: {element}");
        }

        // in
        internal struct EmptyBody : IRequestBody
        {
        }

        internal struct InShardsBody : IRequestBody
        {
            public long seqno { get; set; }

            internal InShardsBody(long seqno) => this.seqno = seqno;
        }

        internal struct InBlockTransactions : IRequestBody
        {
            public int workchain { get; set; }
            public long shard { get; set; }
            public long seqno { get; set; }
            public string root_hash { get; set; }
            public string file_hash { get; set; }
            public ulong? after_lt { get; set; }
            public string after_hash { get; set; }
            public int? count { get; set; }

            internal InBlockTransactions(
                int workchain,
                long shard,
                long seqno,
                string root_hash = null,
                string file_hash = null,
                ulong? after_lt = null,
                string after_hash = null,
                int? count = null)
            {
                this.workchain = workchain;
                this.shard = shard;
                this.seqno = seqno;
                this.root_hash = root_hash;
                this.file_hash = file_hash;
                this.after_lt = after_lt;
                this.after_hash = after_hash;
                this.count = count;
            }
        }

        internal struct InAdressInformationBody : IRequestBody
        {
            public string address { get; set; }

            internal InAdressInformationBody(string address) => this.address = address;
        }

        internal struct InTransactionsBody : IRequestBody
        {
            public string address;

            public int limit;

            public ulong lt;

            public string hash;

            public ulong to_lt;

            public bool archival;
        }

        internal struct InRunGetMethodBody : IRequestBody
        {
            public string address;
            public string method;
            public string[][] stack;

            internal InRunGetMethodBody(string address, string method, string[][] stack)
            {
                this.address = address;
                this.method = method;
                this.stack = stack;
            }
        }

        internal struct InSendBocBody : IRequestBody
        {
            public string boc;
        }

        internal struct InEstimateFeeBody : IRequestBody
        {
            public string address;
            public string body;
            public string init_code;
            public string init_data;
            public bool ignore_chksig;
        }

        internal struct InGetConfigParamBody : IRequestBody
        {
            public int config_id;
            public int seqno;
        }
        
        internal interface OutResult
        {
        }

        internal struct RootAddressInformation
        {
            [JsonProperty("ok")] public bool Ok { get; set; }
            [JsonProperty("result")] public OutAddressInformationResult Result { get; set; }
            [JsonProperty("id")] public string Id { get; set; }
            [JsonProperty("jsonrpc")] public string JsonRPC { get; set; }
        }
        
        internal struct RootWalletInformation
        {
            [JsonProperty("ok")] public bool Ok { get; set; }

            [JsonProperty("result")]
            public OutWalletInformationResult Result { get; set; }
        }

        internal struct RootMasterchainInformation
        {
            [JsonProperty("ok")] public bool Ok { get; set; }
            [JsonProperty("result")] public OutMasterchanInformationResult Result { get; set; }
            [JsonProperty("id")] public string Id { get; set; }
            [JsonProperty("jsonrpc")] public string JsonRPC { get; set; }
        }

        internal struct RootShardsInformation
        {
            [JsonProperty("ok")] public bool Ok { get; set; }
            [JsonProperty("result")] public OutShardsInformationResult Result { get; set; }
            [JsonProperty("id")] public string Id { get; set; }
            [JsonProperty("jsonrpc")] public string JsonRPC { get; set; }
        }

        internal struct RootBlockTransactions
        {
            [JsonProperty("ok")] public bool Ok { get; set; }
            [JsonProperty("result")] public OutBlockTransactionsResult Result { get; set; }
            [JsonProperty("id")] public string Id { get; set; }
            [JsonProperty("jsonrpc")] public string JsonRPC { get; set; }
        }

        internal struct RootTransactions
        {
            [JsonProperty("ok")] public bool Ok { get; set; }
            [JsonProperty("result")] public OutTransactionsResult[] Result { get; set; }
            [JsonProperty("id")] public string Id { get; set; }
            [JsonProperty("jsonrpc")] public string JsonRPC { get; set; }
        }

        internal struct RootRunGetMethod
        {
            [JsonProperty("ok")] public bool Ok { get; set; }
            [JsonProperty("result")] public OutRunGetMethod Result { get; set; }
            [JsonProperty("id")] public string Id { get; set; }
            [JsonProperty("jsonrpc")] public string JsonRPC { get; set; }
        }

        internal struct RootSendBoc
        {
            [JsonProperty("ok")] public bool Ok { get; set; }
            [JsonProperty("result")] public SendBocResult Result { get; set; }
            [JsonProperty("id")] public string Id { get; set; }
            [JsonProperty("jsonrpc")] public string JsonRPC { get; set; }
        }

        internal struct RootEstimateFee
        {
            [JsonProperty("ok")] public bool Ok { get; set; }
            [JsonProperty("result")] public EstimateFeeResult Result { get; set; }
            [JsonProperty("id")] public string Id { get; set; }
            [JsonProperty("jsonrpc")] public string JsonRPC { get; set; }
        }

        internal struct RootGetConfigParam
        {
            [JsonProperty("ok")] public bool Ok { get; set; }
            [JsonProperty("result")] public OutGetConfigParamResult Result { get; set; }
            [JsonProperty("id")] public string Id { get; set; }
            [JsonProperty("jsonrpc")] public string JsonRPC { get; set; }
        }

        internal struct OutGetConfigParamResult
        {
            [JsonProperty("config")] public OutConfigParamResult Config;
        }

        internal struct OutConfigParamResult
        {
            [JsonProperty("bytes")] public string Bytes;
        }

        internal struct OutAddressInformationResult
        {
            [JsonProperty("state")] public string State;
            [JsonProperty("balance")] public string Balance;
            [JsonProperty("code")] public string Code;
            [JsonProperty("data")] public string Data;
            [JsonProperty("last_transaction_id")] public TransactionId LastTransactionId;
            [JsonProperty("block_id")] public BlockIdExternal BlockId;
            [JsonProperty("frozen_hash")] public string FrozenHash;
            [JsonProperty("sync_utime")] public long SyncUtime;
        }
        
        internal struct OutWalletInformationResult
        {
            [JsonProperty("wallet")] public string IsWallet;
            [JsonProperty("balance")] public string Balance;
            [JsonProperty("account_state")] public string State;
            [JsonProperty("wallet_type")] public string WalletType;
            [JsonProperty("seqno")] public string Seqno;
            [JsonProperty("last_transaction_id")] public TransactionId LastTransactionId;
            [JsonProperty("wallet_id")] public string WalletId;
        }

        internal struct OutMasterchanInformationResult
        {
            [JsonProperty("last")] public BlockIdExternal LastBlock;
            [JsonProperty("init")] public BlockIdExternal InitBlock;
            [JsonProperty("state_root_hash")] public string StateRootHash;
        }

        internal struct OutShardsInformationResult
        {
            [JsonProperty("shards")] public BlockIdExternal[] Shards;
        }

        internal struct OutBlockTransactionsResult
        {
            [JsonProperty("id")] public BlockIdExternal Id;
            [JsonProperty("req_count")] public int ReqCount;
            [JsonProperty("incomplete")] public bool Incomplete;
            [JsonProperty("transactions")] public ShortTransactionsResult[] Transactions;
        }

        

        internal struct OutTransactionsResult
        {
            [JsonProperty("utime")] public long Utime;
            [JsonProperty("data")] public string Data;
            [JsonProperty("transaction_id")] public TransactionId TransactionId;
            [JsonProperty("fee")] public string Fee;
            [JsonProperty("storage_fee")] public string StorageFee;
            [JsonProperty("other_fee")] public string OtherFee;
            [JsonProperty("in_msg")] public OutRawMessage InMsg;
            [JsonProperty("out_msgs")] public OutRawMessage[] OutMsgs;
        }

        internal struct OutRawMessage
        {
            [JsonProperty("source")] public string Source;
            [JsonProperty("destination")] public string Destination;
            [JsonProperty("value")] public string Value;
            [JsonProperty("fwd_fee")] public string FwdFee;
            [JsonProperty("ihr_fee")] public string IhrFee;
            [JsonProperty("created_lt")] public long CreaterLt;
            [JsonProperty("body_hash")] public string BodyHash;
            [JsonProperty("msg_data")] public OutRawMessageData MsgData;
            [JsonProperty("message")] public string Message;
        }

        internal struct OutRawMessageData
        {
            [JsonProperty("text")] public string Text;
            [JsonProperty("body")] public string Body;
            [JsonProperty("init_state")] public string InitState;
        }
    }
    
    public struct TransactionId
    {
        [JsonProperty("lt")] public ulong Lt;
        [JsonProperty("hash")] public string Hash;
    }

    public struct BlockIdExternal
    {
        [JsonProperty("workchain")] public int Workchain;
        [JsonProperty("shard")] public long Shard;
        [JsonProperty("seqno")] public long Seqno;
        [JsonProperty("hash")] public string Hash;
        [JsonProperty("root_hash")] public string RootHash;
        [JsonProperty("file_hash")] public string FileHash;
    }
    
    public struct ShortTransactionsResult
    {
        [JsonProperty("mode")] public int Mode;
        [JsonProperty("account")] public string Account;
        [JsonProperty("lt")] public ulong Lt;
        [JsonProperty("hash")] public string Hash;
    }

    public struct AddressInformationResult
    {
        public AccountState State;
        public Coins Balance;
        public Cell Code;
        public Cell Data;
        public TransactionId LastTransactionId;
        public BlockIdExternal BlockId;
        public string FrozenHash;
        public long SyncUtime;

        internal AddressInformationResult(OutAddressInformationResult outAddressInformationResult)
        {
            switch (outAddressInformationResult.State)
            {
                case "active":
                    {
                        State = AccountState.Active;
                        break;
                    }
                case "frozen":
                    {
                        State = AccountState.Frozen;
                        break;
                    }
                case "uninitialized":
                    {
                        State = AccountState.Uninit;
                        break;
                    }
                default:
                    {
                        State = AccountState.NonExist;
                        break;
                    }
            }

            Balance = new Coins(outAddressInformationResult.Balance, new CoinsOptions(true, 9));
            Code = outAddressInformationResult.Code == "" ? null : Cell.From(outAddressInformationResult.Code);
            Data = outAddressInformationResult.Data == "" ? null : Cell.From(outAddressInformationResult.Data);
            LastTransactionId = outAddressInformationResult.LastTransactionId;
            BlockId = outAddressInformationResult.BlockId;
            FrozenHash = outAddressInformationResult.FrozenHash;
            SyncUtime = outAddressInformationResult.SyncUtime;
        }
    }

    public struct MasterchainInformationResult
    {
        public BlockIdExternal LastBlock;
        public BlockIdExternal InitBlock;
        public string StateRootHash;

        internal MasterchainInformationResult(OutMasterchanInformationResult outAddressInformationResult)
        {
            LastBlock = outAddressInformationResult.LastBlock;
            InitBlock = outAddressInformationResult.InitBlock;
            StateRootHash = outAddressInformationResult.StateRootHash;
        }
    }

    public struct ShardsInformationResult
    {
        public BlockIdExternal[] Shards;

        internal ShardsInformationResult(OutShardsInformationResult outShardsInformationResult)
        {
            Shards = outShardsInformationResult.Shards;
        }
    }

    public struct BlockTransactionsResult
    {
        public BlockIdExternal Id;
        public int ReqCount;
        public bool Incomplete;
        public ShortTransactionsResult[] Transactions;

        internal BlockTransactionsResult(OutBlockTransactionsResult outBlockTransactionsResult)
        {
            Id = outBlockTransactionsResult.Id;
            ReqCount = outBlockTransactionsResult.ReqCount;
            Incomplete = outBlockTransactionsResult.Incomplete;
            Transactions = outBlockTransactionsResult.Transactions;
        }
    }

    public struct TransactionsInformationResult
    {
        public long Utime;
        public Cell Data;
        public TransactionId TransactionId;
        public Coins Fee;
        public Coins StorageFee;
        public Coins OtherFee;
        public RawMessage InMsg;
        public RawMessage[] OutMsgs;

        internal TransactionsInformationResult(OutTransactionsResult outTransactionsResult)
        {
            Utime = outTransactionsResult.Utime;
            Data = Cell.From(outTransactionsResult.Data);
            TransactionId = outTransactionsResult.TransactionId;
            Fee = new Coins(outTransactionsResult.Fee, new CoinsOptions(true, 9));
            StorageFee = new Coins(outTransactionsResult.StorageFee, new CoinsOptions(true, 9));
            OtherFee = new Coins(outTransactionsResult.OtherFee, new CoinsOptions(true, 9));
            InMsg = new RawMessage(outTransactionsResult.InMsg);

            OutMsgs = new RawMessage[outTransactionsResult.OutMsgs.Length];
            for (int i = 0; i < outTransactionsResult.OutMsgs.Length; i++)
            {
                OutMsgs[i] = new RawMessage(outTransactionsResult.OutMsgs[i]);
            }
        }
    }

    public struct ConfigParamResult
    {
        public Cell Bytes;

        internal ConfigParamResult(OutConfigParamResult outConfigParamResult)
        {
            Bytes = Cell.From(outConfigParamResult.Bytes);
        }
    }

    public struct RawMessage
    {
        public Address Source;
        public Address Destination;
        public Coins Value;
        public Coins FwdFee;
        public Coins IhrFee;
        public long CreaterLt;
        public string BodyHash;
        public RawMessageData MsgData;
        public string Message;

        internal RawMessage(OutRawMessage outRawMessage)
        {
            Source = outRawMessage.Source != null && outRawMessage.Source.Length != 0
                ? new Address(outRawMessage.Source)
                : null;
            Destination = new Address(outRawMessage.Destination);
            Value = new Coins(outRawMessage.Value, new CoinsOptions(true, 9));
            FwdFee = new Coins(outRawMessage.FwdFee, new CoinsOptions(true, 9));
            IhrFee = new Coins(outRawMessage.IhrFee, new CoinsOptions(true, 9));
            CreaterLt = outRawMessage.CreaterLt;
            BodyHash = outRawMessage.BodyHash;
            MsgData = new RawMessageData(outRawMessage.MsgData);
            Message = outRawMessage.Message;
        }
    }

    public struct RawMessageData
    {
        public string Text;
        public Cell Body;
        public string InitState;

        internal RawMessageData(OutRawMessageData outRawMessageData)
        {
            Text = outRawMessageData.Text ?? null;
            Body = outRawMessageData.Body != null ? Cell.From(outRawMessageData.Body) : null;
            InitState = outRawMessageData.InitState ?? null;
        }
    }

    internal struct OutRunGetMethod
    {
        [JsonProperty("gas_used")] public int GasUsed;
        [JsonProperty("stack")] public object[][] Stack;
        [JsonProperty("exit_code")] public int ExitCode;
    }

    public struct RunGetMethodResult
    {
        public int GasUsed;
        public object[] Stack;
        public int ExitCode;

        internal RunGetMethodResult(OutRunGetMethod outRunGetMethod)
        {
            GasUsed = outRunGetMethod.GasUsed;
            ExitCode = outRunGetMethod.ExitCode;
            Stack = new object[outRunGetMethod.Stack.Length];
            for (int i = 0; i < outRunGetMethod.Stack.Length; i++)
            {
                Stack[i] = ParseStackItem(outRunGetMethod.Stack[i]);
            }
        }

        internal static object ParseObject(JObject x)
        {
            string typeName = x["@type"].ToString();
            switch (typeName)
            {
                case "tvm.list":
                case "tvm.tuple":
                    object[] list = new object[x["elements"].Count()];
                    for (int c = 0; c < x["elements"].Count(); c++)
                    {
                        list[c] = ParseObject((JObject)x["elements"][c]);
                    }

                    return list;
                case "tvm.cell":
                    return Cell.From(x["bytes"].ToString()); // Cell.From should be defined elsewhere in your code.
                case "tvm.stackEntryCell":
                    return ParseObject((JObject)x["cell"]);
                case "tvm.stackEntryTuple":
                    return ParseObject((JObject)x["tuple"]);
                case "tvm.stackEntryNumber":
                    return ParseObject((JObject)x["number"]);
                case "tvm.numberDecimal":
                    string number = x["number"].ToString();
                    return BigInteger.Parse(number);
                default:
                    throw new Exception($"Unknown type {typeName}");
            }
        }

        internal static object ParseStackItem(object[] item)
        {
            string type = item[0].ToString();
            object value = item[1];

            switch (type)
            {
                case "num":
                    {
                        string valueStr = value as string;
                        if (valueStr == null)
                            throw new Exception("Expected a string value for 'num' type.");

                        bool isNegative = valueStr[0] == '-';
                        string slice = isNegative ? valueStr.Substring(3) : valueStr.Substring(2);
                        BigInteger x = BigInteger.Parse(slice, NumberStyles.HexNumber);

                        return isNegative ? 0 - x : x;
                    }
                case "cell":
                    {
                        if (value is JObject jObject && jObject["bytes"] is JValue jValue)
                        {
                            return Cell.From((string)jValue.Value);
                        }
                        else
                        {
                            throw new Exception("Expected a JObject value for 'cell' type.");
                        }
                    }
                case "list":
                case "tuple":
                    {
                        if (value is JObject jObject)
                        {
                            return ParseObject(jObject);
                        }
                        else
                        {
                            throw new Exception("Expected a JObject value for 'list' or 'tuple' type.");
                        }
                    }
                default:
                    {
                        throw new Exception("Unknown type " + type);
                    }
            }
        }
    }
    
    public struct WalletInformationResult
    {
        public bool IsWallet;
        public Coins Balance;
        public AccountState State;
        public string? WalletType;
        public long? Seqno;
        public TransactionId LastTransactionId;
        public long? WalletId;

        internal WalletInformationResult(OutWalletInformationResult walletInformationResult)
        {
            IsWallet = bool.Parse(walletInformationResult.IsWallet);
            if (IsWallet)
            {
                WalletType = walletInformationResult.WalletType;
                Seqno = long.Parse(walletInformationResult.Seqno);
                WalletId = long.Parse(walletInformationResult.WalletId);
            }
            else
            {
                WalletType = null;
                Seqno = null;
                WalletId = null;
            }
            Balance = new Coins(walletInformationResult.Balance, new CoinsOptions(true, 9));
            LastTransactionId = walletInformationResult.LastTransactionId;
            switch (walletInformationResult.State)
            {
                case "active":
                {
                    State = AccountState.Active;
                    break;
                }
                case "frozen":
                {
                    State = AccountState.Frozen;
                    break;
                }
                case "uninitialized":
                {
                    State = AccountState.Uninit;
                    break;
                }
                default:
                {
                    State = AccountState.NonExist;
                    break;
                }
            }
        }
    }


    public struct SendBocResult
    {
        [JsonProperty("@type")] public string Type;
    }

    public struct EstimateFeeResult
    {
        [JsonProperty("@type")] public string Type;
        [JsonProperty("source_fees")] public SourceFees SourceFees;
    }

    public struct SourceFees
    {
        [JsonProperty("@type")] public string Type;
        [JsonProperty("in_fwd_fee")] public long InFwdFee;
        [JsonProperty("storage_fee")] public long StorageFee;
        [JsonProperty("gas_fee")] public long GasFee;
        [JsonProperty("fwd_fee")] public long FwdFee;
    }

    public enum AccountState
    {
        Active,
        Frozen,
        Uninit,
        NonExist
    }
}