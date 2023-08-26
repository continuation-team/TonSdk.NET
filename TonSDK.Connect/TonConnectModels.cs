using System.Security.Principal;
using System;
using Newtonsoft.Json;

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

public class DeviceFeature
{

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
    public uint Timestamp { get; set; }
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
            Timestamp = (uint)proof.timestamp,
            DomainLen = (int)proof.domain.lengthBytes,
            DomainVal = (string)proof.domain.value,
            Payload = (string)proof.payload,
            Signature = Convert.FromBase64String((string)proof.signature)
        };
        return tonProof;
    }
}

public class SendTrasactionRequest
{
    /// <summary>
    /// Sending transaction deadline in unix epoch seconds.
    /// </summary>
    [JsonProperty("valid_until")] public long? ValidUntil { get; set; }

    /// <summary>
    /// The network (mainnet or testnet) where DApp intends to send the transaction. If not set, the transaction is sent to the network currently set in the wallet, but this is not safe and DApp should always strive to set the network. If the network parameter is set, but the wallet has a different network set, the wallet should show an alert and DO NOT ALLOW TO SEND this transaction.
    /// </summary>
    [JsonProperty("network")] public CHAIN? Network { get; set; }

    /// <summary>
    /// The sender address in wc:hex format from which DApp intends to send the transaction. Current account.address by default.
    /// </summary>
    [JsonProperty("from")] public string? From { get; set; }

    /// <summary>
    /// Messages to send: min is 1, max is 4.
    /// </summary>
    [JsonProperty("messages")] public Message[] Messages { get; set; }
}

public struct Message
{
    /// <summary>
    /// Receiver's address.
    /// </summary>
    [JsonProperty("address")] public string Address { get; set; }

    /// <summary>
    /// Amount to send in nanoTon.
    /// </summary>
    [JsonProperty("amount")] public string Amount { get; set; }

    /// <summary>
    /// Contract specific data to add to the transaction.
    /// </summary>
    [JsonProperty("stateInit")] public string? StateInit { get; set; }

    /// <summary>
    /// Contract specific data to add to the transaction.
    /// </summary>
    [JsonProperty("payload")] public string? Payload { get; set; }
}

public struct SendTransactionResult
{
    public string Boc { get; set; }
}
