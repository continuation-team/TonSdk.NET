using Newtonsoft.Json;
namespace TonSdk.Client;

public interface IRequestBody { }

public struct RequestParameters
{
    public string MethodName { get; private set; }
    public IRequestBody RequestBody { get; private set; }
    public HttpApiParameters HttpApiParameters { get; private set; }

    public RequestParameters(string methodName, IRequestBody requestBody, HttpApiParameters httpApiParameters)
    {
        MethodName = methodName;
        RequestBody = requestBody;
        HttpApiParameters = httpApiParameters;
    }
}

public class TonRequest
{
    private readonly RequestParameters _params;
    public TonRequest(RequestParameters _params) => this._params = _params;

    public async Task<string> Call()
    {
        HttpClient httpClient = new();
        httpClient.Timeout = TimeSpan.FromMilliseconds(Convert.ToDouble(_params.HttpApiParameters.Timeout));
        //httpClient.DefaultRequestHeaders.Add("Content-Type", "application/json");
        
        if(_params.HttpApiParameters.ApiKey != null && _params.HttpApiParameters.ApiKey != string.Empty)
            httpClient.DefaultRequestHeaders.Add("X-API-Key", _params.HttpApiParameters.ApiKey);

        string data = JsonConvert.SerializeObject(new
        {
            id = "1",
            jsonrpc = "2.0",
            method = _params.MethodName,
            @params = _params.RequestBody != null ? _params.RequestBody : null
        });

        StringContent content = new(data, System.Text.Encoding.UTF8, "application/json");
        HttpResponseMessage response = await httpClient.PostAsync(_params.HttpApiParameters.Endpoint, content);

        if (!response.IsSuccessStatusCode) throw new Exception($"Received error: {await response.Content.ReadAsStringAsync()}");
        string result = await response.Content.ReadAsStringAsync();

        return result;
    }
}
