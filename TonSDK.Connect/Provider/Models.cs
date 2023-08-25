using Newtonsoft.Json;
using Org.BouncyCastle.Bcpg.Sig;
using static LaunchDarkly.Logging.LogCapture;
using System;

namespace TonSdk.Connect;

public struct BridgeIncomingMessage
{
    [JsonProperty("from")] public string? From { get; set; }
    [JsonProperty("message")] public string? Message { get; set; }
}

public interface RpcRequest 
{
    public string method { get; set; }
    public string id { get; set; }
};

public class DisconnectRpcRequest : RpcRequest
{
    public string method { get; set; } = "disconnect";

    public object[] @params;
    public string id { get; set; }
}

public class ConnectAdditionalRequest
{
    public string? TonProof { get; set; }
}

public interface IConnectItem 
{
    public string? name { get; set; }
}

public class ConnectRequest
{
    public string? manifestUrl { get; set; }
    public IConnectItem[] items { get; set; }
}

public class ConnectAddressItem : IConnectItem
{
    public string? name { get; set; }
}

public class ConnectProofItem : IConnectItem
{
    public string? name { get; set; }
    public string? payload { get; set; }
}

public struct ConnectionInfo
{
    public string? Type { get; set; }
    public SessionInfo? Session { get; set; }
    public int? LastWalletEventId { get; set; }
    public int? NextRpcRequestId { get; set; }
    public dynamic ConnectEvent { get; set; }
}

public enum CHAIN
{
    MAINNET = -239,
    TESTNET = -3
}

public enum CONNECT_EVENT_ERROR_CODE
{
    UNKNOWN_ERROR = 0,
    BAD_REQUEST_ERROR = 1,
    MANIFEST_NOT_FOUND_ERROR = 2,
    MANIFEST_CONTENT_ERROR = 3,
    UNKNOWN_APP_ERROR = 100,
    USER_REJECTS_ERROR = 300,
    METHOD_NOT_SUPPORTED = 400
}

public struct ConnectErrorData
{
    public CONNECT_EVENT_ERROR_CODE Code { get; set; }
    public string Message { get; set; }
}

public class ConnectEventParser
{
    public static Wallet ParseResponse(dynamic payload)
    {
        if (payload.items == null) throw new TonConnectError("items not contains in payload");

        Wallet wallet = new();

        foreach (var item in payload.items)
        {
            if(item.name != null)
            {
                if ((string)item.name == "ton_addr") wallet.Account = Account.Parse(item);
                else if ((string)item.name == "ton_proof") wallet.TonProof = TonProof.Parse(item);
            }
        }

        if (wallet.Account == null) throw new TonConnectError("ton_addr not contains in items");
        wallet.Device = DeviceInfo.Parse(payload.device);

        return wallet;
    }

    public static ConnectErrorData ParseError(dynamic payload)
    {
        ConnectErrorData data = new()
        {
            Code = (CONNECT_EVENT_ERROR_CODE)payload.code,
            Message = payload.message.ToString()
        };
        return data;
    }
}


//    def parse_error(payload: dict) -> TonConnectError:
//        error_constructor: TonConnectError = UnknownError

//        code = payload.get('error', {}).get('code', None)
//        if code is not None and code in CONNECT_EVENT_ERRORS:
//error_constructor = CONNECT_EVENT_ERRORS[code]


//        message = payload.get('error', { }).get('message', None)
//        return error_constructor(message)