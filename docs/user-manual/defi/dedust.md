# DeDust

{% hint style="info" %}
For example, we will use `WalletV4`. You can find more examples for different contracts [here](https://docs.tonsdk.net/user-manual/contracts/wallet).
{% endhint %}

### Getting Started

Initialize the SDK in your code using:

```csharp
Mnemonic mnemonic = new Mnemonic();
WalletV4 wallet = new WalletV4(new WalletV4Options()
{
    PublicKey = mnemonic.Keys.PublicKey
});

TonClient tonClient = new TonClient(TonClientType.HTTP_TONCENTERAPIV2, new HttpParameters() {});
DeDustFactory factory = DeDustFactory.CreateFromAddress(DeDustConstants.MainNetFactory);

Address jettonAddress = new Address(/* here address of jetton master */); 
```



### Swapping TON to Jetton

```csharp
// define asset instances
DeDustAsset nativeAsset = DeDustAsset.Native();
DeDustAsset jettonAsset = DeDustAsset.Jetton(jettonAddress);

// get actual pool of swap pair
DeDustPool pool = await factory.GetPool(tonClient, DeDustPoolType.Volatile, new[] { nativeAsset, jettonAsset });

// get native TON vault
DeDustNativeVault tonVault = await factory.GetNativeVault(tonClient);

// check if vault of TON exists
if (await tonVault.GetReadinessStatus(tonClient) != DeDustReadinessStatus.Ready)
{
    Console.WriteLine("[Ton To Jetton] Vault (TON) does not exist.");
    return;
}

// check if pool of swap pair exists
if (await pool.GetReadinessStatus(tonClient) != DeDustReadinessStatus.Ready)
{
    Console.WriteLine("[Ton To Jetton] Pool (TON, Jetton) does not exist.");
    return;
}

 // amount of TONs to swap
double amount = 5;

// create swap body cell
Cell swapToJettonBody = DeDustNativeVault.CreateSwapBody(new DeDustNativeSwapOptions()
{
    PoolAddress = pool.Address,
    Amount = new Coins(amount)
});

// getting seqno using tonClient
uint? seqno = await tonClient.Wallet.GetSeqno(walletV4.Address);

// create transfer message to vault contract
var msg = wallet.CreateTransferMessage(new WalletTransfer[]
{
    new WalletTransfer
    {
        Message = new InternalMessage(new InternalMessageOptions
        {
            Info = new IntMsgInfo(new IntMsgInfoOptions
            {
                Dest = tonVault.Address,
                Value = new Coins(amount).Add(new Coins(0.25)) // gas amount, dont change
            }),
            Body = swapToJettonBody
        }),
        Mode = 1
    }
}, seqno.Value).Sign(mnemonic.Keys.PrivateKey);

// send signed message
await tonClient.SendBoc(msg.Cell);
```



### Swapping Jetton to TON

```csharp
// get jetton wallet address 
Address jettonWallet = await tonClient.Jetton.GetWalletAddress(jettonAddress, wallet.Address);

// get jetton vault
DeDustJettonVault jettonVault = await factory.GetJettonVault(tonClient, jettonAddress);

// define asset instances
DeDustAsset nativeAsset = DeDustAsset.Native();
DeDustAsset jettonAsset = DeDustAsset.Jetton(jettonAddress);

// get actual pool of swap pair
DeDustPool pool = await factory.GetPool(tonClient, DeDustPoolType.Volatile, new[] { nativeAsset, jettonAsset });

// check if vault of Jetton exists
if (await jettonVault.GetReadinessStatus(Core.TonSecondClient) != DeDustReadinessStatus.Ready)
{
    Console.WriteLine("[Jetton To Ton] Vault (TON) does not exist.");
    return;
}

// check if pool of swap pair exists
if (await pool.GetReadinessStatus(Core.TonSecondClient) != DeDustReadinessStatus.Ready)
{
    Console.WriteLine("[Jetton To Ton] Pool (TON, Jetton) does not exist.");
    return;
}

// jetton amount to swap
double amount = 5;

// create jetton transfer options
JettonTransferOptions options = new JettonTransferOptions()
{
    Amount = new Coins(amount),
    Destination = jettonVault.Address,
    ResponseDestination = wallet.Address,
    ForwardAmount = new Coins(0.25), // gas, dont change
    ForwardPayload = DeDustJettonVault.CreateSwapPayload(new DeDustJettonSwapOptions()
    {
        PoolAddress = pool.Address
    })
};

// create jetton tranfer cell
Cell jettonTransfer = JettonWallet.CreateTransferRequest(options);

// getting seqno using tonClient
uint? seqno = await tonClient.Wallet.GetSeqno(walletV4.Address);

// create transfer message to jetton wallet with swap body
var msg = taskWallet.CreateTransferMessage(new WalletTransfer[]
{
    new WalletTransfer
    {
        Message = new InternalMessage(new InternalMessageOptions
        {
            Info = new IntMsgInfo(new IntMsgInfoOptions
            {
                Dest = jettonWallet,
                Value = new Coins(0.3)
            }),
            Body = jettonTransfer
        }),
        Mode = 1
    }
}, seqno.Value).Sign(mnemonic.Keys.PrivateKey);

// send signed message
await tonClient.SendBoc(msg.Cell);
```
