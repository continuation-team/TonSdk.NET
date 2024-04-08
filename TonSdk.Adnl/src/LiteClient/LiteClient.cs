using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
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
            if(_adnlClient.State == AdnlClientState.Open) return;
            _pendingRequests = new Dictionary<string, TaskCompletionSource<TLReadBuffer>>();
            await _adnlClient.Connect();
            while (_adnlClient.State != AdnlClientState.Open)
            {
                if (cancellationToken.IsCancellationRequested) return;
                await Task.Delay(150, cancellationToken);
            }
        }

        public void Disconnect()
        {
            if(_adnlClient.State != AdnlClientState.Open) return;
            _pendingRequests = new Dictionary<string, TaskCompletionSource<TLReadBuffer>>();
            _adnlClient.End();
        }

        public async Task PingPong()
        {
            if (_adnlClient.State != AdnlClientState.Open) 
                throw new Exception("Connection to lite server must be init before method calling. Use Connect() method to set up connection.");
            
            byte[] data = LiteClientEncoder.EncodePingPong();
            await _adnlClient.Write(data);
        }
        
        public async Task<MasterChainInfo> GetMasterChainInfo()
        {
            if (_adnlClient.State != AdnlClientState.Open) 
                throw new Exception("Connection to lite server must be init before method calling. Use Connect() method to set up connection.");
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
        
        public async Task<MasterChainInfoExtended> GetMasterChainInfoExtended()
        {
            if (_adnlClient.State != AdnlClientState.Open) 
                throw new Exception("Connection to lite server must be init before method calling. Use Connect() method to set up connection.");
            byte[] id;
            byte[] data;

            (id, data) = LiteClientEncoder.EncodeGetMasterchainInfoExt();

            var tcs = new TaskCompletionSource<TLReadBuffer>();
            _pendingRequests.Add(Utils.BytesToHex(id), tcs);
            
            await _adnlClient.Write(data);
            TLReadBuffer payload = await tcs.Task;
            if (payload == null) return null;
            return LiteClientDecoder.DecodeGetMasterchainInfoExtended(payload);
        }

        public async Task<int> GetTime()
        {
            if (_adnlClient.State != AdnlClientState.Open) 
                throw new Exception("Connection to lite server must be init before method calling. Use Connect() method to set up connection.");
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
            if (_adnlClient.State != AdnlClientState.Open) 
                throw new Exception("Connection to lite server must be init before method calling. Use Connect() method to set up connection.");
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

        public async Task<byte[]> GetBlock(BlockIdExtended block)
        {
            if (_adnlClient.State != AdnlClientState.Open) 
                throw new Exception("Connection to lite server must be init before method calling. Use Connect() method to set up connection.");
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

        public async Task<BlockHeader> GetBlockHeader(BlockIdExtended block)
        {
            if (_adnlClient.State != AdnlClientState.Open) 
                throw new Exception("Connection to lite server must be init before method calling. Use Connect() method to set up connection.");
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
            if (_adnlClient.State != AdnlClientState.Open) 
                throw new Exception("Connection to lite server must be init before method calling. Use Connect() method to set up connection.");
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
            if (_adnlClient.State != AdnlClientState.Open) 
                throw new Exception("Connection to lite server must be init before method calling. Use Connect() method to set up connection.");
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
            
            BlockIdExtended blockId = (await GetMasterChainInfo()).LastBlockId;
            
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
            if (_adnlClient.State != AdnlClientState.Open) 
                throw new Exception("Connection to lite server must be init before method calling. Use Connect() method to set up connection.");
            BlockIdExtended blockId = (await GetMasterChainInfo()).LastBlockId;
            
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
        
        public async Task<byte[]> GetAllShardsInfo(BlockIdExtended blockIdExtended = null)
        {
            if (_adnlClient.State != AdnlClientState.Open) 
                throw new Exception("Connection to lite server must be init before method calling. Use Connect() method to set up connection.");
            if(blockIdExtended == null)
                blockIdExtended = (await GetMasterChainInfo()).LastBlockId;
            
            byte[] id;
            byte[] data;
            
            (id, data) = LiteClientEncoder.EncodeGetAllShardsInfo(blockIdExtended);
            
            var tcs = new TaskCompletionSource<TLReadBuffer>();
            _pendingRequests.Add(Utils.BytesToHex(id), tcs);
            
            await _adnlClient.Write(data);
            TLReadBuffer payload = await tcs.Task;
            if (payload == null) return null;
            return LiteClientDecoder.DecodeGetAllShardsInfo(payload);
        }
        
        public async Task<byte[]> GetTransactions(uint count, Address account, long lt, string hash)
        {
            if (_adnlClient.State != AdnlClientState.Open) 
                throw new Exception("Connection to lite server must be init before method calling. Use Connect() method to set up connection.");
            
            byte[] hashBytes;
            if (hash.isHexString()) hashBytes = Utils.HexToBytes(hash);
            else if (hash.isBase64()) hashBytes = Convert.FromBase64String(hash);
            else throw new Exception("Not valid hash string. Set only in hex or non-url base64.");
            
            byte[] id;
            byte[] data;
            
            (id, data) = LiteClientEncoder.EncodeGetTransactions(count, account, lt, hashBytes);
            
            var tcs = new TaskCompletionSource<TLReadBuffer>();
            _pendingRequests.Add(Utils.BytesToHex(id), tcs);
            
            await _adnlClient.Write(data);
            TLReadBuffer payload = await tcs.Task;
            if (payload == null) return null;
            return LiteClientDecoder.DecodeGetTransactions(payload);
        }
        
        public async Task<BlockHeader> LookUpBlock(int workchain, long shard, long? seqno = null, ulong? lt = null, ulong? uTime = null)
        {
            if (_adnlClient.State != AdnlClientState.Open) 
                throw new Exception("Connection to lite server must be init before method calling. Use Connect() method to set up connection.");
            byte[] id;
            byte[] data;
            
            (id, data) = LiteClientEncoder.EncodeLookUpBlock(workchain, shard, seqno, lt, uTime);
            
            var tcs = new TaskCompletionSource<TLReadBuffer>();
            _pendingRequests.Add(Utils.BytesToHex(id), tcs);
            
            await _adnlClient.Write(data);
            TLReadBuffer payload = await tcs.Task;
            if (payload == null) return null;
            return LiteClientDecoder.DecodeBlockHeader(payload);
        }

        public async Task<ListBlockTransactionsResult> ListBlockTransactions(BlockIdExtended blockIdExtended, uint count, ITransactionId after = null, bool? reverseOrder = null, bool? wantProof = null)
        {
            if (_adnlClient.State != AdnlClientState.Open) 
                throw new Exception("Connection to lite server must be init before method calling. Use Connect() method to set up connection.");
            byte[] id;
            byte[] data;

            (id, data) = LiteClientEncoder.EncodeListBlockTransactions(blockIdExtended, count, after, reverseOrder, wantProof, 
                "liteServer.listBlockTransactions id:tonNode.blockIdExt mode:# count:# after:mode.7?liteServer.transactionId3 reverse_order:mode.6?true want_proof:mode.5?true = liteServer.BlockTransactions");
            
            var tcs = new TaskCompletionSource<TLReadBuffer>();
            _pendingRequests.Add(Utils.BytesToHex(id), tcs);
            
            await _adnlClient.Write(data);
            TLReadBuffer payload = await tcs.Task;
            if (payload == null) return null;
            return LiteClientDecoder.DecodeListBlockTransactions(payload);
        }
        
        public async Task<ListBlockTransactionsExtendedResult> ListBlockTransactionsExtended(BlockIdExtended blockIdExtended, uint count, ITransactionId after = null, bool? reverseOrder = null, bool? wantProof = null)
        {
            if (_adnlClient.State != AdnlClientState.Open) 
                throw new Exception("Connection to lite server must be init before method calling. Use Connect() method to set up connection.");
            byte[] id;
            byte[] data;

            (id, data) = LiteClientEncoder.EncodeListBlockTransactions(blockIdExtended, count, after, reverseOrder, wantProof,
                "liteServer.listBlockTransactionsExt id:tonNode.blockIdExt mode:# count:# after:mode.7?liteServer.transactionId3 reverse_order:mode.6?true want_proof:mode.5?true = liteServer.BlockTransactionsExt");
            
            var tcs = new TaskCompletionSource<TLReadBuffer>();
            _pendingRequests.Add(Utils.BytesToHex(id), tcs);
            
            await _adnlClient.Write(data);
            TLReadBuffer payload = await tcs.Task;
            if (payload == null) return null;
            return LiteClientDecoder.DecodeListBlockTransactionsExtended(payload);
        }

        [Obsolete]
        public async Task<PartialBlockProof> GetBlockProof(BlockIdExtended knownBlock, BlockIdExtended targetBlock = null)
        {
            if (_adnlClient.State != AdnlClientState.Open) 
                throw new Exception("Connection to lite server must be init before method calling. Use Connect() method to set up connection.");
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
            if (_adnlClient.State != AdnlClientState.Open) 
                throw new Exception("Connection to lite server must be init before method calling. Use Connect() method to set up connection.");
            BlockIdExtended blockId = (await GetMasterChainInfo()).LastBlockId;
            
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
            if (_adnlClient.State != AdnlClientState.Open) 
                throw new Exception("Connection to lite server must be init before method calling. Use Connect() method to set up connection.");
            BlockIdExtended blockId = (await GetMasterChainInfo()).LastBlockId;
            
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
        
        public async Task<LibraryEntry[]> GetLibraries(BigInteger[] libraryList)
        {
            if (_adnlClient.State != AdnlClientState.Open) 
                throw new Exception("Connection to lite server must be init before method calling. Use Connect() method to set up connection.");
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

        public async Task<ShardBlockProof> GetShardBlockProof(BlockIdExtended blockIdExtended)
        {
            if (_adnlClient.State != AdnlClientState.Open) 
                throw new Exception("Connection to lite server must be init before method calling. Use Connect() method to set up connection.");
            byte[] id;
            byte[] data;
            
            (id, data) = LiteClientEncoder.EncodeGetShardBlockProof(blockIdExtended);
            var tcs = new TaskCompletionSource<TLReadBuffer>();
            _pendingRequests.Add(Utils.BytesToHex(id), tcs);
            
            await _adnlClient.Write(data);
            TLReadBuffer payload = await tcs.Task;
            if (payload == null) return null;
            return LiteClientDecoder.DecodeGetShardBlockProof(payload);
        }
        
        private async Task<byte[]> FetchAccountState(Address address, string query)
        {
            if (_adnlClient.State != AdnlClientState.Open) 
                throw new Exception("Connection to lite server must be init before method calling. Use Connect() method to set up connection.");
            BlockIdExtended blockId = (await GetMasterChainInfo()).LastBlockId;
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
            
            uint answer = readBuffer.ReadUInt32();
            
            if(answer == BitConverter.ToUInt32(
                   Crc32.ComputeChecksum(
                       Encoding.UTF8.GetBytes("tcp.pong random_id:long = tcp.Pong")), 0)) return;
            
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