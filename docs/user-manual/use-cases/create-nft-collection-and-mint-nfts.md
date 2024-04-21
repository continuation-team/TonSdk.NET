# Create Nft Collection and Mint Nfts

In this topic you will know how to create new Nft collection and mint new Nfts using `TonSDK.Net`

{% hint style="info" %}
For example, we will use `WalletV4`. You can find more examples for different contracts [here](https://docs.tonsdk.net/user-manual/contracts/wallet).
{% endhint %}

**Create new collection**

First of all, you will need to create a collection using `NftCollection` class in `TonSdk.Contracts.nft`:

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

// define nft collections options
NftCollectionOptions opts = new NftCollectionOptions()
{
    OwnerAddress = wallet.Address, // collection owner address
    RoyaltyAddress = wallet.Address, // address to receiving royalty payments
    CollectionContentUri = "https://example.com/metadata.json", // collection metadata
    NftItemContentBaseUri = "https://example.com/nfts/", // nft content base
    Royalty = 0.1 // 10%, for example 0.05 - 5%, 0.3 - 30% etc
};

// create new NftCollection instance using options
NftCollection collection = new NftCollection(opts);

// getting seqno using tonClient
uint? seqno = await tonClient.Wallet.GetSeqno(wallet.Address);

// creating collection deploy message
var msg = wallet.CreateTransferMessage(new[]
{
    new WalletTransfer
    {
        Message = new InternalMessage(new InternalMessageOptions
        {
            Info = new IntMsgInfo(new IntMsgInfoOptions
            {
                Dest = collection.Address,
                Value = new Coins(0.05)
            }),
            Body = null,
            StateInit = collection.StateInit
        }),
        Mode = 3 // message mode
    }
}, seqno ?? 0).Sign(m.Keys.PrivateKey);

// send this message via TonClient
await client.SendBoc(msg.Cell);

// print collection contract address
Console.WriteLine(collection.Address);
```

#### Mint new nfts

After that collection created, you can create nft mint message using `CreateMintRequest` method:

```csharp
// we will use same wallet and collection instance like in prev code block

// create nft mint options
NftMintOptions mintOptions = new NftMintOptions()
{
    ItemIndex = 0, // item index to mint, if collection is empty then 0
    Amount = new Coins(0.05), // amount send to nft item contract
    ItemOwnerAddress = wallet.Address, // address which will own new nft
    ItemContentUri = "0.json" // nft content will be splitted with {baseUri}{contentUri} 
};

// create nft mint request body
Cell nftMintBody = NftCollection.CreateMintRequest(mintOptions);

// getting seqno using tonClient
uint? seqno = await tonClient.Wallet.GetSeqno(wallet.Address);

// creating mint message
var msg = wallet.CreateTransferMessage(new[]
{
    new WalletTransfer
    {
        Message = new InternalMessage(new InternalMessageOptions
        {
            Info = new IntMsgInfo(new IntMsgInfoOptions
            {
                Dest = collection.Address,
                Value = new Coins(0.05)
            }),
            Body = nftMintBody
        }),
        Mode = 3 // message mode
    }
}, seqno ?? 0).Sign(m.Keys.PrivateKey);

// send this message via TonClient
await client.SendBoc(msg.Cell);

// print item address
Console.WriteLine(await client.Nft.GetItemAddress(collection.Address, 0));
```
