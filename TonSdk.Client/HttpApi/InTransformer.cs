using System.Net.NetworkInformation;
using TonSdk.Core;

namespace TonSdk.Client.HttpApi;

public struct InAdressInformationBody : IRequestBody
{
    public string address { get; set; }

    public InAdressInformationBody(string address) => this.address = address;
}

public class InGetTransactions : IRequestBody
{
    public Address? Address { get; set; }
    public int? Limit { get; set; }
    public string? Lt { get; set; }
    public string? Hash { get; set; }
    public string? ToLt { get; set; }
    public bool? Archival { get; set; }
}

public class InRunGetMethod : IRequestBody
{
    public Address? Address { get; set; }
    public string? Method { get; set; }
    public List<object>? Params { get; set; }
    public bool? Raw { get; set; }
}

public class InTrasformer
{
    //public static void GetAddressInformation(InAddressInformation options);
        

    /*public static InGetTransactions GetTransactions(InGetTransactions options)
    {

    }

    public static InRunGetMethod RunGetMethod(InRunGetMethod options)
    {

    }*/
}

