using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace TonSdk.Connect
{
    public delegate void ListenEventsFunction(CancellationToken token, string url, ProviderMessageHandler handler, ProviderErrorHandler errorHandler);

    public interface ISSEClient 
    {
        public void StartClient();
        public void StopClient();
    }

    public class SSEClient : ISSEClient
    {
        private readonly string _url;
        private readonly HttpClient _httpClient;
        private bool _isRunning;
        private CancellationTokenSource? _cancellationTokenSource;

        private ProviderMessageHandler _handler;
        private ProviderErrorHandler _errorHandler;
        private ListenEventsFunction eventsFunction;

        public SSEClient(string url, ProviderMessageHandler handler, ProviderErrorHandler errorHandler, ListenEventsFunction listenEventsFunction)
        {
            _url = url;
            _httpClient = new HttpClient();
            _isRunning = false;

            _handler = handler;
            _errorHandler = errorHandler;
            eventsFunction = listenEventsFunction;
        }

        public void StartClient()
        {
            if (_isRunning) return;
            _isRunning = true;

            _cancellationTokenSource = new CancellationTokenSource();

            if (eventsFunction == null) ListenForEvents(_cancellationTokenSource.Token);
            else eventsFunction(_cancellationTokenSource.Token, _url, _handler, _errorHandler);
        }

        public void StopClient()
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

                using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                using var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
                using var reader = new StreamReader(stream);
                while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
                {
                    var line = await reader.ReadLineAsync().ConfigureAwait(false);
                    if (!string.IsNullOrWhiteSpace(line)) _handler(line);
                }
            }
            catch (Exception ex)
            {
                _errorHandler(ex);
            }
        }
    }
}
