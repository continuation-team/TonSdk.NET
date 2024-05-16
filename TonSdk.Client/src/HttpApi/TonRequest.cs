using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

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
    

    
    public struct RequestParametersV3
    {
        public string MethodName { get; private set; }
        public Dictionary<string, object> RequestBody { get; private set; }
        public RequestParametersV3(string methodName, Dictionary<string, object> requestBody)
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
    
    public class TonRequestV3
    {
        private readonly RequestParametersV3 _params;
        private readonly HttpClient _httpClient;

        public TonRequestV3(RequestParametersV3 requestParameters, HttpClient httpClient)
        {
            _params = requestParameters;
            _httpClient = httpClient;
        }
        
        public async Task<string> CallGetList(List<string[]> list)
            {
                var builder = new UriBuilder(_httpClient.BaseAddress + _params.MethodName);
                StringBuilder queryString = new StringBuilder();
        
                bool isFirstParam = true;
                foreach (var param in list)
                {
                    if (param.Length == 2)
                    {
                        if (!isFirstParam)
                            queryString.Append('&');
                        queryString.AppendFormat("{0}={1}", Uri.EscapeDataString(param[0]), Uri.EscapeDataString(param[1]));
                        isFirstParam = false;
                    }
                }
        
                builder.Query = queryString.ToString();
                string url = builder.ToString();
        
                var response = await _httpClient.GetAsync(url);
        
                if (response.StatusCode == HttpStatusCode.Conflict)
                    return "conflict";
                if (!response.IsSuccessStatusCode)
                    throw new Exception($"Received error: {await response.Content.ReadAsStringAsync()}");
        
                string result = await response.Content.ReadAsStringAsync();
                return result;
            }

        public async Task<string> CallGet()
        {
            var builder = new UriBuilder(_httpClient.BaseAddress + _params.MethodName);
            var query = HttpUtility.ParseQueryString(string.Empty);
            foreach (string key in _params.RequestBody.Keys)
            {
                query[key] = _params.RequestBody[key].ToString();
            }
            builder.Query = query.ToString();
            string url = builder.ToString();
            
            var response = await _httpClient.GetAsync(url);

            if (response.StatusCode == HttpStatusCode.Conflict)
                return "conflict";
            if (!response.IsSuccessStatusCode)
                throw new Exception($"Received error: {await response.Content.ReadAsStringAsync()}");
            
            string result = await response.Content.ReadAsStringAsync();

            return result;
        }
        
        public async Task<string> CallPost()
        {
            try
            {
                var builder = new UriBuilder(_httpClient.BaseAddress + _params.MethodName);
                string url = builder.ToString();

                string data = JsonConvert.SerializeObject(_params.RequestBody);
                var content = new StringContent(data, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync(url, content);
                if (!response.IsSuccessStatusCode)
                    throw new Exception($"Received error: {await response.Content.ReadAsStringAsync()}");
                return await response.Content.ReadAsStringAsync();
            }
            catch (HttpRequestException e)
            {
                return $"Error: {e.Message}";
            }
        }
    }
}