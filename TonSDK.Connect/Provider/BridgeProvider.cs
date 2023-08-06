using LaunchDarkly.EventSource;

namespace TonSdk.Connect;

public delegate void ProviderMessageHandler(string eventData);
public delegate void ProviderErrorHandler(ExceptionEventArgs args);

public class BridgeProvider
{
    private readonly int DISCONNECT_TIMEOUT = 600;
    private readonly string STANDART_UNIVERSAL_URL = "tc://";

    private WalletConfig _wallet;
    private BridgeSession? _session;
    private BridgeGateway? _gateway;

    private Dictionary<string, object>? _pendingRequests;
    private List<object>? _listeners;

    public BridgeProvider(WalletConfig wallet)
    {
        _wallet = wallet;
        _session = new BridgeSession();
        _gateway = null;

        _pendingRequests = new Dictionary<string, object>();
        _listeners = new List<object>();
    }

    public async Task<string> ConnectAsync()
    {
        CloseGateways();
        CryptedSessionInfo sessionInfo = new();

        string bridgeUrl = _wallet.BridgeUrl;
        string universalUrl = _wallet.UniversalUrl ?? STANDART_UNIVERSAL_URL;

        _gateway = new BridgeGateway(bridgeUrl, sessionInfo.SesionId, new ProviderMessageHandler(GatewayMessageListener), new ProviderErrorHandler(GatewayErrorListener));
        await _gateway.RegisterSession();

        _session.CryptedSessionInfo = sessionInfo;
        _session.BridgeUrl = bridgeUrl;

        // TODO: Generate Universal Url
        return "";
    }

    public async Task RestoreConnection()
    {
        CloseGateways();


    }

    //async def restore_connection(self):
    //    self._close_gateways()

    //    connection = await self._storage.get_item(IStorage.KEY_CONNECTION)
    //    if not connection:
    //        return False
    //    connection = json.loads(connection)

    //    if 'session' not in connection:
    //        return False
    //    self._session = BridgeSession(connection['session'])

    //    self._gateway = BridgeGateway(
    //        self._storage,
    //        self._session.bridge_url,
    //        self._session.session_crypto.session_id,
    //        self._gateway_listener,
    //        self._gateway_errors_listener
    //    )

    //    await self._gateway.register_session()

    //    for listener in self._listeners:
    //        listener(connection['connect_event'])
        
    //    return True


    public void CloseGateways() => _gateway?.Close();

    private void GatewayMessageListener(string eventData)
    {
        Console.WriteLine(eventData);
    }

    private void GatewayErrorListener(ExceptionEventArgs e)
    {
        throw new TonConnectError(e.Exception.ToString());
    }
}
