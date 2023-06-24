using TonSdk.Core;

namespace TonSdk.Client.HttpApi;

public struct HttpApiParameters
{
    public string Endpoint { get; set; }
    public int? Timeout { get; set; }
    public string? ApiKey { get; set; }
}

public class HttpApi
{
    protected readonly HttpApiParameters ApiOptions;

    public HttpApi(HttpApiParameters options)
    {
        if(options.Endpoint == null || options.Endpoint.Length == 0) { throw new ArgumentNullException("Endpoint field in Http options cannot be null."); }
        
        this.ApiOptions = new HttpApiParameters
        {
            Endpoint = options.Endpoint,
            Timeout = options.Timeout ?? 30000,
            ApiKey = options.ApiKey ?? ""
        };        
    }

    public async Task<object> GetAddressInformation(Address address)
    {
        InAdressInformationBody requestBody = new(address.ToString(AddressType.Base64, new AddressStringifyOptions(true, false, false)));
        var result = await new TonRequest(new RequestParameters("getAddressInformation", requestBody, ApiOptions)).Call();
        return result;
    }
}
