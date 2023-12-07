## TonSdk.Adnl


### Overview example

```csharp
// Define constants for the ADNL client configuration
private static readonly int AdnlHost = 84478511; // Host ID for ADNL client
private static readonly int AdnlPort = 19949; // Port number for ADNL client
private static readonly string AdnlPubKey = "n4VDnSCUuSpjnCyUk9e3QOOd6o0ItSWYbTnW3Wnn8wk="; // Public key for ADNL client

// Initialize the ADNL client with the predefined settings
AdnlClientTcp client = new AdnlClientTcp(AdnlHost, AdnlPort, AdnlPubKey);

// Subscribe to various client events
client.Connected += OnClientConnected; // Event when client connected, but not ready
client.Ready += OnClientReady; // Event when client is ready
client.Closed += OnClientClosed; // Event when client connection closes
client.DataReceived += OnDataReceived; // Event when data is received
client.ErrorOccurred += OnErrorOccured; // Event when an error occurs

// Connect the client and wait for input before closing
await client.Connect();

// Event handlers for the ADNL client
private static void OnClientConnected()
{
    Console.WriteLine("Connected");
}

private static void OnClientClosed()
{
    Console.WriteLine("Closed");
}

private static void OnDataReceived(byte[] data)
{
    Console.WriteLine("Data Received");
    Console.WriteLine(Utils.BytesToHex(data));
}

private static async void OnClientReady()
{
    Console.WriteLine("Ready");
    
    // Send a predefined byte array (TL) to the client
    byte[] tlGettime = Utils.HexToBytes("7af98bb435263e6c95d6fecb497dfd0aa5f031e7d412986b5ce720496db512052e8f2d100cdf068c7904345aad16000000000000");
    await client.Write(tlGettime);
}

private static void OnErrorOccured(Exception ex)
{
    throw ex;
}
```

#### Write byte data method
```csharp
// Write byte array via NetworkStream. Should be predefined by using TL Schemes 
// TL docs: https://core.telegram.org/mtproto/TL or https://docs.ton.org/develop/data-formats/tl
await client.Write(byte[] data);
```

#### End connection method
```csharp
// Close and Dispose connection to Liteserver
client.End();
```

### LiteClient overview example

```csharp
// Initialize the Lite client with the predefined settings
LiteClient liteClient = new LiteClient(AdnlHost, AdnlPort, AdnlPubKey);

// Connect the client
await liteClient.Connect();

// It is optimal to send a ping packet once every 5 seconds.
// This is necessary to maintain the connection while no data is being transmitted,
// otherwise the server may terminate the connection.
await client.PingPong();

// RunSmcMethod example
Address destination = new Address("xxx");
RunSmcMethodResult result = await liteClient.RunSmcMethod(destination, "seqno", Array.Empty<byte>(), new RunSmcOptions() 
{ Result = true });

// GetMasterChainInfo example
MasterChainInfo mcInfo = await liteClient.GetMasterChainInfo();
BlockIdExtended blockId = mcInfo.LastBlockId;

// Disconnect the client
liteClient.Disconnect();
```

## Donation

`continuation.ton`

## License

MIT License
