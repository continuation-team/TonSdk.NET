namespace TonSdk.Connect;

public class TonConnect
{
    public ConnectRequest CreateConnectRequest(ConnectAdditionalRequest? connectAdditionalRequest)
    {
        ConnectRequest connectRequest = new();
        connectRequest.manifestUrl = "https://dedust.io/tonconnect-manifest.json";
        connectRequest.items = new ConnectItem[1];
        connectRequest.items[0] = new ConnectAddressItem() { name = "ton_addr" };
        return connectRequest;
    }
}
