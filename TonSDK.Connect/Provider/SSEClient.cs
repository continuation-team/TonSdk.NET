using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TonSdk.Connect;

public class SSEClient
{
    private readonly string _url;
    private readonly HttpClient _httpClient;
    private bool _isRunning;
    private CancellationTokenSource? _cancellationTokenSource;

    private ProviderMessageHandler _handler;
    private ProviderErrorHandler _errorHandler;

    public SSEClient(string url, ProviderMessageHandler handler, ProviderErrorHandler errorHandler)
    {
        _url = url;
        _httpClient = new HttpClient();
        _isRunning = false;

        _handler = handler;
        _errorHandler = errorHandler;
    }

    public async Task StartAsync()
    {
        if (_isRunning) return;
        _isRunning = true;

        _cancellationTokenSource = new CancellationTokenSource();

        ListenForEvents(_cancellationTokenSource.Token);
    }

    public void Close()
    {
        if (!_isRunning) return;

        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _isRunning = false;
    }

    private async void ListenForEvents(CancellationToken cancellationToken)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, _url);
            request.Headers.Add("Accept", "text/event-stream");

            using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var reader = new StreamReader(stream);
            while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
            {
                var line = await reader.ReadLineAsync(cancellationToken);
                if (!string.IsNullOrWhiteSpace(line)) _handler(line);
            }
        }
        catch (Exception ex)
        {
            _errorHandler(ex);
        }
    }
}
