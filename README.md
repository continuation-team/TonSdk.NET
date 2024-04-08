## TonSdk.NET

You can ask questions that may arise during the use of the SDK in our [Telegram group](https://t.me/cont_team/104).

## Packages

### [TonSdk.Core](https://www.nuget.org/packages/TonSdk.Core/)
[![NuGet](https://img.shields.io/nuget/dt/TonSdk.Core.svg)](https://www.nuget.org/packages/TonSdk.Core)
[![NuGet](https://img.shields.io/nuget/vpre/TonSdk.Core.svg)](https://www.nuget.org/packages/TonSdk.Core) \
Core library with types and structures for TON Blockchain

### [TonSdk.Client](https://www.nuget.org/packages/TonSdk.Client/)
[![NuGet](https://img.shields.io/nuget/dt/TonSdk.Client.svg)](https://www.nuget.org/packages/TonSdk.Client)
[![NuGet](https://img.shields.io/nuget/vpre/TonSdk.Client.svg)](https://www.nuget.org/packages/TonSdk.Client) \
RPC Client for work with TonCenter API

### [TonSdk.Contracts](https://www.nuget.org/packages/TonSdk.Contracts/)
[![NuGet](https://img.shields.io/nuget/dt/TonSdk.Contracts.svg)](https://www.nuget.org/packages/TonSdk.Contracts)
[![NuGet](https://img.shields.io/nuget/vpre/TonSdk.Contracts.svg)](https://www.nuget.org/packages/TonSdk.Contracts) \
Abstractions for work with smart contracts in TON Blockchain

### [TonSdk.Connect](https://www.nuget.org/packages/TonSdk.Connect/)
[![NuGet](https://img.shields.io/nuget/dt/TonSdk.Connect.svg)](https://www.nuget.org/packages/TonSdk.Connect)
[![NuGet](https://img.shields.io/nuget/vpre/TonSdk.Connect.svg)](https://www.nuget.org/packages/TonSdk.Connect) \
Library to work with Ton Connect 2.0

### [TonSdk.Adnl](https://www.nuget.org/packages/TonSdk.Adnl/)
[![NuGet](https://img.shields.io/nuget/dt/TonSdk.Adnl.svg)](https://www.nuget.org/packages/TonSdk.Adnl)
[![NuGet](https://img.shields.io/nuget/vpre/TonSdk.Adnl.svg)](https://www.nuget.org/packages/TonSdk.Adnl) \
Library to work with Ton ADNL

## Features and status

- [x] Builder, Cell, Slice
- [x] BOC  (de)serialization
- [x] Hashmap(E) (dictionary) (de)serialization
- [x] Mnemonic BIP39 standard
- [x] Mnemonic TON standard
- [x] Coins (class for TON, JETTON, e.t.c.)
- [x] Address (class for TON address)
- [x] Message layouts (such as MessageX e.t.c.)
- [x] RPC client
- [x] Popular structures from block.tlb
- [x] Contracts (abstract TON contract class)
- [x] Ed25519 signing of transactions
- [ ] ~100% tests coverage

### Overview example

```csharp
// Create a new instance of the TonClient using the specified endpoint and API key for Http based client
TonClient client = new TonClient(TonClientType.HTTP_TONCENTERAPIV2, new HttpParameters { Endpoint = "https://toncenter.com/api/v2/jsonRPC", ApiKey = "xxx" });

// Create a new instance of the TonClient using the specified ip, port and key for LiteClient based client
TonClient clientLite = new TonClient(TonClientType.LITECLIENT, , new LiteClientParameters(ip, port, key));

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

// Pre-calculate fee before sending message
EstimateFeeResult fees = await _client.EstimateFee(message);

// Send the serialized message
await client.SendBoc(message.Cell!);
```

## Donation

`continuation.ton`

## License

MIT License
