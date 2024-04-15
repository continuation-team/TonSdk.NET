using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using TonSdk.Adnl.LiteClient;
using TonSdk.Client.Stack;
using TonSdk.Core;
using TonSdk.Core.Block;
using TonSdk.Core.Boc;
using TonSdk.Core.Crypto;

namespace TonSdk.Client
{
    internal class LiteClientApi
    {
        private LiteClient _liteClient;
        
        internal LiteClientApi(string host, int port, string pubKey)
        {
            _liteClient = new LiteClient(host, port, pubKey);
        }

        private async Task Init()
        {
            await _liteClient.Connect();
        }

        internal async Task<WalletInformationResult> GetWalletInformation(Address address)
        {
            await Init();
            
            var result = new WalletInformationResult
            {
                IsWallet = false,
                Balance = new Coins(0)
            };

            var addressInformation = await GetAddressInformation(address);
            result.State = addressInformation.State;
            
            if (addressInformation.State == AccountState.Uninit || addressInformation.State == AccountState.NonExist) 
                return result;

            result.IsWallet = true;
            result.Balance = addressInformation.Balance;
            result.LastTransactionId = addressInformation.LastTransactionId;

            string resultHash = addressInformation.Code.Hash.ToString("base64");
            foreach (var t in WalletUtils.KnownWallets.Where(t => string.Equals(t.CodeHash, resultHash)))
            {
                result.WalletType = t.Type;
                t.DataExtractor(ref result, addressInformation.Data);
                break;
            }
            return result;
        }
        internal async Task<AddressInformationResult> GetAddressInformation(Address address)
        {
            AddressInformationResult result = new AddressInformationResult();
            await Init();
            byte[] accountStateBytes = await _liteClient.GetAccountState(address);
            
            if (accountStateBytes.Length == 0)
            {
                result.State = AccountState.Uninit;
                return result;
            }
            
            CellSlice slice = Cell.From(new Bits(accountStateBytes)).Parse();
            
            slice.LoadBit(); // tag
            slice.LoadAddress(); // skip address (not needed)

            slice.LoadVarUInt(7);
            slice.LoadVarUInt(7);
            slice.LoadVarUInt(7);

            slice.LoadUInt32LE();
            
            if (slice.LoadBit())
                slice.LoadCoins();

            result.LastTransactionId = new TransactionId()
            {
                Lt = (ulong)slice.LoadUInt(64),
            };
            result.Balance = slice.LoadCoins();
            
            var hmOptions = new HashmapOptions<int, int>()
            {
                KeySize = 32,
                Serializers = null,
                Deserializers = null
            };

            slice.LoadDict(hmOptions);

            if (slice.LoadBit()) // active
            {
                result.State = AccountState.Active;
                if(slice.LoadBit())
                    slice.LoadUInt(5);
                
                if (slice.LoadBit())
                {
                    slice.LoadBit();
                    slice.LoadBit();
                }

                if (slice.LoadBit())
                    result.Code = slice.LoadRef();
                
                if (slice.LoadBit())
                    result.Data = slice.LoadRef();
                
                if (slice.LoadBit())
                    slice.LoadRef();
            }
            else if (slice.LoadBit()) // frozen
            {
                result.State = AccountState.Frozen;
                result.FrozenHash = slice.LoadBits(256).ToString("base64");
            }
            else result.State = AccountState.Uninit;
            
            return result;
        }
        internal async Task<MasterchainInformationResult> GetMasterchainInfo()
        {
            await Init();
            return new MasterchainInformationResult(await _liteClient.GetMasterChainInfo());
        }

        internal async Task<SendBocResult> SendBoc(Cell boc)
        {
            await Init();
            var data = await _liteClient.SendMessage(BagOfCells.SerializeBoc(boc).ToBytes());
            return new SendBocResult() { Type = data.ToString() };
        }

        internal async Task<BlockIdExtended> LookUpBlock(int workchain, long shard, long? seqno = null, ulong? lt = null, ulong? unixTime = null)
        {
            await Init();
            var blockHeader = await _liteClient.LookUpBlock(workchain, shard, seqno, lt, unixTime);
            return new BlockIdExtended(blockHeader.BlockId);
        }
        
        internal async Task<BlockDataResult> GetBlockData(int workchain, long shard, long seqno)
        {
            var result = new BlockDataResult();
            await Init();
            var blockHeader = await _liteClient.LookUpBlock(workchain, shard, seqno);
            result.BlockIdExtended = new BlockIdExtended(blockHeader.BlockId);
            
            byte[] blockDataBytes = await _liteClient.GetBlock(blockHeader.BlockId);
            if (blockDataBytes.Length == 0) 
                return result;
            
            CellSlice slice = Cell.From(new Bits(blockDataBytes)).Parse();
            result.Version = (uint)slice.LoadUInt(32);
            return result;
        }

        internal async Task<BlockTransactionsResult> GetBlockTransactions(
            int workchain,
            long shard,
            long seqno,
            string rootHash = null,
            string fileHash = null,
            ulong? afterLt = null,
            string afterHash = null,
            uint count = 10)
        {
            var result = new BlockTransactionsResult();
            await Init();

            Adnl.LiteClient.BlockIdExtended blockId;
            if (rootHash == null || fileHash == null)
                blockId = (await _liteClient.LookUpBlock(workchain, shard, seqno)).BlockId;
            else
                blockId = new Adnl.LiteClient.BlockIdExtended(workchain, Convert.FromBase64String(rootHash),
                    Convert.FromBase64String(fileHash), shard, (int)seqno);

            var transactionId = new Adnl.LiteClient.TransactionId();
            if (afterLt != null && afterHash != null)
            {
                transactionId.Hash = Convert.FromBase64String(afterHash);
                transactionId.Lt = (long)afterLt;
            }
            else transactionId = null;
            
            ListBlockTransactionsResult blockTransactions = await _liteClient.ListBlockTransactions(blockId, count, transactionId);

            result.Transactions = blockTransactions.TransactionIds.Select(tx => 
                new ShortTransactionsResult()
                {
                    Account = new Address(workchain, tx.Account).ToString(),
                    Hash = Convert.ToBase64String(tx.Hash).ToLower(),
                    Lt = (ulong)tx.Lt
                }).ToArray();
            result.Incomplete = blockTransactions.InComplete;
            result.ReqCount = (int)count;
            result.Id = new BlockIdExtended(blockId);
            return result;
        }

        internal async Task<TransactionsInformationResult[]> GetTransactions(Address address, uint limit, long lt, string hash)
        {
            var result = new List<TransactionsInformationResult>();
            await Init();
            
            byte[] transactions = await _liteClient.GetTransactions(limit, address, lt, hash);
            
            if (transactions.Length == 0) 
                return result.ToArray();
            
            // public long Utime;
            // public Cell Data;
            // public TransactionId TransactionId;
            // public Coins Fee;
            // public Coins StorageFee;
            // public Coins OtherFee;
            // public RawMessage InMsg;
            // public RawMessage[] OutMsgs;

            // Console.WriteLine(new Bits(transactions).ToString("base64"));
            Cell[] cells = BagOfCells.DeserializeBoc(new Bits(transactions));
            
            foreach (var cell in cells)
            {
                var tx = new TransactionsInformationResult();
                
                var slice = cell.Parse();
                
                slice.LoadBits(4);
                slice.LoadBytes(32);
                tx.TransactionId.Lt = (ulong)slice.LoadUInt(64);
                
                slice.LoadBytes(32);
                slice.LoadUInt(64);
                
                uint outMsgCount = (uint)slice.LoadUInt(15);
                slice.LoadBits(2);
                slice.LoadBits(2);

                var firstRefSlice = slice.LoadRef().Parse();
                var isMsgRef = firstRefSlice.LoadBit();
                if (isMsgRef)
                {
                    var inMsg = firstRefSlice.LoadRef().Parse();
                    var msgx = MessageX.Parse(inMsg);

                    var cmnMsgInfo = msgx.Data.Info.Cell.Parse();
                    cmnMsgInfo.LoadBit();
                    cmnMsgInfo.LoadBit();
                    cmnMsgInfo.LoadBit();
                    cmnMsgInfo.LoadBit();
                    tx.InMsg.Source = cmnMsgInfo.LoadAddress();
                    tx.InMsg.Destination = cmnMsgInfo.LoadAddress();
                    if (cmnMsgInfo.RemainderBits != 2)
                    {
                        tx.InMsg.Value = cmnMsgInfo.LoadCoins() ?? new Coins(0);
                        if (cmnMsgInfo.LoadBit())
                            cmnMsgInfo.LoadRef();
                        tx.InMsg.IhrFee = cmnMsgInfo.LoadCoins() ?? new Coins(0);
                        tx.InMsg.FwdFee = cmnMsgInfo.LoadCoins() ?? new Coins(0);
                        tx.InMsg.CreaterLt = (long)cmnMsgInfo.LoadUInt(64);
                    }
                }
                
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
                
                var outMsgsMap = firstRefSlice.LoadDict(hmOptions);
                var msgsList = new List<RawMessage>();
                for (uint i = 0; i < outMsgsMap.Count; i++)
                {
                    var rawMessage = new RawMessage();
                    var msg = outMsgsMap.Get(i);
                    var outMsgX = MessageX.Parse(msg.LoadRef().Parse());
                    var cmnMsgInfo = outMsgX.Data.Info.Cell.Parse();
                    cmnMsgInfo.LoadBit();
                    cmnMsgInfo.LoadBit();
                    cmnMsgInfo.LoadBit();
                    cmnMsgInfo.LoadBit();
                    rawMessage.Source = cmnMsgInfo.LoadAddress();
                    rawMessage.Destination = cmnMsgInfo.LoadAddress();
                    if (cmnMsgInfo.RemainderBits != 2)
                    {
                        rawMessage.Value = cmnMsgInfo.LoadCoins();
                        if (cmnMsgInfo.LoadBit())
                            cmnMsgInfo.LoadRef();
                        rawMessage.IhrFee = cmnMsgInfo.LoadCoins();
                        rawMessage.FwdFee = cmnMsgInfo.LoadCoins();
                        rawMessage.CreaterLt = (long)cmnMsgInfo.LoadUInt(64);
                    }
                    msgsList.Add(rawMessage);
                }
                tx.OutMsgs = msgsList.ToArray();
                result.Add(tx);
            }
            return result.ToArray();
        }

        internal async Task<RunGetMethodResult?> RunGetMethod(Address address, string method, IStackItem[] stackItems)
        {
            await Init();
            
            RunGetMethodResult result = new RunGetMethodResult();
            byte[] stackBytes = BagOfCells.SerializeBoc(StackUtils.SerializeStack(stackItems)).ToBytes();
            RunSmcMethodResult smcResult = await _liteClient.RunSmcMethod(address, method, stackBytes, new RunSmcOptions() { Result = true });
            try
            {
                IStackItem[] resultStack = StackUtils.DeserializeStack(Convert.ToBase64String(smcResult.Result));
                result.StackItems = resultStack;
                result.ExitCode = smcResult.ExitCode;
                return result;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        internal async Task<ConfigParamResult> GetConfigParam(int configId)
        {
            await Init();
            
            var result = new ConfigParamResult();
            byte[] configBytes = (await _liteClient.GetConfigParams(new int[] { configId })).ConfigProof;
            Console.WriteLine(Convert.ToBase64String(configBytes));
            result.Bytes = Cell.From(new Bits(configBytes));
            return result;
        }

        internal async Task<EstimateFeeResult> EstimateFee(MessageX messageX, bool ignore = true)
        {
            await Init();
            var result = new EstimateFeeResult();
            //await _liteClient.SendMessage(messageX.Cell.Bits.ToBytes());
            return result;
        }

        internal async Task<ShardsInformationResult> GetShards(long seqno)
        {
            await Init();

            var result = new ShardsInformationResult();
            var block = (await _liteClient.LookUpBlock(-1, -9223372036854775808, seqno)).BlockId;
            byte[] data = (await _liteClient.GetAllShardsInfo(block));
            var cells = BagOfCells.DeserializeBoc(new Bits(data));

            foreach (var cell in cells)
            {
                var hmOptions = new HashmapOptions<uint, CellSlice>()
                {
                    KeySize = 32,
                    Serializers = new HashmapSerializers<uint, CellSlice>
                    {
                        Key = k => new BitsBuilder(32).StoreUInt(k, 32).Build(),
                        Value = v => new CellBuilder().Build()
                    },
                    Deserializers = new HashmapDeserializers<uint, CellSlice>
                    {
                        Key = k => (uint)k.Parse().LoadUInt(32),
                        Value = v => v.Parse()
                    }
                };
                
                var hashes = cell.Parse().LoadDict(hmOptions);
                var binTree = hashes.Get(0).LoadRef().Parse();
                var shards = new List<BlockIdExtended>();
                LoadBinTreeR(binTree, ref shards);

                result.Shards = shards.ToArray();
            }
            return result;
        }
        
        private BlockIdExtended LoadShardDescription(CellSlice slice)
        {
            uint type = (uint)slice.LoadUInt(4);
            
            if (type != 0xa && type != 0xb)
                throw new Exception("not a ShardDescr");
            
            int seqno = (int)slice.LoadUInt(32);
            slice.LoadUInt(32);
            slice.LoadUInt(64);
            slice.LoadUInt(64);
            byte[] rootHash = slice.LoadBits(256).ToBytes(); // root_hash
            byte[] fileHash = slice.LoadBits(256).ToBytes(); // file_hash
            slice.LoadBit();
            slice.LoadBit();
            slice.LoadBit();
            slice.LoadBit();
            slice.LoadBit();
            slice.LoadUInt(3);
            slice.LoadUInt(32);
            long shard = (long)slice.LoadInt(64);

            return new BlockIdExtended(new TonSdk.Adnl.LiteClient.BlockIdExtended(0, rootHash, fileHash, shard, seqno));
        }
        
        private void LoadBinTreeR(CellSlice slice, ref List<BlockIdExtended> shards)
        {
            if (!slice.LoadBit())
            {
                shards.Add(LoadShardDescription(slice));
            }
            else
            {
                LoadBinTreeR(slice.LoadRef().Parse(), ref shards);
                LoadBinTreeR(slice.LoadRef().Parse(), ref shards);
            }
        }
    }
}