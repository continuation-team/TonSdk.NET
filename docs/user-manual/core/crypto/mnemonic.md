---
description: TonSdk.Core.Crypto
---

# Mnemonic

Mnemonic is a class that is used to work with mnemonic phrases for accouns in Ton blockchain.

To create new mnemonic, use the following code:

```csharp
// this will generate new mnemonic
Mnemonic mnemonic1 = new Mnemonic();

// also you can pass the words directly to constructor
string[] words = /* your words */;
Mnemonic mnemonic2 = new Mnemonic(words);
```

{% hint style="warning" %}
Note, that mnemonic in Ton Blockchain must contain 24 bip39 words.
{% endhint %}



Each mnemonic contains `Words`, `Seed` and `Keys` fields.

```csharp
Mnemonic mnemonic = new Mnemonic();

string[] words = mnemonic.Words;
byte[] seed = mnemonic.Seed;
KeyPair keys = mnemonic.Keys;
```

For different situations you can direclty call methods and generate separate parts:

```csharp
string[] words = Mnemonic.GenerateWords();
byte[] seed = Mnemonic.GenerateSeed(words);
KeyPair keys = Mnemonic.GenerateKeyPair(seed);
```
