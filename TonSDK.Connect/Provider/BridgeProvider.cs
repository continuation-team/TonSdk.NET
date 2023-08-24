using LaunchDarkly.EventSource;
using Newtonsoft.Json;

namespace TonSdk.Connect;

public delegate void ProviderMessageHandler(string eventData);
public delegate void ProviderErrorHandler(Exception args);
public delegate void WalletEventListener(dynamic walletEvent);

public class BridgeProvider
{
    private readonly int DISCONNECT_TIMEOUT = 600;
    private readonly string STANDART_UNIVERSAL_URL = "tc://";

    private WalletConfig _wallet;
    private BridgeSession? _session;
    private BridgeGateway? _gateway;

    private Dictionary<string, object>? _pendingRequests;
    private List<WalletEventListener>? _listeners;

    public BridgeProvider(WalletConfig wallet)
    {
        _wallet = wallet;
        _session = new BridgeSession();
        _gateway = null;

        _pendingRequests = new Dictionary<string, object>();
        _listeners = new List<WalletEventListener>();
    }

    public async Task<string> ConnectAsync(ConnectRequest connectRequest)
    {
        CloseGateways();
        CryptedSessionInfo sessionInfo = new CryptedSessionInfo();

        string bridgeUrl = _wallet.BridgeUrl;
        string universalUrl = _wallet.UniversalUrl ?? STANDART_UNIVERSAL_URL;

        _gateway = new BridgeGateway(bridgeUrl, sessionInfo.SesionId, new ProviderMessageHandler(GatewayMessageListener), new ProviderErrorHandler(GatewayErrorListener));
        await _gateway.RegisterSession();

        _session.CryptedSessionInfo = sessionInfo;
        _session.BridgeUrl = bridgeUrl;

        return GenerateUniversalLink(universalUrl, connectRequest);
    }

    public async Task RestoreConnection()
    {
        CloseGateways();
        string connectionJsonString = await DefaultStorage.GetItem(DefaultStorage.KEY_CONNECTION, "{}");
        if (connectionJsonString == null || connectionJsonString == "{}") return;

        ConnectionInfo connection = JsonConvert.DeserializeObject<ConnectionInfo>(connectionJsonString);

        if (connection.Session == null) return;
        _session = new BridgeSession(connection.Session);

        _gateway = new BridgeGateway(_session.BridgeUrl, _session.CryptedSessionInfo.SesionId, new ProviderMessageHandler(GatewayMessageListener), new ProviderErrorHandler(GatewayErrorListener));

        _gateway.RegisterSession();

        foreach (var listener in _listeners)
        {
            listener(connection.ConnectEvent);
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

    private async void GatewayMessageListener(string eventData)
    {
        if(eventData.StartsWith("id:")) await DefaultStorage.SetItem(DefaultStorage.KEY_LAST_EVENT_ID, eventData[4..]);

        if(eventData.StartsWith("data:") && _gateway != null && !_gateway.isClosed)
        {
            string data = eventData["data: ".Length..];
            BridgeIncomingMessage dataMessage = JsonConvert.DeserializeObject<BridgeIncomingMessage>(data);
            await ParseGatewayMessage(dataMessage);
        }
    }

    public void CloseConnection()
    {
        CloseGateways();
        _session = new BridgeSession();
        _gateway = null;
        // _pending_requests = { }
        _listeners = new List<WalletEventListener>();
    }

    // TODO: send request and disconnect

    public void Pause() => _gateway?.Pause();

    public async void UnPause() => await _gateway?.UnPause();

    public void Listen(WalletEventListener listener) => _listeners.Add(listener);

    private void RemoveSession()
    {
        if(_gateway != null)
        {
            CloseConnection();
            DefaultStorage.RemoveItem(DefaultStorage.KEY_CONNECTION);
            DefaultStorage.RemoveItem(DefaultStorage.KEY_LAST_EVENT_ID);
        }
    }

    private async void UpdateSession(dynamic walletMessage, string walletPublicKey)
    {
        _session.WalletPublicKey = walletPublicKey;
        ConnectionInfo connection = new();
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
        try
        {
            string json = _session!.CryptedSessionInfo!.Decrypt(Convert.FromBase64String(message.Message), message.From);
            if (json == null || json.Length == 0) return;

            dynamic data = JsonConvert.DeserializeObject<dynamic>(json);
            if(data.@event == null)
            {
                if(data.id != null)
                {
                    //id = wallet_message['id']
                    //if id not in self._pending_requests:
                    //    _LOGGER.debug(f"Response id {id} doesn't match any request's id")
                    //    return

                    //self._pending_requests[id].set_result(wallet_message)
                    //del self._pending_requests[id]
                }
                return;
            }

            if (data.id != null)
            {
                int id = (int)data.id;
                ConnectionInfo connection = JsonConvert.DeserializeObject<ConnectionInfo>(await DefaultStorage.GetItem(DefaultStorage.KEY_CONNECTION, "{}"));
                int lastId = connection.LastWalletEventId ?? 0;

                if (id <= lastId) throw new TonConnectError($"Received event id (={id}) must be greater than stored last wallet event id (={lastId})");

                if (data.@event != null && data.@event != "connect")
                {
                    connection.LastWalletEventId = id;
                    string dumpedConnection = JsonConvert.SerializeObject(connection);
                    await Console.Out.WriteLineAsync(dumpedConnection);
                    await DefaultStorage.SetItem(DefaultStorage.KEY_CONNECTION, dumpedConnection);
                }
            }

            List<WalletEventListener> listenersTemp = _listeners;

            if (data.@event != null && data.@event == "connect") UpdateSession(data, message.From);

            if (data.@event != null && data.@event == "disconnect") RemoveSession();

            foreach (WalletEventListener listener in listenersTemp)
            {
                listener(data);
            }

            return;
        }
        catch (Exception e)
        {
            throw new TonConnectError(e.Message);
        }
        
    }

    private void GatewayErrorListener(Exception e)
    {
        throw new TonConnectError(e.ToString());
    }
}

