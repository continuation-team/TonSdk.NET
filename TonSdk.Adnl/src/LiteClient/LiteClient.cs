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

        private void AdnlClientOnClosed()
        {
            Console.WriteLine("closed");
        }

        public async Task GetMasterChainInfo()
        {
            byte[] id;
            byte[] data;

            (id, data) = LiteClientMethods.EncodeGetMasterchainInfo();

            var tcs = new TaskCompletionSource<byte[]>();
            _pendingRequests.Add(Utils.BytesToHex(id), tcs);
            
            await _adnlClient.Write(data);
            byte[] payload = await tcs.Task;
            
            LiteClientMethods.DecodeGetMasterchainInfo(payload);
        }
        
        private async void OnDataReceived(byte[] data)
        {
            Console.WriteLine("data " + Utils.BytesToHex(data).ToLower());
            Console.WriteLine();
            
            var readBuffer = new TLReadBuffer(data);
            
            readBuffer.ReadUInt32Le(); // adnlAnswer
            string queryId = Utils.BytesToHex(readBuffer.ReadInt256Le().ToByteArray()); // queryId

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