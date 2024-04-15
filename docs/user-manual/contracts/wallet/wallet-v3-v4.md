---
description: TonSdk.Contracts.Wallet
---

# Wallet V3, V4

`WalletV3` and `WalletV4` is class to work with Wallet v3 and Wallet v4, what includes `1` and `2` revisions.



To create `WalletV3` or `WalletV4` instance you can use class constructor:

```csharp
// create new mnemonic or use existing
Mnemonic mnemonic = new Mnemonic();

// create wallet v3 options
WalletV3Options optionsV3 = new WalletV3Options()
{
    PublicKey = mnemonic.Keys.PublicKey,
    // Workchain = 0,
    // SubwalletId = 0,
};

WalletV3 walletV3R1 = new WalletV3(optionsV3, 1);
WalletV3 walletV3R2 = new WalletV3(optionsV3, 2); 

// create wallet v4 options
WalletV4Options optionsV4 = new WalletV4Options()
{
    PublicKey = mnemonic.Keys.PublicKey
};

WalletV4 walletV4R1 = new WalletV4(optionsV4, 1); 
WalletV4 walletV4R2 = new WalletV4(optionsV4, 2); 
```



You can create deploy message using `CreateDeployMessage` method:

<pre class="language-csharp"><code class="lang-csharp"><strong>WalletV3 walletV3 = new WalletV3(options); // by default, wallet has 2 revision
</strong><strong>
</strong><strong>// create deploy message
</strong><strong>ExternalInMessage message = wallet.CreateDeployMessage();
</strong><strong>
</strong><strong>// sign deploy message
</strong><strong>message.Sign(mnemonic.Keys.PrivateKey);
</strong><strong>
</strong><strong>// get message cell
</strong><strong>Cell cell = message.Cell;
</strong><strong>
</strong><strong>// send this message via TonClient,
</strong><strong>// for example, await tonClient.SendBoc(cell);
</strong></code></pre>



Also you can create deploy message using `CreateTransferMessage` method:

```csharp
Address destination = new Address("/* destination address */");
Coins amount = new Coins(1); // 1 TON
string comment = "Hello TON!";

WalletV4 walletV4 = new WalletV4(options);

// create transaction body query + comment
Cell body = new CellBuilder().StoreUInt(0, 32).StoreString(comment).Build();

// getting seqno using tonClient
uint? seqno = await tonClient.Wallet.GetSeqno(walletV4.Address);

// create transfer message
ExternalInMessage message = walletV4.CreateTransferMessage(new[]
{
    new WalletTransfer
    {
        Message = new InternalMessage(new InternalMessageOptions
        {
            Info = new IntMsgInfo(new IntMsgInfoOptions
            {
                Dest = destination,
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

// send this message via TonClient,
// for example, await tonClient.SendBoc(cell);
```

{% hint style="info" %}
To get more info about message modes check this [page](https://docs.ton.org/develop/smart-contracts/messages#message-modes).&#x20;
{% endhint %}

{% hint style="info" %}
If you will pass seqno = 0, message will contain state init data.
{% endhint %}
