---
description: TonSdk.Contracts.Wallet
---

# Preprocessed V2

`PreprocessedV2`  is class to work with Preprocessed v2 wallet.

To create `PreprocessedV2` instance you can use class constructor:

```csharp
// create new mnemonic or use existing
Mnemonic mnemonic = new Mnemonic();

// create wallet options
PreprocessedV2Options options = new PreprocessedV2Options()
{
    PublicKey = mnemonic.Keys.PublicKey,
    Workchain = 0 // set workchain if needed
};

PreprocessedV2 wallet = new PreprocessedV2(options);
```

You can create deploy message using `CreateDeployMessage` method:

```csharp
PreprocessedV2 wallet = new PreprocessedV2(options); 

// create deploy message
ExternalInMessage message = wallet.CreateDeployMessage();

// sign deploy message
message.Sign(mnemonic.Keys.PrivateKey, true);

// get message cell
Cell cell = message.Cell;

// send this message via TonClient,
// for example, await tonClient.SendBoc(cell);
```

Also you can create deploy message using `CreateTransferMessage` method:

```csharp
Address destination = new Address("/* destination address */");
Coins amount = new Coins(1); // 1 TON
string comment = "Hello TON!";

PreprocessedV2 wallet = new PreprocessedV2(options); 

// create transaction body query + comment
Cell body = new CellBuilder().StoreUInt(0, 32).StoreString(comment).Build();

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
                Dest = destination,
                Value = amount,
            }),
            Body = body
        }),
        Mode = 1 // message mode
    }
}, seqno ?? 0); // if seqno is null we pass 0, wallet will auto deploy on message send

// sign transfer message
message.Sign(mnemonic.Keys.PrivateKey, true);

// get message cell
Cell cell = message.Cell;

// send this message via TonClient,
// for example, await tonClient.SendBoc(cell);
```

{% hint style="info" %}
To get more info about message modes check this [page](https://docs.ton.org/develop/smart-contracts/messages#message-modes).
{% endhint %}

{% hint style="info" %}
If you will pass seqno = 0, message will contain state init data.
{% endhint %}
