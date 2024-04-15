---
description: TonSdk.Contracts.Jetton
---

# JettonWallet

`JettonWallet` is a class that contains methods to work with transferring and burning Jettons.

Methods return `Cell` in output and this cell can be setted like message body in `Wallet.CreateTransferMessage` .

{% hint style="info" %}
You can find tutorials of message sending in one of the [wallet topics](../wallet/).
{% endhint %}



To create jetton transfer request, you can use `JettonWallet.CreateTransferRequest` :

```csharp
// define the address of the jetton master contract and jetton wallet owner
Address jettonMasterContract = new Address("/* jetton contract address */");
Address address = new Address("/* jetton wallet owner */");

// get the jetton wallet address using TonClient: TonSdk.Client
Address jettonWallet = await tonclient.Jetton.GetWalletAddress(jettonMasterContract, address);

// create transfer options
JettonTransferOptions options = new JettonTransferOptions()
{
    Amount = new Coins(100), // jetton amount to send, for ex 100 jettons
    Destination = new Address("/* receiver wallet address */") // receiver
};

// create a message body for the jetton transfer
Cell jettonTransfer = JettonWallet.CreateTransferRequest(options);

// create a transfer message for the wallet
ExternalInMessage message = wallet.CreateTransferMessage(new[]
{
    new WalletTransfer
    {
        Message = new InternalMessage(new()
        {
            Info = new IntMsgInfo(new()
            {
                Dest = jettonWallet,
                Value = new Coins(0.1) // amount in TONs to send
            }),
            Body = jettonTransfer
        }),
        Mode = 1
    }
}, seqno).Sign(mnemonic.Keys.PrivateKey);

// get message cell
Cell cell = message.Cell;

// send this message via TonClient,
// for example, await tonClient.SendBoc(cell);
```



To create jetton burn request, you can use `JettonWallet.CreateBurnRequest` :&#x20;

```csharp
// define the address of the jetton master contract and jetton wallet owner
Address jettonMasterContract = new Address("/* jetton contract address */");
Address address = new Address("/* jetton wallet owner */");

// get the jetton wallet address using TonClient: TonSdk.Client
Address jettonWallet = await tonclient.Jetton.GetWalletAddress(jettonMasterContract, address);

// create burn options
JettonBurnOptions options = new JettonBurnOptions()
{
    Amount = new Coins(1000), // jetton amount to burn, for ex 1000 jettons
};

// create a message body for the jetton burn
Cell jettonBurn = JettonWallet.CreateBurnRequest(options);

// create a transfer message for the wallet
ExternalInMessage message = wallet.CreateTransferMessage(new[]
{
    new WalletTransfer
    {
        Message = new InternalMessage(new()
        {
            Info = new IntMsgInfo(new()
            {
                Dest = jettonWallet,
                Value = new Coins(0.1) // amount in TONs to send
            }),
            Body = jettonBurn
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
You can find more`JettonTransferOptions` and `JettonBurnOptions` [here](https://github.com/continuation-team/TonSdk.NET/blob/main/TonSdk.Contracts/src/jetton/JettonWallet.cs#L14C5-L29C6).
{% endhint %}
