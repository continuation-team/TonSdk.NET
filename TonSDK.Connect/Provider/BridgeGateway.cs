using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace TonSdk.Connect
{
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

        public BridgeGateway(string bridgeUrl, string sessionId, ProviderMessageHandler handler, ProviderErrorHandler errorHandler, RemoteStorage storage, ListenEventsFunction eventsFunction)
        {
            isClosed = false;
            _bridgeUrl = bridgeUrl;
            _sessionId = sessionId;

            _handler = handler;
            _errorHandler = errorHandler;
            _storage = storage;
            _eventsFunction = eventsFunction;
            _sseClient = null;
        }

        public async Task RegisterSession()
        {
            if (isClosed) return;

            string bridgeBase = _bridgeUrl.TrimEnd('/');
            string bridgeUrl = $"{bridgeBase}/{SSE_PATH}?client_id={_sessionId}";

            string? lastEventId = _storage != null ? _storage.GetItem(RemoteStorage.KEY_LAST_EVENT_ID) : await DefaultStorage.GetItem(DefaultStorage.KEY_LAST_EVENT_ID);
            if (lastEventId != null) bridgeUrl += $"&last_event_id={lastEventId}";
            await Console.Out.WriteLineAsync($"\"{bridgeUrl}\"");

            _sseClient?.StopClient();
            _sseClient = new SSEClient(bridgeUrl, _handler, _errorHandler, _eventsFunction);
            _sseClient?.StartClient();

        }

        public async Task Send(string request, string receiverPublicKey, string topic, int? ttl = null)
        {
            string bridgeBase = _bridgeUrl.TrimEnd('/');
            string bridgeUrl = $"{bridgeBase}/{POST_PATH}?client_id={_sessionId}";
            bridgeUrl += $"&to={receiverPublicKey}";
            bridgeUrl += $"&ttl={ttl ?? DEFAULT_TTL}";
            bridgeUrl += $"&topic={topic}";

            using HttpClient client = new HttpClient();
            StringContent content = new StringContent(request);
            await client.PostAsync(bridgeUrl, content);
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
