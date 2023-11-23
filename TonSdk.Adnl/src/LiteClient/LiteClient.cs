using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TonSdk.Adnl.TL;
using TonSdk.Core.Crypto;

namespace TonSdk.Adnl.LiteClient
{
    public class LiteClient
    {
        private Dictionary<string, TaskCompletionSource<byte[]>> _pendingRequests;
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
            _pendingRequests = new Dictionary<string, TaskCompletionSource<byte[]>>();
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

            var tcs = new TaskCompletionSource<byte[]>();
            _pendingRequests.Add(Utils.BytesToHex(id), tcs);
            
            await _adnlClient.Write(data);
            byte[] payload = await tcs.Task;
            
            return LiteClientDecoder.DecodeGetMasterchainInfo(payload);
        }
        
        public async Task<MasterChainInfoExternal> GetMasterChainInfoExternal()
        {
            byte[] id;
            byte[] data;

            (id, data) = LiteClientEncoder.EncodeGetMasterchainInfoExt();

            var tcs = new TaskCompletionSource<byte[]>();
            _pendingRequests.Add(Utils.BytesToHex(id), tcs);
            
            await _adnlClient.Write(data);
            byte[] payload = await tcs.Task;
            
            return LiteClientDecoder.DecodeGetMasterchainInfoExternal(payload);
        }

        public async Task<int> GetTime()
        {
            byte[] id;
            byte[] data;

            (id, data) = LiteClientEncoder.EncodeGetTime();
            
            var tcs = new TaskCompletionSource<byte[]>();
            _pendingRequests.Add(Utils.BytesToHex(id), tcs);
            
            await _adnlClient.Write(data);
            byte[] payload = await tcs.Task;
            
            return LiteClientDecoder.DecodeGetTime(payload);
        }

        public async Task<ChainVersion> GetVersion()
        {
            byte[] id;
            byte[] data;
            
            (id, data) = LiteClientEncoder.EncodeGetVersion();
            var tcs = new TaskCompletionSource<byte[]>();
            _pendingRequests.Add(Utils.BytesToHex(id), tcs);
            
            await _adnlClient.Write(data);
            byte[] payload = await tcs.Task;

            return LiteClientDecoder.DecodeGetVersion(payload);
        }

        public async Task GetBlock(BlockIdExternal block)
        {
            byte[] id;
            byte[] data;
            
            (id, data) = LiteClientEncoder.EncodeGetBlock(block, "liteServer.getBlock id:tonNode.blockIdExt = liteServer.BlockData");
            
            var tcs = new TaskCompletionSource<byte[]>();
            _pendingRequests.Add(Utils.BytesToHex(id), tcs);
            
            await _adnlClient.Write(data);
            byte[] payload = await tcs.Task;

            LiteClientDecoder.DecodeGetBlock(payload);
        }
        
        public async Task GetBlockState(BlockIdExternal block)
        {
            byte[] id;
            byte[] data;
            
            (id, data) = LiteClientEncoder.EncodeGetBlock(block, "liteServer.getState id:tonNode.blockIdExt = liteServer.BlockState");
            Console.WriteLine("sending payload " + Utils.BytesToHex(data).ToLower());
            
            var tcs = new TaskCompletionSource<byte[]>();
            _pendingRequests.Add(Utils.BytesToHex(id), tcs);
            
            await _adnlClient.Write(data);
            byte[] payload = await tcs.Task;

            LiteClientDecoder.DecodeGetBlockState(payload);
        }

        public async Task GetBlockHeader(BlockIdExternal block)
        {
            byte[] id;
            byte[] data;
            
            (id, data) = LiteClientEncoder.EncodeGetBlockHeader(block);
            
            var tcs = new TaskCompletionSource<byte[]>();
            _pendingRequests.Add(Utils.BytesToHex(id), tcs);
            
            await _adnlClient.Write(data);
            byte[] payload = await tcs.Task;
        }

        public async Task SendMessage(byte[] body)
        {
            byte[] id;
            byte[] data;
            
            (id, data) = LiteClientEncoder.EncodeSendMessage(body);
            
            var tcs = new TaskCompletionSource<byte[]>();
            _pendingRequests.Add(Utils.BytesToHex(id), tcs);
            
            await _adnlClient.Write(data);
            byte[] payload = await tcs.Task;

            LiteClientDecoder.DecodeSendMessage(payload);
        }
        private async void OnDataReceived(byte[] data)
        {
            Console.WriteLine("data " + Utils.BytesToHex(data).ToLower());
            Console.WriteLine();
            
            var readBuffer = new TLReadBuffer(data);
            
            readBuffer.ReadUInt32Le(); // adnlAnswer
            string queryId = Utils.BytesToHex(readBuffer.ReadInt256Le().ToByteArray()); // queryId
            Console.WriteLine(readBuffer.ReadUInt8()); // size
            readBuffer.ReadUInt32Le(); // liteQuery

            if (!_pendingRequests.ContainsKey(queryId))
            {
                await Console.Out.WriteLineAsync("Response id doesn't match any request's id");
                return;
            }
            
            _pendingRequests[queryId].SetResult(readBuffer.Remainder);
            _pendingRequests.Remove(queryId);
        }
    }
}