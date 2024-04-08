using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace TonSdk.Client
{
    public interface IRequestBody
    {
    }

    public struct RequestParameters
    {
        public string MethodName { get; private set; }
        public IRequestBody RequestBody { get; private set; }
        public RequestParameters(string methodName, IRequestBody requestBody)
        {
            MethodName = methodName;
            RequestBody = requestBody;
        }
    }

    public class TonRequest
    {
        private readonly RequestParameters _params;
        private readonly HttpClient _httpClient;

        public TonRequest(RequestParameters requestParameters, HttpClient httpClient)
        {
            _params = requestParameters;
            _httpClient = httpClient;
        }

        public async Task<string> Call()
        {
            string data = JsonConvert.SerializeObject(new
            {
                id = "1",
                jsonrpc = "2.0",
                method = _params.MethodName,
                @params = _params.RequestBody != null ? _params.RequestBody : null
            });

            StringContent content = new StringContent(data, System.Text.Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _httpClient.PostAsync(string.Empty, content);

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Received error: {await response.Content.ReadAsStringAsync()}");
            string result = await response.Content.ReadAsStringAsync();

            return result;
        }
    }
}