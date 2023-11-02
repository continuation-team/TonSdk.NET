using System.Net.Sockets;

namespace TonSdk.Adnl.Tcp;

internal class TcpNetworkClient : IAdnlNetworkClient
{
    private TcpClient _socket;
    public void Connect(int port, string host) => _socket.Connect(host, port);
    public void End() => _socket.Close();

    public void Write(byte[] data) => _socket.Client.Send(data);
}

public class AdnlClientTcp : AdnlClient
{
    public AdnlClientTcp(IAdnlNetworkClient socket, string url, byte[] peerPublicKey) : base(socket, url, peerPublicKey)
    {
    }
}