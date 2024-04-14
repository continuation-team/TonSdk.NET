---
description: TonSdk.Core.Boc
---

# Cell

A cell represents a data structure on TON Blockchain. Cells are able to store up to 1023 bits and possess up to 4 references to other cells.



You can create new `Cell` instance using constructor:

<pre class="language-csharp"><code class="lang-csharp">string bitsArray = /* bits array string representation */;
// or
Bits bits = new Bits(/* byte array */);
<strong>Cell[] refs = /* array of cell refs */;
</strong><strong>
</strong><strong>
</strong>Cell cell = new Cell(bitsArray);
// or
<strong>Cell cell = new Cell(bits, refs);
</strong></code></pre>

also you can use static method `Cell.From` to get `Cell` instance:

```csharp
string bitsArray = /* bits array string representation */;
// or
Bits bits = new Bits(/* byte array */);

Cell cell = Cell.From(bitsArray);
// or
Cell cell = Cell.From(bits);
```



Any `Cell` instance you can parse into `CellSlice` using `Parse` method:

```csharp
string bitsArray = /* bits array string representation */;
Cell cell = Cell.From(bitsArray);
CellSlice slice = cell.Parse();
```

Its also possible to serialize any `Cell` instance using `Serialize` method:

```csharp
string bitsArray = /* bits array string representation */;
Cell cell = Cell.From(bitsArray);
Bits serializedCell = cell.Serialize();
```



Any `Cell` instance can be converted into string representation with different modes: `hex`, `fiftBin`, `fiftHex`, `base64` and `base64Url`:

```csharp
string bitsArray = /* bits array string representation */;
Cell cell = Cell.From(bitsArray);

string base64Cell = cell.toString(); // by default, toString() converts cell to base64 format

string hexCell = cell.toString("hex");
string fiftBinCell = cell.toString("fiftBin");
string fiftHexCell = cell.toString("fiftHex");
string base64Cell1 = cell.toString("base64");
string base64urlCell = cell.toString("base64url");
```

