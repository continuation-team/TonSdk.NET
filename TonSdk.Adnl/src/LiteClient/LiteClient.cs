using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TonSdk.Adnl.TL;
using TonSdk.Core;
using TonSdk.Core.Crypto;

namespace TonSdk.Adnl.LiteClient
{
    public class LiteClient
    {
        private Dictionary<string, TaskCompletionSource<TLReadBuffer>> _pendingRequests;
        private AdnlClientTcp _adnlClient;
        
        public LiteClient(int host, int port, byte[] peerPublicKey)
        {
            _adnlClient = new AdnlClientTcp(host, port, peerPublicKey);
            _adnlClient.DataReceived += OnDataReceived;
        }

        public LiteClient(int host, int port, string peerPublicKey)
        {
            _adnlClient = new AdnlClientTcp(host, port, peerPublicKey);
            _adnlClient.DataReceived += OnDataReceived;
        }
        
        public LiteClient(string host, int port, byte[] peerPublicKey)
        {
            _adnlClient = new AdnlClientTcp(host, port, peerPublicKey);
            _adnlClient.DataReceived += OnDataReceived;
        }

        public LiteClient(string host, int port, string peerPublicKey)
        {
            _adnlClient = new AdnlClientTcp(host, port, peerPublicKey);
            _adnlClient.DataReceived += OnDataReceived;
        }
        
        public async Task Connect(CancellationToken cancellationToken = default)
        {
            _pendingRequests = new Dictionary<string, TaskCompletionSource<TLReadBuffer>>();
            await _adnlClient.Connect();
            while (_adnlClient.State != AdnlClientState.Open)
            {
                if (cancellationToken.IsCancellationRequested) return;
                await Task.Delay(150, cancellationToken);
            }
        }
        
        public async Task<MasterChainInfo> GetMasterChainInfo()
        {
            byte[] id;
            byte[] data;

            (id, data) = LiteClientEncoder.EncodeGetMasterchainInfo();

            var tcs = new TaskCompletionSource<TLReadBuffer>();
            _pendingRequests.Add(Utils.BytesToHex(id), tcs);
            
            await _adnlClient.Write(data);
            TLReadBuffer payload = await tcs.Task;
            if (payload == null) return null;
            return LiteClientDecoder.DecodeGetMasterchainInfo(payload);
        }
        
        public async Task<MasterChainInfoExternal> GetMasterChainInfoExternal()
        {
            byte[] id;
            byte[] data;

            (id, data) = LiteClientEncoder.EncodeGetMasterchainInfoExt();

            var tcs = new TaskCompletionSource<TLReadBuffer>();
            _pendingRequests.Add(Utils.BytesToHex(id), tcs);
            
            await _adnlClient.Write(data);
            TLReadBuffer payload = await tcs.Task;
            if (payload == null) return null;
            return LiteClientDecoder.DecodeGetMasterchainInfoExternal(payload);
        }

        public async Task<int> GetTime()
        {
            byte[] id;
            byte[] data;

            (id, data) = LiteClientEncoder.EncodeGetTime();
            
            var tcs = new TaskCompletionSource<TLReadBuffer>();
            _pendingRequests.Add(Utils.BytesToHex(id), tcs);
            
            await _adnlClient.Write(data);
            TLReadBuffer payload = await tcs.Task;
            if (payload == null) return -1;
            return LiteClientDecoder.DecodeGetTime(payload);
        }

        public async Task<ChainVersion> GetVersion()
        {
            byte[] id;
            byte[] data;
            
            (id, data) = LiteClientEncoder.EncodeGetVersion();
            var tcs = new TaskCompletionSource<TLReadBuffer>();
            _pendingRequests.Add(Utils.BytesToHex(id), tcs);
            
            await _adnlClient.Write(data);
            TLReadBuffer payload = await tcs.Task;
            if (payload == null) return null;
            return LiteClientDecoder.DecodeGetVersion(payload);
        }

        public async Task<byte[]> GetBlock(BlockIdExternal block)
        {
            byte[] id;
            byte[] data;
            
            (id, data) = LiteClientEncoder.EncodeGetBlock(block, "liteServer.getBlock id:tonNode.blockIdExt = liteServer.BlockData");
            
            var tcs = new TaskCompletionSource<TLReadBuffer>();
            _pendingRequests.Add(Utils.BytesToHex(id), tcs);
            
            await _adnlClient.Write(data);
            TLReadBuffer payload = await tcs.Task;
            if (payload == null) return null;
            return LiteClientDecoder.DecodeGetBlock(payload);
        }

        public async Task<byte[]> GetBlockHeader(BlockIdExternal block)
        {
            byte[] id;
            byte[] data;
            
            (id, data) = LiteClientEncoder.EncodeGetBlockHeader(block);
            
            var tcs = new TaskCompletionSource<TLReadBuffer>();
            _pendingRequests.Add(Utils.BytesToHex(id), tcs);
            
            await _adnlClient.Write(data);
            TLReadBuffer payload = await tcs.Task;
            if (payload == null) return null;
            return LiteClientDecoder.DecodeBlockHeader(payload);
        }

        public async Task<int> SendMessage(byte[] body)
        {
            byte[] id;
            byte[] data;
            
            (id, data) = LiteClientEncoder.EncodeSendMessage(body);
            
            var tcs = new TaskCompletionSource<TLReadBuffer>();
            _pendingRequests.Add(Utils.BytesToHex(id), tcs);
            
            await _adnlClient.Write(data);
            TLReadBuffer payload = await tcs.Task;
            if (payload == null) return -1;
            return LiteClientDecoder.DecodeSendMessage(payload);
        }

        public async Task<byte[]> GetAccountState(Address address) => await FetchAccountState(address,
            "liteServer.getAccountState id:tonNode.blockIdExt account:liteServer.accountId = liteServer.AccountState");
        
        public async Task<byte[]> GetAccountStatePrunned(Address address) => await FetchAccountState(address,
            "liteServer.getAccountStatePrunned id:tonNode.blockIdExt account:liteServer.accountId = liteServer.AccountState");

        public async Task<RunSmcMethodResult> RunSmcMethod(Address address, string methodName, byte[] stack, RunSmcOptions options)
        {
            ushort crc = Crc32.CalculateCrc16Xmodem(Encoding.UTF8.GetBytes(methodName));
            ulong crcExtended = ((ulong)(crc & 0xffff)) | 0x10000;
            
            uint mode = 0;
            if (options.ShardProof || options.Proof)
                mode |= 1u << 0;
            if (options.StateProof)
                mode |= 1u << 1;
            if (options.Result)
                mode |= 1u << 2;
            if (options.InitC7)
                mode |= 1u << 3;
            if (options.LibExtras)
                mode |= 1u << 4;
            
            BlockIdExternal blockId = (await GetMasterChainInfo()).LastBlockId;
            
            byte[] id;
            byte[] data;
            
            (id, data) = LiteClientEncoder.EncodeRunSmcMethod(blockId, address, (long)crcExtended, stack, mode);
            
            var tcs = new TaskCompletionSource<TLReadBuffer>();
            _pendingRequests.Add(Utils.BytesToHex(id), tcs);
            
            await _adnlClient.Write(data);
            TLReadBuffer payload = await tcs.Task;
            if (payload == null) return null;
            return LiteClientDecoder.DecodeRunSmcMethod(payload);
        }

        public async Task<ShardInfo> GetShardInfo(int workchain, long shard, bool exact = false)
        {
            BlockIdExternal blockId = (await GetMasterChainInfo()).LastBlockId;
            
            byte[] id;
            byte[] data;
            
            (id, data) = LiteClientEncoder.EncodeGetShardInfo(blockId, workchain, shard, exact);
            
            var tcs = new TaskCompletionSource<TLReadBuffer>();
            _pendingRequests.Add(Utils.BytesToHex(id), tcs);
            
            await _adnlClient.Write(data);
            TLReadBuffer payload = await tcs.Task;
            if (payload == null) return null;
            return LiteClientDecoder.DecodeGetShardInfo(payload);
        }
        
        public async Task<byte[]> GetAllShardsInfo()
        {
            BlockIdExternal blockId = (await GetMasterChainInfo()).LastBlockId;
            
            byte[] id;
            byte[] data;
            
            (id, data) = LiteClientEncoder.EncodeGetAllShardsInfo(blockId);
            
            var tcs = new TaskCompletionSource<TLReadBuffer>();
            _pendingRequests.Add(Utils.BytesToHex(id), tcs);
            
            await _adnlClient.Write(data);
            TLReadBuffer payload = await tcs.Task;
            if (payload == null) return null;
            return LiteClientDecoder.DecodeGetAllShardsInfo(payload);
        }
        
        /// <summary>
        /// Get account transaction only in masterchain account.
        /// </summary>
        /// <param name="account"></param>
        /// <param name="lt"></param>
        /// <returns></returns>
        public async Task<byte[]> GetOneTransaction(Address account, long lt)
        {
            BlockIdExternal blockId = (await GetMasterChainInfo()).LastBlockId;
            
            byte[] id;
            byte[] data;
            
            (id, data) = LiteClientEncoder.EncodeGetOneTransaction(blockId, account, lt);
            
            var tcs = new TaskCompletionSource<TLReadBuffer>();
            _pendingRequests.Add(Utils.BytesToHex(id), tcs);
            
            await _adnlClient.Write(data);
            TLReadBuffer payload = await tcs.Task;
            if (payload == null) return null;
            return LiteClientDecoder.DecodeGetOneTransaction(payload);
        }
        
        public async Task<byte[]> GetTransactions(uint count, Address account, long lt, BigInteger hash)
        {
            byte[] id;
            byte[] data;
            
            (id, data) = LiteClientEncoder.EncodeGetTransactions(count, account, lt, hash);
            
            var tcs = new TaskCompletionSource<TLReadBuffer>();
            _pendingRequests.Add(Utils.BytesToHex(id), tcs);
            
            await _adnlClient.Write(data);
            TLReadBuffer payload = await tcs.Task;
            if (payload == null) return null;
            return LiteClientDecoder.DecodeGetTransactions(payload);
        }
        
        public async Task<byte[]> LookUpBlock(BlockId blockId, long? lt = null, int? uTime = null)
        {
            byte[] id;
            byte[] data;
            
            (id, data) = LiteClientEncoder.EncodeLookUpBlock(blockId, lt, uTime);
            
            var tcs = new TaskCompletionSource<TLReadBuffer>();
            _pendingRequests.Add(Utils.BytesToHex(id), tcs);
            
            await _adnlClient.Write(data);
            TLReadBuffer payload = await tcs.Task;
            if (payload == null) return null;
            return LiteClientDecoder.DecodeBlockHeader(payload);
        }

        public async Task<ListBlockTransactionsResult> ListBlockTransactions(BlockIdExternal blockIdExternal, uint count, TransactionId3 after = null, bool? reverseOrder = null, bool? wantProof = null)
        {
            byte[] id;
            byte[] data;

            (id, data) = LiteClientEncoder.EncodeListBlockTransactions(blockIdExternal, count, after, reverseOrder, wantProof, 
                "liteServer.listBlockTransactions id:tonNode.blockIdExt mode:# count:# after:mode.7?liteServer.transactionId3 reverse_order:mode.6?true want_proof:mode.5?true = liteServer.BlockTransactions");
            
            var tcs = new TaskCompletionSource<TLReadBuffer>();
            _pendingRequests.Add(Utils.BytesToHex(id), tcs);
            
            await _adnlClient.Write(data);
            TLReadBuffer payload = await tcs.Task;
            if (payload == null) return null;
            return LiteClientDecoder.DecodeListBlockTransactions(payload);
        }
        
        public async Task<ListBlockTransactionsExternalResult> ListBlockTransactionsExternal(BlockIdExternal blockIdExternal, uint count, TransactionId3 after = null, bool? reverseOrder = null, bool? wantProof = null)
        {
            byte[] id;
            byte[] data;

            (id, data) = LiteClientEncoder.EncodeListBlockTransactions(blockIdExternal, count, after, reverseOrder, wantProof,
                "liteServer.listBlockTransactionsExt id:tonNode.blockIdExt mode:# count:# after:mode.7?liteServer.transactionId3 reverse_order:mode.6?true want_proof:mode.5?true = liteServer.BlockTransactionsExt");
            
            var tcs = new TaskCompletionSource<TLReadBuffer>();
            _pendingRequests.Add(Utils.BytesToHex(id), tcs);
            
            await _adnlClient.Write(data);
            TLReadBuffer payload = await tcs.Task;
            if (payload == null) return null;
            return LiteClientDecoder.DecodeListBlockTransactionsExternal(payload);
        }

        public async Task<PartialBlockProof> GetBlockProof(BlockIdExternal knownBlock, BlockIdExternal targetBlock = null)
        {
            byte[] id;
            byte[] data;

            (id, data) = LiteClientEncoder.EncodeGetBlockProof(knownBlock, targetBlock);
            
            var tcs = new TaskCompletionSource<TLReadBuffer>();
            _pendingRequests.Add(Utils.BytesToHex(id), tcs);
            
            await _adnlClient.Write(data);
            TLReadBuffer payload = await tcs.Task;
            if (payload == null) return null;
            return LiteClientDecoder.DecodeGetBlockProof(payload);
        }

        public async Task<ConfigInfo> GetConfigAll()
        {
            BlockIdExternal blockId = (await GetMasterChainInfo()).LastBlockId;
            
            byte[] id;
            byte[] data;

            (id, data) = LiteClientEncoder.EncodeGetConfigAll(blockId);
            var tcs = new TaskCompletionSource<TLReadBuffer>();
            _pendingRequests.Add(Utils.BytesToHex(id), tcs);
            
            await _adnlClient.Write(data);
            TLReadBuffer payload = await tcs.Task;
            if (payload == null) return null;
            return LiteClientDecoder.DecodeGetConfigAll(payload);
        }
        
        public async Task<ConfigInfo> GetConfigParams(int[] paramIds)
        {
            BlockIdExternal blockId = (await GetMasterChainInfo()).LastBlockId;
            
            byte[] id;
            byte[] data;

            (id, data) = LiteClientEncoder.EncodeGetConfigParams(blockId, paramIds);
            var tcs = new TaskCompletionSource<TLReadBuffer>();
            _pendingRequests.Add(Utils.BytesToHex(id), tcs);
            
            await _adnlClient.Write(data);
            TLReadBuffer payload = await tcs.Task;
            if (payload == null) return null;
            return LiteClientDecoder.DecodeGetConfigAll(payload);
        }
        
        public async Task<ValidatorStats> GetValidatorStats(BlockIdExternal blockId, BigInteger? startAfter = null, int? modifiedAfter = null)
        {
            byte[] id;
            byte[] data;

            (id, data) = LiteClientEncoder.EncodeGetValidatorStats(blockId, startAfter, modifiedAfter);
            var tcs = new TaskCompletionSource<TLReadBuffer>();
            _pendingRequests.Add(Utils.BytesToHex(id), tcs);
            
            await _adnlClient.Write(data);
            TLReadBuffer payload = await tcs.Task;
            if (payload == null) return null;
            return LiteClientDecoder.DecodeGetValidatorStats(payload);
        }
        
        public async Task<LibraryEntry[]> GetLibraries(BigInteger[] libraryList)
        {
            byte[] id;
            byte[] data;
            
            (id, data) = LiteClientEncoder.EncodeGetLibraries(libraryList);
            var tcs = new TaskCompletionSource<TLReadBuffer>();
            _pendingRequests.Add(Utils.BytesToHex(id), tcs);
            
            await _adnlClient.Write(data);
            TLReadBuffer payload = await tcs.Task;
            if (payload == null) return null;
            return LiteClientDecoder.DecodeGetLibraries(payload);
        }

        public async Task<ShardBlockProof> GetShardBlockProof(BlockIdExternal blockIdExternal)
        {
            byte[] id;
            byte[] data;
            
            (id, data) = LiteClientEncoder.EncodeGetShardBlockProof(blockIdExternal);
            var tcs = new TaskCompletionSource<TLReadBuffer>();
            _pendingRequests.Add(Utils.BytesToHex(id), tcs);
            
            await _adnlClient.Write(data);
            TLReadBuffer payload = await tcs.Task;
            if (payload == null) return null;
            return LiteClientDecoder.DecodeGetShardBlockProof(payload);
        }
        
        private async Task<byte[]> FetchAccountState(Address address, string query)
        {
            BlockIdExternal blockId = (await GetMasterChainInfo()).LastBlockId;
            byte[] id;
            byte[] data;
            
            (id, data) = LiteClientEncoder.EncodeGetAccountState(blockId, address, query);
            var tcs = new TaskCompletionSource<TLReadBuffer>();
            _pendingRequests.Add(Utils.BytesToHex(id), tcs);
            
            await _adnlClient.Write(data);
            TLReadBuffer payload = await tcs.Task;
            if (payload == null) return null;

            byte[] stateBytes = LiteClientDecoder.DecodeGetAccountState(payload);
            return stateBytes;
        }
        
        private async void OnDataReceived(byte[] data)
        {
            var readBuffer = new TLReadBuffer(data);
            
            readBuffer.ReadUInt32(); // adnlAnswer
            string queryId = Utils.BytesToHex(readBuffer.ReadInt256()); // queryId
            byte[] liteQuery = readBuffer.ReadBuffer();
            
            var liteQueryBuffer = new TLReadBuffer(liteQuery);
            uint responseCode = liteQueryBuffer.ReadUInt32(); // liteQuery

            if (responseCode == BitConverter.ToUInt32(Crc32.ComputeChecksum(
                        Encoding.UTF8.GetBytes(
                            "liteServer.error code:int message:string = liteServer.Error")),
                    0))
            {
                int code = liteQueryBuffer.ReadInt32();
                string message = liteQueryBuffer.ReadString();
                Console.WriteLine("Error: " + message + ". Code: " + code);
                _pendingRequests[queryId].SetResult(null);
                _pendingRequests.Remove(queryId);
                return;
            }
            
            
            if (!_pendingRequests.ContainsKey(queryId))
            {
                await Console.Out.WriteLineAsync("Response id doesn't match any request's id"); 
                return;
            }
            
            _pendingRequests[queryId].SetResult(liteQueryBuffer);
            _pendingRequests.Remove(queryId);
        }
    }
}