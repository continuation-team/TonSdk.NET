---
description: TonSdk.Core.Boc
---

# CellBuilder

`CellBuilder` - a special class which allows you to write data and build a cell.



`CellBuilder` instance can be create via constructor with optional `int length` parameter:

```csharp
CellBuilder builder1 = new CellBuilder(); // by default, length = 1023
CellBuilder builder2 = new CellBuilder(512);
```



You can build `CellBuilder` instance into `Cell` using `Build` method:

<pre class="language-csharp"><code class="lang-csharp">CellBuilder builder = new CellBuilder();

// example data
builder.StoreUInt(0, 32);
builder.StoreUInt(_subwalletId, 32);
builder.StoreBytes(_publicKey);

// building into cell
<strong>Cell cell = builder.Build();
</strong></code></pre>

To make code more readable and shorten, its prefer to use in one statement:

```csharp
Cell cell = new CellBuilder()
    .StoreUInt(0, 32)
    .StoreUInt(_subwalletId, 32)
    .StoreBytes(_publicKey)
    .Build();
```

{% hint style="info" %}
In docs we will use larger code for education purposes, but its prefer to use shorten version of code, implemented in previous code block.&#x20;
{% endhint %}



Below you can see the methods what can be used**.**

* `StoreRefs(Cell[] refs)` - get array of `Cell[]` and store them in builder
* `StoreRef(Cell ref)` - get one ref as `Cell` and store it in builder
* `StoreOptRef(Cell ref)` - get one ref as `Cell` and store it in builder, also stores one bit (will be true, if ref not null, otherwise false)

<pre class="language-csharp"><code class="lang-csharp"><strong>Cell[] refs = /* your refs as Cell[] */;
</strong><strong>
</strong><strong>CellBuilder builder = new CellBuilder();
</strong><strong>builder.StoreOptRef(refs[0]);
</strong><strong>builder.StoreRefs(refs);
</strong></code></pre>



* `StoreCellSlice(CellSlice slice)` - get `CellSlice` and store it in builder
* `StoreBitsSlice(BitsSlice slice)` - get `BitsSlice` and store it in builder
* `StoreDict(HashmapE<K, V> hashmap)` - get `HashmapE<K,V>` and store it in builder

```csharp
CellSlice slice = /* your CellSlice */;
HashmapE<K, V> hashmap = /* your HashmapE<K, V> */;

CellBuilder builder = new CellBuilder();
builder.StoreCellSlice(slice);
builder.StoreDict(hashmap);
builder.StoreCellSlice(hashmap.Build().Parse()); // you also can store hashmap as slice
```



* `StoreBits(Bits bits)` - get `Bits` and store it in builder
* `StoreBits(BitsArray bitsArray)` - get `BitsArray` and store it in builder
* `StoreBits(string bitsArray)` - get string representation of`BitsArray` and store it in builder
* `StoreBit(bool bit)` - get `bool` as bit value and store it in builder

```csharp
Bits bits = new Bits(); // your Bits instance

CellBuilder builder = new CellBuilder();
builder.StoreBits(bits);
builder.StoreBit(false);
```



* `StoreBytes(byte[] value)` - get `byte[]` and store it in builder
* `StoreByte(byte value)` - get one`byte` and store it in builder
* `StoreString(string value)` - get `string` , convert it to bytes and store in builder

```csharp
byte[] bytes = /* your byte[] data */
string greeting = "Hello TON!";

CellBuilder builder = new CellBuilder();
builder.StoreBytes(bytes);
builder.StoreByte(bytes[0]);
builder.StoreString(greeting);
```



`StoreUInt(ulong value, int size)` - get `ulong` value and `int` size, and store value in builder

`StoreUInt(BigInteger value, int size)` - get `BigInteger` value and `int` size, and store value in builder

`StoreInt(long value, int size)` - get `long` value and `int` size, and store value in builder

`StoreInt(BigInteger value, int size)` - get `BigInteger` value and `int` size, and store value in builder

`StoreUInt32LE(uint value)` - get `uint` value, and store it in builder in Little-Endian order

`StoreUInt64LE(ulong value)` - get `ulong` value, and store it in builder in Little-Endian order

```csharp
CellBuilder builder = new CellBuilder();

builder.StoreUInt(0, 32);
builder.StoreInt(long.MaxValue, 64);
builder.StoreUInt64LE(ulong.MaxValue);
```



`StoreAddress(Address? address)` - get `Address`  and store it in builder

`StoreCoins(Coins coins)` - get `Coins` and store it in builder

`StoreVarUInt(BigInteger value, int length)` - get `BigInteger` value of VarUInt  and `int` length, and store value in builder

`StoreVarInt(BigInteger value, int length)` - get `BigInteger` value of VarInt and `int` length, and store value in builder

<pre class="language-csharp"><code class="lang-csharp"><strong>Address address = new Address(/* any address */);
</strong><strong>Coins coins = new Coins(10);
</strong><strong>CellBuilder builder = new CellBuilder();
</strong>
builder.StoreAddress(address);
builder.StoreCoins(coins);
builder.StoreVarUInt(coins.ToBigInt(), 16);
</code></pre>
