using System.Security.Principal;
using System;

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

    public static DeviceInfo Parse(dynamic device)
    {
        DeviceInfo deviceInfo = new()
        {
            Platform = (string)device.platform,
            AppName = (string)device.appName,
            AppVersion = (string)device.appVersion,
            MaxProtocolVersion = (int)device.maxProtocolVersion,
            Features = device.features.ToObject<object[]>()
        };
        return deviceInfo;
    }
}

public class Account
{
    public string? Address { get; set; }
    public CHAIN Chain { get; set; }
    public string? WalletStateInit { get; set; }
    public string? PublicKey { get; set; }

    public static Account Parse(dynamic item)
    {
        if (item.address == null) throw new TonConnectError("address not contains in ton_addr");

        Account account = new()
        {
            Address = item.address.ToString(),
            Chain = (CHAIN)(int)item.network,
            WalletStateInit = item.walletStateInit.ToString(),
            PublicKey = item.publicKey?.ToString()
        };
        return account;
    }
}

public class TonProof
{
    public int Timestamp { get; set; }
    public int DomainLen { get; set; }
    public string? DomainVal { get; set; }
    public string? Payload { get; set; }
    public byte[]? Signature { get; set; }

    public static TonProof Parse(dynamic item)
    {
        if (item.proof == null) throw new TonConnectError("proof not contains in ton_proof");

        dynamic proof = item.proof;

        TonProof tonProof = new()
        {
            Timestamp = (int)proof.timestamp,
            DomainLen = (int)proof.domain.lengthBytes,
            DomainVal = (string)proof.domain.value,
            Payload = (string)proof.payload,
            Signature = Convert.FromBase64String((string)proof.signature)
        };
        return tonProof;
    }
}
