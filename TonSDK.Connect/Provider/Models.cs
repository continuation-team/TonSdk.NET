using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TonSdk.Core;

namespace TonSdk.Connect
{
    internal interface IProvider 
    {
        public void CloseConnection();
        
        public void Listen(WalletEventListener listener);
        public Task<JObject> SendRequest(IRpcRequest request, OnRequestSentHandler onRequestSent = null);
    }

    internal interface IHttpProvider : IProvider 
    {
        public const string Type = "http";
        public Task<string> ConnectAsync(ConnectRequest connectRequest);
        public Task Disconnect();
        public Task<bool> RestoreConnection();
        public void Pause();
        public Task UnPause();
        
    }

    internal interface IInternalProvider : IProvider
    {
        public const string Type = "injected";
        public void Connect(ConnectRequest connectRequest, int protocolVersion);
        public void Disconnect();
        public Task<bool> RestoreConnection(string key);
        public void ParseMessage(string message);
    }

    public class WebWalletInfo
    {
        public string name;
        public string app_name;
        public string tondns;
        public string image;
        public string about_url;
        public string platforms;
    }

    public class AdditionalConnectOptions
    {
        public ListenEventsFunction listenEventsFunction {get; set;}
        public SendGatewayMessage sendGatewayMessage {get; set;}
    }

    internal struct BridgeIncomingMessage
    {
        [JsonProperty("from")] public string? From { get; set; }
        [JsonProperty("message")] public string? Message { get; set; }
    }

    public interface IRpcRequest
    {
        public string method { get; set; }
        public string id { get; set; }
        public string[] @params {get; set;}
    };

    [Serializable]
    internal class DisconnectRpcRequest : IRpcRequest
    {
        public string method { get; set; } = "disconnect";

        public string[] @params {get; set;}
        public string id { get; set; }
    }

    [JsonObject]
    internal class SendTransactionRpcRequest : IRpcRequest
    {
        public string method { get; set; } = "sendTransaction";

        public string[] @params {get; set;}
        public string id { get; set; }
    }

    [JsonObject]
    public class ConnectAdditionalRequest
    {
        public string TonProof { get; set; }
    }

    public interface IConnectItem
    {
        [JsonProperty("name")] public string name { get; set; }
    }

    [JsonObject]
    public class ConnectRequest
    {
        [JsonProperty("manifestUrl")] public string manifestUrl { get; set; }
        [JsonProperty("items")] public IConnectItem[] items { get; set; }
    }

    [JsonObject]
    public class ConnectAddressItem : IConnectItem
    {
        [JsonProperty("name")] public string name { get; set; }
    }

    [JsonObject]
    public class ConnectProofItem : IConnectItem
    {
        [JsonProperty("name")] public string name { get; set; }
        [JsonProperty("payload")] public string payload { get; set; }
    }

    public struct ConnectionInfo
    {
        public string? Type { get; set; }
        public SessionInfo? Session { get; set; }
        public long? LastWalletEventId { get; set; }
        public int? NextRpcRequestId { get; set; }
        public JObject ConnectEvent { get; set; }
        public string JsBridgeKey { get; set; }
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
        public static Wallet ParseResponse(JObject payload)
        {
            if (payload["items"] == null) throw new TonConnectError("items not contains in payload");

            Wallet wallet = new Wallet();

            foreach (var item in payload["items"])
            {
                if (item["name"] != null)
                {
                    if ((string)item["name"] == "ton_addr") wallet.Account = Account.Parse((JObject)item);
                    else if ((string)item["name"] == "ton_proof") wallet.TonProof = TonProof.Parse((JObject)item);
                }
            }

            if (wallet.Account == null) throw new TonConnectError("ton_addr not contains in items");
            wallet.Device = DeviceInfo.Parse((JObject)payload["device"]);

            return wallet;
        }

        public static ConnectErrorData ParseError(JObject payload)
        {
            ConnectErrorData data = new ConnectErrorData()
            {
                Code = (CONNECT_EVENT_ERROR_CODE)(int)payload["code"],
                Message = payload["message"].ToString()
            };
            return data;
        }
    }

    public class ProviderModels
    {
        public class SendTransactionRequestSerialized
        {
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public long? valid_until { get; set; }
            public string network { get; set; }
            public string from { get; set; }
            public SendTransactionMessageSerialized[] messages { get; set; }

            public SendTransactionRequestSerialized(SendTransactionRequest request)
            {
                valid_until = request.ValidUntil;
                network = ((int)request.Network!).ToString();
                from = request.From!.ToString(AddressType.Raw);

                List<SendTransactionMessageSerialized> messagesList = new List<SendTransactionMessageSerialized>();
                foreach (Message message in request.Messages)
                {
                    messagesList.Add(new SendTransactionMessageSerialized(message));
                }
                messages = messagesList.ToArray();
            }
        }

        public class SendTransactionMessageSerialized
        {
            public string address { get; set; }
            public string amount { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public string? stateInit { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public string? payload { get; set; }

            public SendTransactionMessageSerialized(Message message)
            {
                address = message.Address.ToString();
                amount = message.Amount.ToNano();
                stateInit = message.StateInit?.ToString();
                payload = message.Payload?.ToString();
            }
        }
    }
}