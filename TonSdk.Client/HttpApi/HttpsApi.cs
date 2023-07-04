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

    /// <summary>
    /// Initializes a new instance of the HttpApi class with the specified options.
    /// </summary>
    /// <param name="options">The HTTP API parameters.</param>
    /// <exception cref="ArgumentNullException">Thrown when the endpoint field in the options is null.</exception>
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

    /// <summary>
    /// Retrieves the address information for the specified address.
    /// </summary>
    /// <param name="address">The address object to retrieve information for.</param>
    /// <returns>An object containing the address information.</returns>
    public async Task<AddressInformationResult> GetAddressInformation(Address address)
    {
        InAdressInformationBody requestBody = new(address.ToString(AddressType.Base64, new AddressStringifyOptions(true, false, false)));
        var result = await new TonRequest(new RequestParameters("getAddressInformation", requestBody, ApiOptions)).Call();
        RootAddressInformation resultAddressInformation = JsonConvert.DeserializeObject<RootAddressInformation>(result);
        AddressInformationResult addressInformationResult = new(resultAddressInformation.Result);
        return addressInformationResult;
    }

    /// <summary>
    /// Retrieves transaction information for the specified address.
    /// </summary>
    /// <param name="address">The address object to retrieve transaction information for.</param>
    /// <param name="limit">The maximum number of transactions to retrieve (default: 10).</param>
    /// <param name="lt">The logical time of the transaction to start retrieving from (optional).</param>
    /// <param name="hash">The hash of the transaction to start retrieving from (optional).</param>
    /// <param name="to_lt">The logical time of the transaction to retrieve up to (optional).</param>
    /// <param name="archival">Specifies whether to retrieve transactions from archival nodes (optional).</param>
    /// <returns>An array of transaction information results.</returns>
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

    /// <summary>
    /// Executes a specific method on the specified address.
    /// </summary>
    /// <param name="address">The address object to execute the method on.</param>
    /// <param name="method">The name of the method to execute.</param>
    /// <param name="stack">The stack parameters for the method (optional).</param>
    /// <returns>The result of the executed method.</returns>
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

    /// <summary>
    /// Sends a Bag of Cells (BoC) to the network.
    /// </summary>
    /// <param name="boc">The Cell object representing the Bag of Cells.</param>
    /// <returns>The result of sending the Bag of Cells.</returns>
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

    /// <summary>
    /// Retrieves a configuration parameter by its ID.
    /// </summary>
    /// <param name="configId">The ID of the configuration parameter to retrieve.</param>
    /// <param name="seqno">The sequence number of the configuration parameter (optional).</param>
    /// <returns>The result of the configuration parameter retrieval.</returns>
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
