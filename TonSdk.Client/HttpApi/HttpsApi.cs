using Newtonsoft.Json;
using TonSdk.Core;
using TonSdk.Core.Boc;
using static TonSdk.Client.Transformers;

namespace TonSdk.Client;

public class HttpApiParameters
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

    public async Task<AddressInformationResult> GetAddressInformation(Address address)
    {
        InAdressInformationBody requestBody = new(address.ToString(AddressType.Base64, new AddressStringifyOptions(true, false, false)));
        var result = await new TonRequest(new RequestParameters("getAddressInformation", requestBody, ApiOptions)).Call();
        RootAddressInformation resultAddressInformation = JsonConvert.DeserializeObject<RootAddressInformation>(result);
        AddressInformationResult addressInformationResult = new(resultAddressInformation.Result);
        return addressInformationResult;
    }

    public async Task<TransactionsInformationResult[]> GetTransactions(Address address, int limit = 10, int? lt = null, string? hash = null, int? to_lt = null, bool? archival = null)
    {
        InTransactionsBody requestBody = new() 
        { 
            address = address.ToString(AddressType.Base64, new AddressStringifyOptions(true, false, false)),
            limit = limit
        };
        if (lt != null) requestBody.lt = (int)lt;
        if (hash != null) requestBody.hash = hash;
        if (to_lt != null) requestBody.to_lt = (int)to_lt;
        if (archival != null) requestBody.archival = (bool)archival;

        var result = await new TonRequest(new RequestParameters("getTransactions", requestBody, ApiOptions)).Call();
        RootTransactions resultRoot = JsonConvert.DeserializeObject<RootTransactions>(result);

        TransactionsInformationResult[] transactionsInformationResult = new TransactionsInformationResult[resultRoot.Result.Length];
        for (int i = 0; i < resultRoot.Result.Length; i++)
        {
            transactionsInformationResult[i] = new TransactionsInformationResult(resultRoot.Result[i]);
        }

        return transactionsInformationResult;
    }

    public async Task<RunGetMethodResult> RunGetMethod(Address address, string method, string[][]? stack = null)
    {
        InRunGetMethodBody requestBody = new()
        {
            address = address.ToString(),
            method = method,
            stack = stack ?? Array.Empty<string[]>()
        };
        var result = await new TonRequest(new RequestParameters("runGetMethod", requestBody, ApiOptions)).Call();
        RootRunGetMethod resultRoot = JsonConvert.DeserializeObject<RootRunGetMethod>(result);
        RunGetMethodResult outRunGetMethod = new(resultRoot.Result);
        return outRunGetMethod;
    }

    public async Task<SendBocResult> SendBoc(Cell boc)
    {
        InSendBocBody requestBody = new()
        {
            boc = boc.ToString("base64")
        };
        var result = await new TonRequest(new RequestParameters("sendBoc", requestBody, ApiOptions)).Call();
        RootSendBoc resultRoot = JsonConvert.DeserializeObject<RootSendBoc>(result);
        SendBocResult outSendBoc = resultRoot.Result;
        return outSendBoc;
    }

    public async Task<ConfigParamResult> GetConfigParam(int configId, int? seqno = null)
    {
        InGetConfigParamBody requestBody = new()
        {
            config_id = configId,
        };
        if(seqno != null) { requestBody.seqno = (int)seqno; }
        var result = await new TonRequest(new RequestParameters("getConfigParam", requestBody, ApiOptions)).Call();
        RootGetConfigParam resultRoot = JsonConvert.DeserializeObject<RootGetConfigParam>(result);
        ConfigParamResult outConfigParam = new(resultRoot.Result.Config);
        return outConfigParam;
    }
}
