using LaunchDarkly.EventSource;
using System.Diagnostics;
using System.Diagnostics.Tracing;

namespace TonSdk.Connect;
public class BridgeGateway
{
    private readonly int DEFAULT_TTL = 300;
    private readonly string SSE_PATH = "events";
    private readonly string POST_PATH = "message";

    private bool _isClosed;
    private string _bridgeUrl;
    private string _sessionId;
    private SSEClient? _sseClient;

    private ProviderMessageHandler _handler;
    private ProviderErrorHandler _errorHandler;

    public BridgeGateway(string bridgeUrl, string sessionId, ProviderMessageHandler handler, ProviderErrorHandler errorHandler)
    {
        _isClosed = false;
        _bridgeUrl = bridgeUrl;
        _sessionId = sessionId;

        _sseClient = null;
        _handler = handler;
        _errorHandler = errorHandler;
    }

    public async Task RegisterSession()
    {
        if(_isClosed) return;

        string bridgeBase = _bridgeUrl.TrimEnd('/');
        string bridgeUrl = $"{bridgeBase}/{SSE_PATH}?client_id={_sessionId}";

        string? lastEventId = await DefaultStorage.GetItem(DefaultStorage.KEY_LAST_EVENT_ID);
        if(lastEventId != null) bridgeUrl += $"&last_event_id={lastEventId}";
        await Console.Out.WriteLineAsync($"Bridge URL: {bridgeUrl}");

        _sseClient?.Close();
        _sseClient = new(bridgeUrl, _handler, _errorHandler);
        await _sseClient.StartAsync();
        //_eventSource.MessageReceived += MessageHandler;
        //_eventSource.Error += ErrorHandler;
        //await _eventSource.StartAsync();
    }

    public async Task Send(string request, string receiverPublicKey, string topic, int? ttl = null)
    {
        string bridgeBase = _bridgeUrl.TrimEnd('/');
        string bridgeUrl = $"{bridgeBase}/{POST_PATH}?client_id={_sessionId}";
        bridgeUrl += $"&to={receiverPublicKey}";
        bridgeUrl += $"&ttl={ttl ?? DEFAULT_TTL}";
        bridgeUrl += $"&topic={topic}";

        using HttpClient client = new();
        StringContent content = new(request);
        await client.PostAsync(bridgeUrl, content);
    }

    public void Pause()
    {
        _sseClient?.Close();
        _sseClient = null;
    }

    public async Task UnPause() => await RegisterSession();

    public void Close()
    {
        _isClosed = true;
        Pause();
    }
}
