---
description: TonSdk.Core.Crypto
---

# KeyPair

`KeyPair` is a class, that is used to work with keypairs (private and public keys).

`KeyPair` can be created via using `Mnemonic` class, for example:

<pre class="language-csharp"><code class="lang-csharp"><strong>// with creating new mnemonic instance
</strong><strong>Mnemonic mnemonic = new Mnemonic();
</strong>KeyPair keys = mnemonic.Keys;

// or directly
byte[] seed = /* 32 bytes of seed */;
KeyPair keys1 = Mnemonic.GenerateKeyPair(seed);
</code></pre>



Each `KeyPair` class contains `PrivateKey` and `PublicKey` fields:

```csharp
Mnemonic mnemonic = new Mnemonic();
KeyPair keys = mnemonic.Keys;

byte[] privateKey = keys.PrivateKey;
byte[] publicKey = keys.PublicKey;
```

{% hint style="info" %}
Each key is a 32 byte array.
{% endhint %}



With using `KeyPair.Sign()` you can sign any data with specified `PrivateKey`:

```csharp
Mnemonic mnemonic = new Mnemonic();
KeyPair keys = mnemonic.Keys;
Cell data = /* your data in cell instance */

byte[] signedBytes = KeyPair.Sign(data, keys.PrivateKey)
```
