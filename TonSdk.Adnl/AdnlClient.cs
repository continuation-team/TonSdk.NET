namespace TonSdk.Adnl;

public interface INetworkClient { }

public enum AdnlClientState
{
    Connecting,
    Open,
    Closing,
    Closed
}

public class AdnlClient
{
    protected INetworkClient _socket;
    protected string _host;
    protected int _port;
    
    private AdnlAddress _address;
    private AdnlClientState _state = AdnlClientState.Closed;
    
    public event Action Connected;
    public event Action Ready;
    public event Action Closed;
    public event Action<byte[]> DataReceived;
    public event Action<Exception, bool> ErrorOccurred;
    
    public AdnlClient(INetworkClient socket, string url, byte[] peerPublicKey)
    {
        Uri uri = new Uri(url);
        _host = uri.Host;
        _port = uri.Port;
        _address = new AdnlAddress(peerPublicKey);
        _socket = socket;
    }

    protected async Task OnBeforeConnect()
    {
        if (_state != AdnlClientState.Closed) return;
    }
}