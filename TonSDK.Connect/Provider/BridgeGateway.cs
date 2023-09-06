using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace TonSdk.Connect
{
    public delegate void SendGatewayMessage(string bridgeUrl, string postPath, string sessionId, string receiver, int ttl, string topic, byte[] message);
    public class BridgeGateway
    {
        private readonly int DEFAULT_TTL = 300;
        private readonly string SSE_PATH = "events";
        private readonly string POST_PATH = "message";

        public bool isClosed { get; private set; }
        private string _bridgeUrl;
        private string _sessionId;
        private ISSEClient _sseClient;
        public RemoteStorage _storage;

        private ProviderMessageHandler _handler;
        private ProviderErrorHandler _errorHandler;
        private ListenEventsFunction _eventsFunction;
        private SendGatewayMessage _sendGatewayMessage;

        public BridgeGateway(string bridgeUrl, string sessionId, ProviderMessageHandler handler, ProviderErrorHandler errorHandler, RemoteStorage storage, ListenEventsFunction eventsFunction, SendGatewayMessage sendGatewayMessage)
        {
            isClosed = false;
            _bridgeUrl = bridgeUrl;
            _sessionId = sessionId;

            _handler = handler;
            _errorHandler = errorHandler;
            _storage = storage;
            _eventsFunction = eventsFunction;
            _sendGatewayMessage = sendGatewayMessage;
            _sseClient = null;
        }

        public async Task RegisterSession()
        {
            if (isClosed) return;

            string bridgeBase = _bridgeUrl.TrimEnd('/');
            string bridgeUrl = $"{bridgeBase}/{SSE_PATH}?client_id={_sessionId}";

            string? lastEventId = _storage != null ? _storage.GetItem(RemoteStorage.KEY_LAST_EVENT_ID) : await DefaultStorage.GetItem(DefaultStorage.KEY_LAST_EVENT_ID, null);
            if (lastEventId != null && lastEventId != "") bridgeUrl += $"&last_event_id={lastEventId}";
            await Console.Out.WriteLineAsync($"\"{bridgeUrl}\"");

            _sseClient?.StopClient();
            _sseClient = new SSEClient(bridgeUrl, _handler, _errorHandler, _eventsFunction);
            _sseClient?.StartClient();

        }

        public async Task Send(byte[] message, string receiver, string topic, int? ttl = null)
        {
            if(_sendGatewayMessage != null)
            {
                _sendGatewayMessage(_bridgeUrl, POST_PATH, _sessionId, receiver, ttl ?? DEFAULT_TTL, topic, message);
                return;
            }
            var url = new Uri($"{_bridgeUrl}/{POST_PATH}");
            var queryString = HttpUtility.ParseQueryString(url.Query);
            queryString["client_id"] = _sessionId;
            queryString["to"] = receiver;
            queryString["ttl"] = (ttl ?? DEFAULT_TTL).ToString();
            queryString["topic"] = topic.ToString();
            url = new Uri(url.GetLeftPart(UriPartial.Path) + "?" + queryString.ToString());

            using (var client = new HttpClient())
            {
                var content = new ByteArrayContent(message);
                await client.PostAsync(url, content);
            }
        }

        public void Pause()
        {
            _sseClient?.StopClient();
            _sseClient = null;
        }

        public async Task UnPause() => await RegisterSession();

        public void Close()
        {
            isClosed = true;
            Pause();
        }
    }
}
