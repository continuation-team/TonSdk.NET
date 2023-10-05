## TonSdk.Connect


### Overview example

```csharp
// Create a TonConnectOptions object with the manifest URL
TonConnectOptions options = new TonConnectOptions()
{
    ManifestUrl = "https://dedust.io/tonconnect-manifest.json"
};

// Initialize TonConnect with the specified options
TonConnect tonConnect = new(options);

// Get wallet configurations
WalletConfig[] walletsConfig = tonConnect.GetWallets();

// Set event handlers for status changes and errors
tonConnect.OnStatusChange(OnStatusChange, OnErrorChange);

// Method called when the wallet status changes
void OnStatusChange(Wallet wallet)
{
    Console.WriteLine("Wallet connected. Address: " + wallet.Account.Address + ". Platform: " + wallet.Device.Platform + "," + wallet.Device.AppName + "," + wallet.Device.AppVersion);
}

// Method called when errors occur
void OnErrorChange(string wallet)
{
    Console.WriteLine(wallet);
}

// Connect to a wallet and await the result
string result = await tonConnect.Connect(walletsConfig[1]);

// Print the connection result
Console.WriteLine("Connect link: " + result);

```

#### Disconnect wallet method
```csharp
// Call this method to disconnect from the current wallet
await tonConnect.Disconnect();
```

#### Use custom remote storage
```csharp
// Create a `RemoteStorage` object with custom storage methods provided by the user (for example Player Prefs in Unity)
RemoteStorage remoteStorage = new(new(PlayerPrefs.GetString), new(PlayerPrefs.SetString), new(PlayerPrefs.DeleteKey), new(PlayerPrefs.HasKey));

// Initialize TonConnect with the specified options and the custom storage methods
TonConnect tonConnect = new(options, remoteStorage);
```

#### Use additional connect options
```csharp
// This delegates uses, if platform as the Unity, needs reimplementation of httpClient logic, with using Coroutines.
// Dont use in usual C# projects.

// Create a custom SSE listen method with implementation of event stream subsctiptions.
// Example, https://github.com/continuation-team/unity-ton-connect/blob/main/Assets/TonConnect/Scripts/TonConnect/TonConnectHandler.cs#L147C5-L192C6
private void YourListenEventsMethod() {}

// Create a custom send post message method
// Example, https://github.com/continuation-team/unity-ton-connect/blob/main/Assets/TonConnect/Scripts/TonConnect/TonConnectHandler.cs#L119C5-L145C6
private void YourSendPostMethod() {}

// Create a `AdditionalConnectOptions` object with custom methods provided by the user (only in Unity)
AdditionalConnectOptions additionalOptions = new AdditionalConnectOptions()
{
    listenEventsFunction = new ListenEventsFunction(YourListenEventsMethod),
    sendGatewayMessage = new SendGatewayMessage(YourSendPostMethod)
}
```

#### Send transaction example
```csharp
// Create a receiver address and the amount to send
Address receiver = new("receiver address");
Coins amount = new("send amount");

// Create an array of messages to send
Message[] sendTons = 
{
    new Message(receiver, amount),
    // Additional messages if needed
};

// Set the valid until timestamp for the transaction (expiration time in seconds from the current moment)
long validUntil = DateTimeOffset.Now.ToUnixTimeSeconds() + 600;

// Create a SendTransactionRequest object
SendTransactionRequest transactionRequest = new SendTransactionRequest(sendTons, validUntil);

// Print the result of sending the transaction
Console.WriteLine(await tonConnect.SendTransaction(transactionRequest));
```

## Donation

`continuation.ton`

## License

MIT License
