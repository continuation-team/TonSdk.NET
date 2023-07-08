## TonSdk.NET


## Packages

- **[TonSdk.Core](https://www.nuget.org/packages/TonSdk.Core/)** - Core library with types and structures for TON Blockchain
- **[TonSdk.Client](https://www.nuget.org/packages/TonSdk.Client/)** - RPC Client for work with TonCenter API
- **[TonSdk.Contracts](https://www.nuget.org/packages/TonSdk.Contracts/)** - Abstractions for work with smart contracts in TON Blockchain


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


### Overview example

```csharp
// Create a new instance of the TonClient using the specified endpoint and API key
TonClient tonclient = new TonClient(new TonClientParameters { Endpoint = "https://toncenter.com/api/v2/jsonRPC", ApiKey = "xxx" });

// Generate a new mnemonic phrase
Mnemonic mnemonic = new Mnemonic();

// Create a new preprocessed wallet using the public key from the generated mnemonic
PreprocessedV2 wallet = new PreprocessedV2(new PreprocessedV2Options { PublicKey = mnemonic.Keys.PublicKey! });

// Get the address associated with the wallet
Address address = wallet.Address;

// Convert the address to a non-bounceable format
string nonBounceableAddress = address.ToString(AddressType.Base64, new AddressStringifyOptions(false, false, true));

// Retrieve the wallet data
Cell? walletData = (await tonclient.GetAddressInformation(address)).Data;

// Extract the sequence number from the wallet data, or set it to 0 if the data is null
uint seqno = walletData == null ? 0 : wallet.ParseStorage(walletData.Parse()).Seqno;

// Get the balance of the wallet
Coins walletBalance = await tonclient.GetBalance(address);

// Get the destination address for the transfer from the Ton DNS
Address destination = await tonclient.Dns.GetWalletAddress("foundation.ton");

// Create a transfer message for the wallet
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
}, seqno).Sign(mnemonic.Keys.PrivateKey, true);

// Send the serialized message
await tonclient.SendBoc(message.Cell!);
```

### Overview example (Jetton Transfer)
```csharp
// Define the address of the jetton master contract
Address jettonMasterContract = new Address("EQBlHnYC0Uk13_WBK4PN-qjB2TiiXixYDTe7EjX17-IV-0eF");

// Get the jetton wallet address
Address jettonWallet = await tonclient.Jetton.GetWalletAddress(jettonMasterContract, address);

// Create a message body for the jetton transfer
Cell jettonTransfer = JettonWallet.CreateTransferRequest(new() { Amount = new Coins(100), Destination = destination });

// Create a transfer message for the wallet
ExternalInMessage message = wallet.CreateTransferMessage(new[]
{
    new WalletTransfer
    {
        Message = new InternalMessage(new()
        {
            Info = new IntMsgInfo(new()
            {
                Dest = jettonWallet,
                Value = new Coins("0.1")
            }),
            Body = jettonTransfer
        }),
        Mode = 1
    }
}, seqno).Sign(mnemonic.Keys.PrivateKey, true);

// Send the serialized message
await tonclient.SendBoc(message.Cell!);
```

## License

MIT License
