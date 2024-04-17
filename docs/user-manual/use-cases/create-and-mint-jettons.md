# Create and Mint Jettons

In this topic you will know how to create new Jetton and mint it using `TonSDK.Net`

#### Preparing Metadata

You can create Jettons with `OffChain` or `OnChain` metadata using structs in `TonSdk.Contracts.Jetton`:

<pre class="language-csharp"><code class="lang-csharp"><strong>// offchain metadata 
</strong>JettonMinterOptions options = new JettonMinterOptions()
{
    AdminAddress = new Address(/* admin address */),
    JettonContent = new JettonOffChainContent()
    {
        ContentUri = "https://example.com/metadata.json" // your metadata link
    }
};
</code></pre>

```csharp
// onchain metadata 
JettonMinterOptions options = new JettonMinterOptions()
{
    AdminAddress = new Address(/* admin address */),
    JettonContent = new JettonOnChainContent()
    {
        Name = "My Best Token",
        Symbol = "MBT",
        Description = "This is the best description."
        Decimals = 9, // optional
        Image = "https://example.com/image.png" // optional
    }
};
```

#### Creating new Jetton Minter

To create new jetton, you can use `JettonMinter` class to create it with `JettonMinterOptions` presented above.

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

// creating options with onchain metadata 
JettonMinterOptions opts = new JettonMinterOptions()
{
    AdminAddress = wallet.Address,
    JettonContent = new JettonOnChainContent()
    {
        Name = "My Best Token",
        Symbol = "MBT",
        Description = "This is the best description."
        Decimals = 9, // optional
        Image = "https://example.com/image.png" // optional
    }
};

// creating jetton minter with options
JettonMinter minter = new JettonMinter(opts);

// getting seqno using tonClient
uint? seqno = await tonClient.Wallet.GetSeqno(wallet.Address);

// creating jetton minter deploy message
var msg = wallet.CreateTransferMessage(new[]
{
    new WalletTransfer
    {
        Message = new InternalMessage(new InternalMessageOptions
        {
            Info = new IntMsgInfo(new IntMsgInfoOptions
            {
                Dest = minter.Address,
                Value = new Coins(0.05)
            }),
            Body = null,
            StateInit = minter.StateInit
        }),
        Mode = 3 // message mode
    }
}, seqno ?? 0).Sign(m.Keys.PrivateKey);

// send this message via TonClient
await client.SendBoc(msg.Cell);

// print jetton master contract
Console.WriteLine(minter.Address);
```

#### Minting Jettons

After that we created our Jetton Minter and deployed it, we can mint more jettons using `CreateMintRequest` method:

```csharp
// we will use same wallet and minter like in prev code block

// create jetton mint options
JettonMintOptions mintOptions = new JettonMintOptions()
{
    JettonAmount = new Coins(10000000), // jetton amount to mint
    Amount = new Coins(0.05), // amount send to jetton wallet
    Destination = wallet.Address // address which will get jettons
};

// create jetton mint request body
Cell jettonMintBody = JettonMinter.CreateMintRequest(mintOptions);

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
                Dest = minter.Address,
                Value = new Coins(0.05)
            }),
            Body = jettonMintBody
        }),
        Mode = 3 // message mode
    }
}, seqno ?? 0).Sign(m.Keys.PrivateKey);

// send this message via TonClient
await client.SendBoc(msg.Cell);
```
