using Org.BouncyCastle.Bcpg.Sig;

namespace TonSdk.Connect;

public struct ConnectionInfo
{
    public string? Type { get; set; }
    public SessionInfo? Session { get; set; }
    public int LastWalletEventId { get; set; }
    public int NextRpcRequestId { get; set; }
    public ConnectEvent ConnectEvent { get; set; }
}

public struct ConnectEvent
{
    public string Event { get; set; }
    public ConnectEventPayload Payload { get; set; }
}

public struct ConnectEventPayload
{
    public TonAddressItemReply[] Items { get; set; }
}

public class TonAddressItemReply
{
    public string? Name { get; } = "ton_addr";
    public string? Address { get; set; }
    public CHAIN Network { get; set; }
    public string? WalletStateInit { get; set; }
    public string? PublicKey { get; set; }
}

public enum CHAIN
{
    MAINNET = -239,
    TESTNET = -3
}
