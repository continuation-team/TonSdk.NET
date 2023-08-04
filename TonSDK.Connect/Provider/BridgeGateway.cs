using LaunchDarkly.EventSource;

namespace TonSdk.Connect;
public class BridgeGateway
{
    private readonly int DEFAULT_TTL = 300;
    private readonly string SSE_PATH = "events";
    private readonly string POST_PATH = "message";

    private bool _isClosed;
    private string _bridgeUrl;
    private string _sessionId;
    private EventSource? _eventSource;

    private ProviderMessageHandler _handler;
    private ProviderErrorHandler _errorHandler;

    public BridgeGateway(string bridgeUrl, string sessionId, ProviderMessageHandler handler, ProviderErrorHandler errorHandler)
    {
        _isClosed = false;
        _bridgeUrl = bridgeUrl;
        _sessionId = sessionId;

        _eventSource = null;
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

        _eventSource?.Close();

        _eventSource = new EventSource(new Uri(bridgeUrl));
        _eventSource.MessageReceived += MessageHandler;
        _eventSource.Error += ErrorHandler;
        await _eventSource.StartAsync();
    }

    // TODO: Implement sending message

    public void Pause()
    {
        _eventSource?.Close();
        _eventSource = null;
    }

    public async Task UnPause() => await RegisterSession();

    public void Close()
    {
        _isClosed = true;
        Pause();
    }

    private async void MessageHandler(object? sender, MessageReceivedEventArgs args)
    {
        if (_isClosed) return;
        await DefaultStorage.SetItem(DefaultStorage.KEY_LAST_EVENT_ID, args.Message.LastEventId);
        _handler(args.Message.Data);
    }

    private void ErrorHandler(object? sender, ExceptionEventArgs e)
    {
        if (_isClosed) return;
        _errorHandler(e);
    }
}
