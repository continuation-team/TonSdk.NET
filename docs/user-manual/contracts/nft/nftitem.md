---
description: TonSdk.Contracts.nft
---

# NftItem

`NftItem` is a class that contains method to work with transferring nft items.

It returns `Cell` in output and this cell can be setted like message body in `Wallet.CreateTransferMessage` .

{% hint style="info" %}
You can find tutorials of message sending in one of the [wallet topics](https://continuation-team.gitbook.io/tonsdk.net-docs/user-manual/contracts/wallet).
{% endhint %}

To create nft transfer request, you can use `NftItem.CreateTransferRequest` :

```csharp
// define the address of the nft collection
Address collection = new Address("/* nft collection address */");

// define index of the nft what will send
uint index = 10;

// get the nft items address using TonClient: TonSdk.Client
Address nftItemAddress = await tonclient.Nft.GetItemAddress(collection, index);

// create transfer options
NftTransferOptions options = new NftTransferOptions()
{
    NewOwner = new Address("/* receiver address */") // new nft owner address
};

// create a message body for the nft transfer
Cell nftTransfer = NftItem.CreateTransferRequest(options);

// create a transfer message for the wallet
ExternalInMessage message = wallet.CreateTransferMessage(new[]
{
    new WalletTransfer
    {
        Message = new InternalMessage(new()
        {
            Info = new IntMsgInfo(new()
            {
                Dest = nftItemAddress,
                Value = new Coins(0.1) // amount in TONs to send
            }),
            Body = nftTransfer
        }),
        Mode = 1
    }
}, seqno).Sign(mnemonic.Keys.PrivateKey);

// get message cell
Cell cell = message.Cell;

// send this message via TonClient,
// for example, await tonClient.SendBoc(cell);
```

{% hint style="info" %}
You can find more `NftTransferOptions` [here](https://github.com/continuation-team/TonSdk.NET/blob/main/TonSdk.Contracts/src/nft/NftItem.cs#L9C5-L16C6).
{% endhint %}
