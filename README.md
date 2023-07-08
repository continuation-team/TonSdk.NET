## TonSdk.NET


## Features and status

| Feature                                   | Status                   |
|-------------------------------------------|--------------------------|
| Builder, Cell, Slice                      | <ul><li>- [x] </li></ul> |
| BOC  (de)serialization                    | <ul><li>- [x] </li></ul> |
| Hashmap(E) (dictionary) (de)serialization | <ul><li>- [x] </li></ul> |
| Mnemonic BIP39 standard                   | <ul><li>- [x] </li></ul> |
| Mnemonic TON standard                     | <ul><li>- [x] </li></ul> |
| Coins (class for TON, JETTON, e.t.c.)     | <ul><li>- [x] </li></ul> |
| Address (class for TON address)           | <ul><li>- [x] </li></ul> |
| Message layouts (such as MessageX e.t.c.) | <ul><li>- [x] </li></ul> |
| RPC client                                | <ul><li>- [x] </li></ul> |
| Popular structures from block.tlb         | <ul><li>- [x] </li></ul> |
| Contracts (abstract TON contract class)   | <ul><li>- [x] </li></ul> |
| Ed25519 signing of transactions           | <ul><li>- [x] </li></ul> |
| ~100% tests coverage                      | <ul><li>- [ ] </li></ul> |


## Overview example

```csharp
TonClient tonclient = new TonClient(new TonClientParameters { Endpoint = "https://toncenter.com/api/v2/jsonRPC", ApiKey = "xxx" });

Mnemonic mnemonic = new Mnemonic();

PreprocessedV2 wallet = new PreprocessedV2(new PreprocessedV2Options { PublicKey = mnemonic.Keys.PublicKey! });

Address address = wallet.Address;

string nonBounceableAddress = address.ToString(AddressType.Base64, new AddressStringifyOptions(false, false, true));

Console.WriteLine(nonBounceableAddress);

uint? seqno = await tonclient.Wallet.GetSeqno(address);

Coins walletBalance = await tonclient.GetBalance(address);

Address destination = await tonclient.Dns.GetWalletAddress("foundation.ton");

Console.WriteLine(walletBalance);
Console.WriteLine(destination);

ExternalInMessage message = wallet.CreateTransferMessage(new[]
{
    new WalletTransfer
    {
        Message = new InternalMessage(new()
        {
            Info = new IntMsgInfo(new()
            {
                Dest = destination,
                Value = new Coins("0.2")
            }),
            Body = new CellBuilder().StoreUInt(0, 32).StoreString("test").Build()
        }),
        Mode = 1
    }
}, seqno ?? 0).Sign(mnemonic.Keys.PrivateKey);

Console.WriteLine(message.Cell);

await tonclient.SendBoc(message.Cell!);
```

## License

LGPL License
