namespace TonSdk.Adnl;

public interface INetworkClient { }

public class AdnlClient
{
    private INetworkClient _socket;
    private string _host;
    private int _port;
    private AdnlAddress _address;
    
    public AdnlClient(INetworkClient socket, string url, byte[] peerPublicKey)
    {
        Uri uri = new Uri(url);
        _host = uri.Host;
        _port = uri.Port;
        _address = new AdnlAddress(peerPublicKey);
        _socket = socket;
    }
}