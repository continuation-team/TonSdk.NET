# Sending Jettons

Its possible to send Jettons to any account using wallet contracts presented in `TonSdk.Contracts.Wallet` and `JettonWallet` abstractions presented in `TonSdk.Contracts.Jetton`.

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

// define the address of the jetton master contract and jetton wallet owner
Address jettonMasterContract = new Address("/* jetton contract address */");

// get the jetton wallet address using TonClient: TonSdk.Client
Address jettonWallet = await tonClient.Jetton.GetWalletAddress(jettonMasterContract, wallet.address);

// define receiver address or get receiver address from ton domain name system
// Address receiver = new Address("/* destination address */");
Address receiver = await tonClient.Dns.GetWalletAddress("continuation.ton");

// define jetton amount to send
Coins amount = new Coins(100); // for ex 100 jettons

// create jetton transfer options
JettonTransferOptions options = new JettonTransferOptions()
{
    Amount = amount,
    Destination = receiver
};

// create a message body for the jetton transfer
Cell jettonTransfer = JettonWallet.CreateTransferRequest(options);

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
                Dest = jettonWallet,
                Value = new Coins(0.1), // amount in TONs to send
            }),
            Body = jettonTransfer
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
