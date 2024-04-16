# Sending NFTs

Its possible to send Nft items to any account using wallet contracts presented in `TonSdk.Contracts.Wallet` and `NftItem` abstractions presented in `TonSdk.Contracts.nft`.

{% hint style="info" %}
For example, we will use `WalletV4`. You can find more examples for different contracts [here](https://docs.tonsdk.net/user-manual/contracts/wallet).
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

// define the address of the nft collection
Address collection = new Address("/* nft collection address */");

// define index of the nft what will send
uint index = 10;

// get the nft items address using TonClient: TonSdk.Client
Address nftItemAddress = await tonclient.Nft.GetItemAddress(collection, index);

// define receiver address or get receiver address from ton domain name system
// Address receiver = new Address("/* destination address */");
Address receiver = await tonClient.Dns.GetWalletAddress("continuation.ton");

// create transfer options
NftTransferOptions options = new NftTransferOptions()
{
    NewOwner = receiver
};

// create a message body for the nft transfer
Cell nftTransfer = NftItem.CreateTransferRequest(options);

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
                Dest = nftItemAddress,
                Value = new Coins(0.1), // amount in TONs to send
            }),
            Body = nftTransfer
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
