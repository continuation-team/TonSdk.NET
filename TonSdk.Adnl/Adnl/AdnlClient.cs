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
    private AdnlKeys _keys;
    private AdnlAesParams _params;
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
        AdnlKeys keys = new AdnlKeys(_address.PublicKey);
        keys.Generate();

        _keys = keys;
        _params = new AdnlAesParams();
    }
    
    /*protected async onBeforeConnect (): Promise<void> {
        if (this.state !== ADNLClientState.CLOSED) {
            return undefined
        }

        const keys = new ADNLKeys(this.address.publicKey)

        await keys.generate()

        this.keys = keys
        this.params = new ADNLAESParams()
        this.cipher = createCipheriv('aes-256-ctr', this.params.txKey, this.params.txNonce)
        this.decipher = createDecipheriv('aes-256-ctr', this.params.rxKey, this.params.rxNonce)
        this.buffer = Buffer.from([])
        this._state = ADNLClientState.CONNECTING
    }*/
}