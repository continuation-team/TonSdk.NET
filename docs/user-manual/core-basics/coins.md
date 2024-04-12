---
description: TonSdk.Core.Coins
---

# Coins

Coins is special class to work with financial assets in Ton Blockchain.

&#x20;To create Coins instance you can use class constructor and set different supported value types:

```csharp
Coins coins1 = new Coins(10);
Coins coins2 = new Coins(10.5);
Coins coins3 = new Coins(10.5f);
Coins coins4 = new Coins(10.5d);
Coins coins5 = new Coins("10.5");
Coins coins6 = new Coins("10,5");
```

You can create Coins instance from nano coins value with `CoinsOptions` in class Constructor or using `Coins.FromNano()`method:

```csharp
int decimals = 9; // coins decimals

Coins coinsFromNano = new Coins(20_555_000_000, new CoinsOptions(true, decimals));
Coins coinsFromNano2 = Coins.FromNano(20_555_000_000, decimals);

/* both instance values are equal */
```

To convert value to nano coins value, you can use `toNano()` method:

```csharp
string coinsNanoStr = new Coins("10.23").ToNano(); // "10230000000"
```



For now, TonSdk.Net supports arithmetical operations through coins methods, for example:

<pre class="language-csharp"><code class="lang-csharp">Coins addCoins = new Coins("10").Add(new Coins(10)); // 20
Coins subCoins = new Coins("10").Sub(new Coins(10)); // 0
<strong>Coins divCoins = new Coins("10").Div(2.5); // 4
</strong>Coins mulCoins = new Coins("10").Mul(2); // 20

// also you can use mutation variant, like
Coins firstCoins = new Coins(10);
firstCoins.Add(new Coins(10); // 20
</code></pre>

{% hint style="danger" %}
Remember, if you will try to do arithmetical operations using Coins with different decimal value, it will throw an Exception.
{% endhint %}

