---
description: TonSdk.Core.Boc
---

# CellSlice

`CellSlice` represents slice - a special object which allows you to read from a cell.



`CellSlice` instance can be create via constructor with speceifing `Cell` or directly via `Cell` mutation with using method `Parse`:

```csharp
Cell cell; // source cell

CellSlice slice = new CellSlice(cell);
// or
CellSlice slice = cell.Parse();
```

{% hint style="info" %}
Its prefer to use `Parse` method when you want to parse cell into slice. It will make you code more readable and shorten.
{% endhint %}



Below you can see the methods what can be used**.**

* `SkipRefs(int size)` - skips specified refs count and return new slice
* `SkipRef` - skip one ref and return new slice
* `SkipOptRef` - firstly load one bit, if it `true` skip ref, return new slice
* `ReadRefs(int size)` - read specified refs count and return `Cell[]` without slice mutation
* `ReadRef`- read one ref and return `Cell` without slice mutation
* `ReadOptRef` - firstly read one bit, if it `true` read ref and return it, otherwise return null, without slice mutation
* `LoadRefs(int size)` - load specified refs count and return `Cell[]`
* `LoadRef` -  read one ref and return `Cell`
* `LoadOptRef` - firstly load one bit, if it `true` load ref and return it, otherwise return null

```csharp
Cell cell;
CellSlice slice = cell.Parse();

slice.SkipRef();
slice.SkipOptRef();

Cell[] refs1 = slice.ReadRefs(3); // slice won`t be mutated
Cell[] refs2 = slice.LoadRefs(3); // slice will be mutated

Cell ref1 = slice.ReadRef(); // slice won`t be mutated
Cell ref2 = slice.LoadRef(); // slice will be mutated
```



* `ReadDict(HasmapOptions opt)` - read and return `HashmapE` with specified `HashmapOptions`, without slice mutation
* `LoadDict(HasmapOptions opt)` - laod and return `HashmapE` with specified `HashmapOptions`

```csharp
// example hashmap options
HashmapOptions<uint, CellSlice> hmOptions = new HashmapOptions<uint, CellSlice>()
{
    KeySize = 32,
    Serializers = new HashmapSerializers<uint, CellSlice>
    {
        Key = k => new BitsBuilder(32).StoreUInt(k, 32).Build(),
        Value = v => new CellBuilder().Build()
    },
    Deserializers = new HashmapDeserializers<uint, CellSlice>
    {
        Key = k => (uint)k.Parse().LoadUInt(32),
        Value = v => v.Parse()
    }
};
                
HashmapE<uint, CellSlice> hashmap1 = cell.Parse().ReadDict(hmOptions); // slice won`t be mutated
HashmapE<uint, CellSlice> hashmap2 = cell.Parse().LoadDict(hmOptions); // slice will be mutated
```



* `SkipBits(int size)` - skips specified bits count and return new slice
* `SkipBit` - skip one bit and return new slice
* `ReadBits(int size)` - read specified bits count and return `Bits` without slice mutation
* `ReadBit`- read one bit and return `bool` without slice mutation
* `LoadBits(int size)` - load specified bits count and return `Bits`&#x20;
* `LoadBit` -  read one bit and return `bool`

```csharp
Cell cell;
CellSlice slice = cell.Parse();

slice.SkipBit();

Bits bits1 = slice.ReadBits(8); // slice won`t be mutated
Bits bits2 = slice.LoadBits(8); // slice will be mutated

bool bit = slice.ReadBit(); // slice won`t be mutated
bool bit = slice.LoadBit(); // slice will be mutated
```



* `ReadUint(int size)` - read uint with spicified size and return `BigInteger` without slice mutation
* `LoadUint(int size)` - load uint with spicified size and return `BigInteger`&#x20;
* `ReadInt(int size)` - read int with spicified size and return `BigInteger` without slice mutation
* `LoadInt(int size)` - load int with spicified size and return `BigInteger`&#x20;
* `ReadUInt32LE` - read uint in Little-Endian order and return `BigInteger` without slice mutation
* `LoadUInt32LE` - load uint in Little-Endian order and return `BigInteger`&#x20;
* `ReadUInt64LE` - read ulong in Little-Endian order and return `BigInteger` without slice mutation
* `LoadUInt64LE` - load ulong in Little-Endian order and return `BigInteger`&#x20;

```csharp
Cell cell;
CellSlice slice = cell.Parse();

uint a = (uint)slice.ReadUint(32); // slice won`t be mutated
long b = (long)slice.LoadInt(64); // slice will be mutated
```



* `ReadCoins(int decimals = 9)` - read and return `Coins` with specified decimals count, without slice mutation
* `LoadCoins(int decimals = 9)` - load and return `Coins` with specified decimals count

<pre class="language-csharp"><code class="lang-csharp">Cell cell;
<strong>CellSlice slice = cell.Parse();
</strong>
Coins coins1 = slice.ReadCoins(); // slice won`t be mutated
Coins coins2 = slice.LoadCoins(16); // slice will be mutated
</code></pre>



* `ReadVarUInt(int length)` - read `VarUInt` with spicified length and return `BigInteger` without slice mutation
* `LoadVarUInt(int length)` - load`VarUInt` with spicified length and return `BigInteger`&#x20;
* `ReadVarInt(int length)` - read `VarInt` with spicified length and return `BigInteger` without slice mutation
* `LoadVarInt(int length)` - load`VarInt` with spicified length and return `BigInteger`&#x20;

```csharp
Cell cell;
CellSlice slice = cell.Parse();

BigInteger a = slice.ReadVarUInt(16); // slice won`t be mutated
BigInteger b = slice.LoadVarInt(16); // slice will be mutated
```



* `ReadAddress` - read and return `Address`, without slice mutation
* `LoadAddress` - load and return `Address`

<pre class="language-csharp"><code class="lang-csharp">Cell cell;
<strong>CellSlice slice = cell.Parse();
</strong>
Address addr1 = slice.ReadAddress(); // slice won`t be mutated
Address addr2 = slice.LoadAddress(); // slice will be mutated
</code></pre>



* `ReadBytes(int size)` - read and return `byte[]`, without slice mutation
* `LoadBytes(int size)` - load and return `byte[]`

<pre class="language-csharp"><code class="lang-csharp">Cell cell;
<strong>CellSlice slice = cell.Parse();
</strong>
byte[] arr1 = slice.ReadBytes(32); // slice won`t be mutated
byte[] arr2 = slice.LoadBytes(16); // slice will be mutated
</code></pre>



* `ReadString` - read all remainder bytes and return as `string`, without slice mutation
* `ReadString(int size)` - read `size` bytes and return as `string`, without slice mutation
* `LoadString` - load all remainder bytes and return as `string`
* `LoadString(int size)` - load `size` bytes and return as `string`, without slice mutation

<pre class="language-csharp"><code class="lang-csharp">Cell cell;
<strong>CellSlice slice = cell.Parse();
</strong>
string str1 = slice.ReadString(); // slice won`t be mutated
string str2 = slice.ReadString(32); // slice won`t be mutated
string str3 = slice.LoadString(); // slice will be mutated
string str4 = slice.LoadString(16); // slice will be mutated
</code></pre>



You can restore remainder data as `Cell` using `RestoreRemainder`:

```csharp
Cell cell;
CellSlice slice = cell.Parse();

slice.SkipBit();
slice.SkipBit();
Coins coins = slice.LoadCoins(16);
uint count = (uint)slice.LoadUint(32);

Cell remainderCell = slice.RestoreRemainder(); // return remainder data as cell
```

also you can restore source cell using `Restore`:

```csharp
Cell cell;
CellSlice slice = cell.Parse();

slice.SkipBit();
slice.LoadBytes(32);
int count = (int)slice.LoadInt(32);

Cell srcCell = slice.Restore(); // return source cell
```



Its also possible to clone current slice with using `Clone`:

```csharp
Cell cell;
CellSlice slice = cell.Parse();
CellSlice sliceCopy = cell.Clone(); // will return new slice with same data
```
