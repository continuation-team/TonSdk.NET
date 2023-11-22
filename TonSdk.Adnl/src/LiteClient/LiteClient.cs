using System;
using System.Threading;
using System.Threading.Tasks;
using TonSdk.Adnl.TL;
using TonSdk.Core.Crypto;

namespace TonSdk.Adnl.LiteClient
{
    public class LiteClient
    {
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
            byte[] tlGettime = Utils.HexToBytes("7af98bb435263e6c95d6fecb497dfd0aa5f031e7d412986b5ce720496db512052e8f2d100cdf068c7904345aad16000000000000");
            await _adnlClient.Write(tlGettime);
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
        }
        
        private async void OnDataReceived(byte[] data)
        {
            Console.WriteLine("data");
        }
    }
}