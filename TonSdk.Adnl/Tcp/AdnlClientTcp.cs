using System.Net.Sockets;

namespace TonSdk.Adnl.Tcp;

internal class TcpNetworkClient : IAdnlNetworkClient
{
    private TcpClient _socket;
    public Task Connect(int port, string host) => _socket.ConnectAsync(host, port);
    public void End() => _socket.Close();
    public Task Write(byte[] data) => _socket.Client.SendAsync(new ArraySegment<byte>(data));
    public Task<int> Read(ArraySegment<byte> buffer) => _socket.Client.ReceiveAsync(buffer);
    public bool IsConnected() => _socket.Connected;
}

public class AdnlClientTcp : AdnlClient
{
    public AdnlClientTcp(IAdnlNetworkClient socket, string url, byte[] peerPublicKey) : base(socket, url, peerPublicKey)
    {
    }
}