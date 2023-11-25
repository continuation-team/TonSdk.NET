using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using TonSdk.Adnl.TL;
using TonSdk.Core;
using TonSdk.Core.Boc;
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
            _adnlClient.ErrorOccurred += AdnlClientOnErrorOccurred;
            _adnlClient.Closed += AdnlClientOnClosed;
            _adnlClient.Connected += AdnlClientOnConnected;
            _adnlClient.Ready += AdnlClientOnReady;
        }

        public LiteClient(int host, int port, string peerPublicKey)
        {
            _adnlClient = new AdnlClientTcp(host, port, peerPublicKey);
            _adnlClient.DataReceived += OnDataReceived;
            _adnlClient.ErrorOccurred += AdnlClientOnErrorOccurred;
            _adnlClient.Closed += AdnlClientOnClosed;
            _adnlClient.Connected += AdnlClientOnConnected;
            _adnlClient.Ready += AdnlClientOnReady;
        }
        
        public LiteClient(string host, int port, byte[] peerPublicKey)
        {
            _adnlClient = new AdnlClientTcp(host, port, peerPublicKey);
            _adnlClient.DataReceived += OnDataReceived;
            _adnlClient.ErrorOccurred += AdnlClientOnErrorOccurred;
            _adnlClient.Closed += AdnlClientOnClosed;
            _adnlClient.Connected += AdnlClientOnConnected;
            _adnlClient.Ready += AdnlClientOnReady;
        }

        public LiteClient(string host, int port, string peerPublicKey)
        {
            _adnlClient = new AdnlClientTcp(host, port, peerPublicKey);
            _adnlClient.DataReceived += OnDataReceived;
            _adnlClient.ErrorOccurred += AdnlClientOnErrorOccurred;
            _adnlClient.Closed += AdnlClientOnClosed;
            _adnlClient.Connected += AdnlClientOnConnected;
            _adnlClient.Ready += AdnlClientOnReady;
        }

        private async void AdnlClientOnReady()
        {
            Console.WriteLine("ready");
        }

        private void AdnlClientOnConnected()
        {
            Console.WriteLine("connected");
        }

        private void AdnlClientOnErrorOccurred(Exception obj)
        {
            Console.WriteLine("error");
        }
        
        private void AdnlClientOnClosed()
        {
            Console.WriteLine("closed");
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

            return LiteClientDecoder.DecodeGetBlock(payload);
        }
        
        // public async Task GetBlockState(BlockIdExternal block)
        // {
        //     byte[] id;
        //     byte[] data;
        //     
        //     (id, data) = LiteClientEncoder.EncodeGetBlock(block, "liteServer.getState id:tonNode.blockIdExt = liteServer.BlockState");
        //     Console.WriteLine("sending payload " + Utils.BytesToHex(data).ToLower());
        //     
        //     var tcs = new TaskCompletionSource<TLReadBuffer>();
        //     _pendingRequests.Add(Utils.BytesToHex(id), tcs);
        //     
        //     await _adnlClient.Write(data);
        //     TLReadBuffer payload = await tcs.Task;
        //
        //     LiteClientDecoder.DecodeGetBlockState(payload);
        // }

        public async Task<byte[]> GetBlockHeader(BlockIdExternal block)
        {
            byte[] id;
            byte[] data;
            
            (id, data) = LiteClientEncoder.EncodeGetBlockHeader(block);
            
            var tcs = new TaskCompletionSource<TLReadBuffer>();
            _pendingRequests.Add(Utils.BytesToHex(id), tcs);
            
            await _adnlClient.Write(data);
            TLReadBuffer payload = await tcs.Task;

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

            return LiteClientDecoder.DecodeSendMessage(payload);
        }

        public async Task<byte[]> GetAccountState(Address address) => await FetchAccountState(address,
            "liteServer.getAccountState id:tonNode.blockIdExt account:liteServer.accountId = liteServer.AccountState");
        
        public async Task<byte[]> GetAccountStatePrunned(Address address) => await FetchAccountState(address,
            "liteServer.getAccountStatePrunned id:tonNode.blockIdExt account:liteServer.accountId = liteServer.AccountState");

        public async Task<RunSmcMethodResult> RunSmcMethod(Address address, string methodName, byte[] stack, RunSmcOptions options)
        {
            ushort crc = Crc32.CalculateCrc16Xmodem(System.Text.Encoding.UTF8.GetBytes(methodName));
            ulong crcExtended = (ulong)crc | 0x10000;

            long methodId;

            using (MemoryStream ms = new MemoryStream())
            {
                await using (BinaryWriter writer = new BinaryWriter(ms))
                {
                    writer.Write(crcExtended);
                }
                
                ms.Position = 0;
                
                using (BinaryReader reader = new BinaryReader(ms))
                {
                    methodId = reader.ReadInt64();
                }
            }
            
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
            
            (id, data) = LiteClientEncoder.EncodeRunSmcMethod(blockId, address, methodId, stack, mode);
            
            var tcs = new TaskCompletionSource<TLReadBuffer>();
            _pendingRequests.Add(Utils.BytesToHex(id), tcs);
            
            await _adnlClient.Write(data);
            TLReadBuffer payload = await tcs.Task;
            
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
            
            return LiteClientDecoder.DecodeGetAllShardsInfo(payload);
        }

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
            return LiteClientDecoder.DecodeBlockHeader(payload);
        }

        public async Task<ListBlockTransactionsResult> ListBLockTransactions(uint count, TransactionId3 after = null, bool? reverseOrder = null, bool? wantProof = null)
        {
            BlockIdExternal blockId = (await GetMasterChainInfo()).LastBlockId;
            
            byte[] id;
            byte[] data;

            (id, data) = LiteClientEncoder.EncodeListBlockTransactions(blockId, count, after, reverseOrder, wantProof);
            
            var tcs = new TaskCompletionSource<TLReadBuffer>();
            _pendingRequests.Add(Utils.BytesToHex(id), tcs);
            
            await _adnlClient.Write(data);
            TLReadBuffer payload = await tcs.Task;
            return LiteClientDecoder.DecodeListBlockTransactions(payload);
        }

        public async Task<PartialBlockProof> GetBlockProof(BlockIdExternal knownBlock = null, BlockIdExternal targetBlock = null)
        {
            knownBlock ??= (await GetMasterChainInfo()).LastBlockId;
            
            byte[] id;
            byte[] data;

            (id, data) = LiteClientEncoder.EncodeGetBlockProof(knownBlock, targetBlock);
            
            var tcs = new TaskCompletionSource<TLReadBuffer>();
            _pendingRequests.Add(Utils.BytesToHex(id), tcs);
            
            await _adnlClient.Write(data);
            TLReadBuffer payload = await tcs.Task;
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
            return LiteClientDecoder.DecodeGetConfigAll(payload);
        }
        
        public async Task<ValidatorStats> GetValidatorStats(BigInteger? startAfter = null, int? modifiedAfter = null)
        {
            BlockIdExternal blockId = (await GetMasterChainInfo()).LastBlockId;
            
            byte[] id;
            byte[] data;

            (id, data) = LiteClientEncoder.EncodeGetValidatorStats(blockId, startAfter, modifiedAfter);
            var tcs = new TaskCompletionSource<TLReadBuffer>();
            _pendingRequests.Add(Utils.BytesToHex(id), tcs);
            
            await _adnlClient.Write(data);
            TLReadBuffer payload = await tcs.Task;
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
            return LiteClientDecoder.DecodeGetLibraries(payload);
        }

        public async Task<ShardBlockProof> GetShardBlockProof(BlockIdExternal blockIdExternal = null)
        {
            blockIdExternal ??= (await GetMasterChainInfo()).LastBlockId;
            
            byte[] id;
            byte[] data;
            
            (id, data) = LiteClientEncoder.EncodeGetShardBlockProof(blockIdExternal);
            var tcs = new TaskCompletionSource<TLReadBuffer>();
            _pendingRequests.Add(Utils.BytesToHex(id), tcs);
            
            await _adnlClient.Write(data);
            TLReadBuffer payload = await tcs.Task;
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

            byte[] stateBytes = LiteClientDecoder.DecodeGetAccountState(payload);
            return stateBytes;
        }
        
        private async void OnDataReceived(byte[] data)
        {
            Console.WriteLine("data " + Utils.BytesToHex(data).ToLower());
            Console.WriteLine();
            
            var readBuffer = new TLReadBuffer(data);
            
            readBuffer.ReadUInt32(); // adnlAnswer
            string queryId = Utils.BytesToHex(readBuffer.ReadInt256()); // queryId
            byte[] liteQuery = readBuffer.ReadBuffer();
            
            var liteQueryBuffer = new TLReadBuffer(liteQuery);
            liteQueryBuffer.ReadUInt32(); // liteQuery

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