# Sending TONs

Its possible to send TONs to any account using wallet contracts presented in `TonSdk.Contracts.Wallet`.

{% hint style="info" %}
For example, we will use `WalletV4`. You can find more examples for different contracts [here](../contracts/wallet/).
{% endhint %}

```csharp
// create http parameters for ton client 
HttpParameters tonClientParams = new HttpParameters 
{
    Endpoint = "https://toncenter.com/api/v2/jsonRPC",
    ApiKey = "xxx" 
};

// create ton client to fetch data and send boc
TonClient tonClient = new TonClient(TonClientType.HTTP_TONCENTERAPIV2, tonClientParams);

// create new mnemonic or use existing
Mnemonic mnemonic = new Mnemonic();

// create wallet options
WalletV4Options options = new WalletV4Options()
{
    PublicKey = mnemonic.Keys.PublicKey,
};

// create wallet instance
WalletV4 wallet = new WalletV4(optionsV4, 2); 

// define receiver address or get receiver address from ton domain name system
// Address receiver = new Address("/* destination address */");
Address receiver = await tonClient.Dns.GetWalletAddress("continuation.ton");

// define amount to send
Coins amount = new Coins(2); // 2 TON

// define text message to send
string memo = "Hello world!";

// create transaction body query + memo
Cell body = new CellBuilder().StoreUInt(0, 32).StoreString(memo).Build();

// getting seqno using tonClient
uint? seqno = await tonClient.Wallet.GetSeqno(wallet.Address);

// create transfer message
ExternalInMessage message = wallet.CreateTransferMessage(new[]
{
    new WalletTransfer
    {
        Message = new InternalMessage(new InternalMessageOptions
        {
            Info = new IntMsgInfo(new IntMsgInfoOptions
            {
                Dest = receiver,
                Value = amount,
                Bounce = true // make bounceable message
            }),
            Body = body
        }),
        Mode = 1 // message mode
    }
}, seqno ?? 0); // if seqno is null we pass 0, wallet will auto deploy on message send

// sign transfer message
message.Sign(mnemonic.Keys.PrivateKey);

// get message cell
Cell cell = message.Cell;

// send this message via TonClient
await tonClient.SendBoc(cell);
```
