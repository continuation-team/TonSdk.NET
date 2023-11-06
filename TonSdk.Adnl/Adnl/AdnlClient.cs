using System.Net.Sockets;

namespace TonSdk.Adnl;

public interface IAdnlNetworkClient
{
    public Task Connect(int port, string host);
    public void End();
    public Task Write(byte[] data);
    public Task<int> Read(ArraySegment<byte> buffer);
    public bool IsConnected();
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

    private List<byte> _buffer;
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
    public event Action<Exception> ErrorOccurred;
    
    public AdnlClient(IAdnlNetworkClient socket, string url, byte[] peerPublicKey)
    {
        Uri uri = new Uri(url);
        _host = uri.Host;
        _port = uri.Port;
        _address = new AdnlAddress(peerPublicKey);
        _socket = socket;
    }

    private async Task Handshake()
    {
        byte[] key = _keys.Shared.Take(16).Concat(_params.Hash.Skip(16).Take(16)).ToArray();
        byte[] nonce = _params.Hash.Take(4).Concat(_keys.Shared.Skip(20).Take(12)).ToArray();

        Cipher cipher = CryptoFactory.CreateCipheriv(key, nonce);

        byte[] payload = cipher.Update(_params.Bytes).Concat(cipher.Final()).ToArray();
        byte[] packet = _address.GetHash().Concat(_keys.Public).Concat(_params.Hash).Concat(payload).ToArray();
        
        await _socket.Write(packet);
    }

    private void OnBeforeConnect()
    {
        if (_state != AdnlClientState.Closed) return;
        AdnlKeys keys = new AdnlKeys(_address.PublicKey);
        keys.Generate();

        _keys = keys;
        _params = new AdnlAesParams();
        _cipher = CryptoFactory.CreateCipheriv(_params.TxKey, _params.TxNonce);
        _decipher = CryptoFactory.CreateDecipheriv(_params.RxKey, _params.RxNonce);
        _buffer = new List<byte>();
        _state = AdnlClientState.Connecting;
    }
    
    private async Task ReadDataAsync()
    {
        try
        {
            byte[] buffer = new byte[4096];

            while (_socket.IsConnected())
            {
                ArraySegment<byte> segment = new ArraySegment<byte>(buffer);
                int bytesRead = await _socket.Read(segment);
                if (bytesRead > 0) OnDataReceived(segment.Array);
            }
        }
        catch (Exception ex)
        {
            ErrorOccurred?.Invoke(ex);
        }
        finally
        {
            OnClose();
        }
    }

    private void OnReady()
    {
        _state = AdnlClientState.Open;
        Ready?.Invoke();
    }

    private void OnClose()
    {
        _state = AdnlClientState.Closed;
        Closed?.Invoke();
    }

    private void OnDataReceived(byte[] data)
    {
        _buffer.AddRange(Decrypt(data));
        
        while (_buffer.Count >= AdnlPacket.PacketMinSize)
        {
            AdnlPacket? packet = AdnlPacket.Parse(_buffer.ToArray());
            if (packet == null) break;
            
            _buffer.RemoveRange(0, packet.Length);

            if (_state == AdnlClientState.Connecting)
            {
                if (packet.Payload.Length != 0)
                {
                    ErrorOccurred?.Invoke(new Exception("AdnlClient: Bad handshake."));
                    End();
                    _state = AdnlClientState.Closed;
                }
                else OnReady();
                break;
            }
            
            DataReceived?.Invoke(packet.Payload);
        }
    }

    public AdnlClientState State => _state;

    public async Task Connect()
    {
        OnBeforeConnect();
        try
        {
            await _socket.Connect(_port, _host);
            Task.Run(async () => await ReadDataAsync());
            Connected?.Invoke();
            await Handshake();
        }
        catch (Exception e)
        {
            ErrorOccurred?.Invoke(e);
            End();
            _state = AdnlClientState.Closed;
        }
    }

    public void End()
    {
        if (_state == AdnlClientState.Closed || _state == AdnlClientState.Closing) return;
        _socket.End();
        OnClose();
    }

    public void Write(byte[] data)
    {
        AdnlPacket packet = new AdnlPacket(data);
        byte[] encrypted = Encrypt(packet.Data);
        _socket.Write(encrypted);
    }
    
    private byte[] Encrypt(byte[] data) => _cipher.Update(data);
    private byte[] Decrypt(byte[] data) => _decipher.Update(data);
}