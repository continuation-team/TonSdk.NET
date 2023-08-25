namespace TonSdk.Connect;

public struct Wallet
{
    public DeviceInfo Device { get; set; }
    public string Provider { get; set; }
    public Account Account { get; set; }
    public TonProof TonProof { get; set; } 
}

public class DeviceInfo
{
    public string? Platform { get; set; }
    public string? AppName { get; set; }
    public string? AppVersion { get; set; }
    public int MaxProtocolVersion { get; set; }

    public object[]? Features { get; set; }
}

public class Account
{
    public string? Address { get; set; }
    public CHAIN Chain { get; set; }
    public string? WalletStateInit { get; set; }
    public string? PublicKey { get; set; }
}

public class TonProof
{
    public int Timestamp { get; set; }
    public int DomainLen { get; set; }
    public string? DomainVal { get; set; }
    public string? Payload { get; set; }
    public byte[]? Signature { get; set; }
}
