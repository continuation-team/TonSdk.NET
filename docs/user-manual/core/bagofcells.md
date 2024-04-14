---
description: TonSdk.Core.Boc
---

# BagOfCells

`BagOfCells` - a special class which allows you to serialize and deserialize bag of cells.



To serialize bag of cells, you can use `BagOfCell.SerializeBoc` method:

```csharp
Cell[] cells = /* your cells */
byte[] data = BagOfCells.SerializeBoc(cells).ToBytes();
string base64 = Convert.ToBase64String(serializedData);
```



To deserialize bag of cells, you can use `BagOfCell.DeserializeBoc` method:

```csharp
string base64 = /* your base64 BOC */
Bits data = new Bits(Convert.FromBase64String(base64)); 
Cell[] cells = BagOfCells.DeserializeBoc(data);
```
