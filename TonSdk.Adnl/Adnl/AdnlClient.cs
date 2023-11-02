namespace TonSdk.Adnl;

public interface IAdnlNetworkClient
{
    public void Connect(int port, string host);
    public void End();
    public void Write(byte[] data);
}

public enum AdnlClientState
{
    Connecting,
    Open,
    Closing,
    Closed
}

public class AdnlClient
{
    protected IAdnlNetworkClient _socket;
    protected string _host;
    protected int _port;

    private byte[] _buffer;
    private AdnlAddress _address;
    private AdnlKeys _keys;
    private AdnlAesParams _params;
    private AdnlClientState _state = AdnlClientState.Closed;
    private Cipher _cipher; 
    private Decipher _decipher; 
    
    public event Action Connected;
    public event Action Ready;
    public event Action Closed;
    public event Action<byte[]> DataReceived;
    public event Action<Exception, bool> ErrorOccurred;
    
    public AdnlClient(IAdnlNetworkClient socket, string url, byte[] peerPublicKey)
    {
        Uri uri = new Uri(url);
        _host = uri.Host;
        _port = uri.Port;
        _address = new AdnlAddress(peerPublicKey);
        _socket = socket;
    }

    protected void OnBeforeConnect()
    {
        if (_state != AdnlClientState.Closed) return;
        AdnlKeys keys = new AdnlKeys(_address.PublicKey);
        keys.Generate();

        _keys = keys;
        _params = new AdnlAesParams();
        _cipher = CryptoFactory.CreateCipheriv(_params.TxKey, _params.TxNonce);
        _decipher = CryptoFactory.CreateDecipheriv(_params.RxKey, _params.RxNonce);
        _buffer = Array.Empty<byte>();
        _state = AdnlClientState.Connecting;
    }

    public AdnlClientState State => _state;

    public void Connect()
    {
        OnBeforeConnect();
        _socket.Connect(_port, _host);
    }

    public void End()
    {
        if (_state == AdnlClientState.Closed || _state == AdnlClientState.Closing) return;
        _socket.End();
    }

    public void Write(byte[] data)
    {
        AdnlPacket packet = new AdnlPacket(data);
        byte[] encrypted = Encrypt(packet.Data);
        _socket.Write(encrypted);
    }
    
    protected byte[] Encrypt(byte[] data) => _cipher.Update(data);
    protected byte[] Decrypt(byte[] data) => _decipher.Update(data);
}