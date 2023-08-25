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

public class ConnectEventParser
{
    public static Wallet ParseResponse(dynamic payload)
    {
        if (payload.items == null) throw new TonConnectError("items not contains in payload");

        Wallet wallet = new();
        wallet.TonProof = payload.items[0];

        foreach(var item in payload.items)
        {
            if(item.name != null)
            {
                if (item.name == "ton_addr") wallet.Account = new Account(); // Доделать парсинг аккаунта
                else if (item.name == "ton_proof") wallet.TonProof = new TonProof(); // Доделать парсинг пруфа
            }
        }

        if (wallet.Account == null) throw new TonConnectError("ton_addr not contains in items");
        wallet.Device = new DeviceInfo(); // сделать парсинг девайса

        return wallet;
    }
}
//    def parse_response(payload: dict) -> WalletInfo:
//        if 'items' not in payload:
//            raise TonConnectError('items not contains in payload')

//        wallet = WalletInfo()

//        for item in payload['items']:
//            if 'name' in item:
//                if item['name'] == 'ton_addr':
//                    wallet.account = Account.from_dict(item)
//                elif item['name'] == 'ton_proof':
//                    wallet.ton_proof = TonProof.from_dict(item)

//        if wallet.account is None:
//            raise TonConnectError('ton_addr not contains in items')

//        wallet.device = DeviceInfo.from_dict(payload['device'])

//        return wallet


//    def parse_error(payload: dict) -> TonConnectError:
//        error_constructor: TonConnectError = UnknownError

//        code = payload.get('error', {}).get('code', None)
//        if code is not None and code in CONNECT_EVENT_ERRORS:
//error_constructor = CONNECT_EVENT_ERRORS[code]


//        message = payload.get('error', { }).get('message', None)
//        return error_constructor(message)