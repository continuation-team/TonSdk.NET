---
description: TonSdk.Contracts.Wallet
---

# Wallet V3

`WalletV3` is class to work with Wallet v3, what includes `1` and `2` revisions.



To create `WalletV3` instance you can use class constructor:

```csharp
// create new mnemonic or use existing
Mnemonic mnemonic = new Mnemonic();

// create wallet options
WalletV3Options options = new WalletV3Options()
{
    PublicKey = mnemonic.Keys.PublicKey,
    // Workchain = 0,
    // SubwalletId = 0,
};

WalletV3 walletV3R1 = new WalletV3(options, 1);
WalletV3 walletV3R2 = new WalletV3(options, 2); 
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
</strong><strong>// for example, await tonClient.SendBoc(message.Cell);
</strong></code></pre>
