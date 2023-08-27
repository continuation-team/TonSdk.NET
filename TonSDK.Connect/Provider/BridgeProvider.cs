using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TonSdk.Connect
{

    public delegate void ProviderMessageHandler(string eventData);
    public delegate void ProviderErrorHandler(Exception args);
    public delegate void WalletEventListener(dynamic walletEvent);

    public delegate void OnRequestSentHandler();

    public class BridgeProvider
    {
        private readonly int DISCONNECT_TIMEOUT = 600;
        private readonly string STANDART_UNIVERSAL_URL = "tc://";

        private WalletConfig? _wallet;
        private BridgeSession? _session;
        private BridgeGateway? _gateway;

        private Dictionary<string, TaskCompletionSource<object>>? _pendingRequests;
        private List<WalletEventListener>? _listeners;

        public BridgeProvider(WalletConfig? wallet = null)
        {
            _wallet = wallet;
            _session = new BridgeSession();
            _gateway = null;

            _pendingRequests = new Dictionary<string, TaskCompletionSource<object>>();
            _listeners = new List<WalletEventListener>();
        }

        public async Task<string> ConnectAsync(ConnectRequest connectRequest)
        {
            CloseGateways();
            CryptedSessionInfo sessionInfo = new CryptedSessionInfo();

            string bridgeUrl = _wallet?.BridgeUrl;
            string universalUrl = _wallet?.UniversalUrl ?? STANDART_UNIVERSAL_URL;

            _gateway = new BridgeGateway(bridgeUrl, sessionInfo.SesionId, new ProviderMessageHandler(GatewayMessageListener), new ProviderErrorHandler(GatewayErrorListener));
            await _gateway.RegisterSession();

            _session.CryptedSessionInfo = sessionInfo;
            _session.BridgeUrl = bridgeUrl;

            return GenerateUniversalLink(universalUrl, connectRequest);
        }

        public async Task<bool> RestoreConnection()
        {
            CloseGateways();
            string connectionJsonString = await DefaultStorage.GetItem(DefaultStorage.KEY_CONNECTION, "{}");
            if (connectionJsonString == null || connectionJsonString == "{}") return false;

            ConnectionInfo connection = JsonConvert.DeserializeObject<ConnectionInfo>(connectionJsonString);

            if (connection.Session == null) return false;
            _session = new BridgeSession(connection.Session);

            _gateway = new BridgeGateway(_session.BridgeUrl, _session.CryptedSessionInfo.SesionId, new ProviderMessageHandler(GatewayMessageListener), new ProviderErrorHandler(GatewayErrorListener));
            await _gateway.RegisterSession();

            foreach (var listener in _listeners)
            {
                listener(connection.ConnectEvent);
            }

            return true;
        }

        public async Task<dynamic> SendRequest(IRpcRequest request, OnRequestSentHandler? onRequestSent = null)
        {
            if (_gateway == null || _session == null || _session.WalletPublicKey == null) throw new TonConnectError("Trying to send bridge request without session.");
            string connectionJsonString = await DefaultStorage.GetItem(DefaultStorage.KEY_CONNECTION, "{}");
            ConnectionInfo connection = JsonConvert.DeserializeObject<ConnectionInfo>(connectionJsonString);

            int id = connection.NextRpcRequestId ?? 0;
            connection.NextRpcRequestId = id + 1;

            string jsonString = JsonConvert.SerializeObject(connection);
            await DefaultStorage.SetItem(DefaultStorage.KEY_CONNECTION, jsonString);

            request.id = id.ToString();
            await Console.Out.WriteLineAsync(">>>>" + JsonConvert.SerializeObject(request) + "<<<<<");
            string encryptedRequest = _session.CryptedSessionInfo.Encrypt(JsonConvert.SerializeObject(request), _session.WalletPublicKey);

            await _gateway.Send(encryptedRequest, _session.WalletPublicKey, request.method);

            TaskCompletionSource<dynamic> resolve = new TaskCompletionSource<dynamic>();
            _pendingRequests.Add(id.ToString(), resolve);

            onRequestSent?.Invoke();
            dynamic result = await resolve.Task;
            return result;
        }

        public async Task Disconnect()
        {
            try
            {
                DisconnectRpcRequest request = new DisconnectRpcRequest()
                {
                    method = "disconnect",
                    @params = Array.Empty<string>(),
                };
                await SendRequest(request, RemoveSession);
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync(ex.Message);
                RemoveSession();
            }
        }

        private string GenerateUniversalLink(string universalLink, ConnectRequest connectRequest)
        {
            UriBuilder url = new UriBuilder(universalLink);
            url.Query += $"v={2}";
            url.Query += $"&id={_session?.CryptedSessionInfo?.SesionId}";
            url.Query += $"&r={JsonConvert.SerializeObject(connectRequest)}";
            return url.ToString();
        }

        public void CloseGateways() => _gateway?.Close();

        public void CloseConnection()
        {
            CloseGateways();
            _session = new BridgeSession();
            _gateway = null;
            _pendingRequests = new Dictionary<string, TaskCompletionSource<object>>();
            _listeners = new List<WalletEventListener>();
        }

        public void Pause() => _gateway?.Pause();

        public async Task UnPause() => await _gateway?.UnPause();

        public void Listen(WalletEventListener listener) => _listeners.Add(listener);

        private void RemoveSession()
        {
            if (_gateway != null)
            {
                CloseConnection();
                DefaultStorage.RemoveItem(DefaultStorage.KEY_CONNECTION);
                DefaultStorage.RemoveItem(DefaultStorage.KEY_LAST_EVENT_ID);
            }
        }

        private async void UpdateSession(dynamic walletMessage, string walletPublicKey)
        {
            _session.WalletPublicKey = walletPublicKey;
            ConnectionInfo connection = new ConnectionInfo();
            connection.Type = "http";
            connection.Session = _session.GetSessionInfo();
            connection.LastWalletEventId = walletMessage.id ?? null;
            connection.ConnectEvent = walletMessage;
            connection.NextRpcRequestId = 0;

            string jsonString = JsonConvert.SerializeObject(connection);
            await DefaultStorage.SetItem(DefaultStorage.KEY_CONNECTION, jsonString);
        }

        private async Task ParseGatewayMessage(BridgeIncomingMessage message)
        {
            string json = _session!.CryptedSessionInfo!.Decrypt(Convert.FromBase64String(message.Message), message.From);
            if (json == null || json.Length == 0) return;


            dynamic data = JsonConvert.DeserializeObject<dynamic>(json);
            await Console.Out.WriteLineAsync(data.ToString());
            if (data.@event == null)
            {
                if (data.id != null)
                {
                    string id = data.id;
                    if (!_pendingRequests.ContainsKey(id))
                    {
                        await Console.Out.WriteLineAsync($"Response id {id} doesn't match any request's id");
                        return;
                    }

                    _pendingRequests[id].SetResult(data);
                    _pendingRequests.Remove(id);
                }
                return;
            }

            if (data.id != null)
            {
                int id = (int)data.id;
                ConnectionInfo connection = JsonConvert.DeserializeObject<ConnectionInfo>(await DefaultStorage.GetItem(DefaultStorage.KEY_CONNECTION, "{}"));
                int lastId = connection.LastWalletEventId ?? 0;

                if (id <= lastId)
                {
                    await Console.Out.WriteLineAsync($"Received event id (={id}) must be greater than stored last wallet event id (={lastId})");
                    return;
                }

                if (data.@event != null && (string)data.@event != "connect")
                {
                    connection.LastWalletEventId = id;
                    string dumpedConnection = JsonConvert.SerializeObject(connection);
                    await Console.Out.WriteLineAsync(dumpedConnection);
                    await DefaultStorage.SetItem(DefaultStorage.KEY_CONNECTION, dumpedConnection);
                }
            }

            List<WalletEventListener> listenersTemp = _listeners;

            if (data.@event != null && (string)data.@event == "connect") UpdateSession(data, message.From);

            if (data.@event != null && (string)data.@event == "disconnect") RemoveSession();

            foreach (WalletEventListener listener in listenersTemp)
            {
                listener(data);
            }
            return;
        }

        private async void GatewayMessageListener(string eventData)
        {
            if (eventData.StartsWith("id:")) await DefaultStorage.SetItem(DefaultStorage.KEY_LAST_EVENT_ID, eventData.Substring(4));

            if (eventData.StartsWith("data:") && _gateway != null && !_gateway.isClosed)
            {
                string data = eventData.Substring(5);
                BridgeIncomingMessage dataMessage = JsonConvert.DeserializeObject<BridgeIncomingMessage>(data);
                await ParseGatewayMessage(dataMessage);
            }
        }

        private void GatewayErrorListener(Exception e)
        {
            throw new TonConnectError(e.ToString());
        }
    }
}
