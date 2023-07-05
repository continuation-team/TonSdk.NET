using TonSdk.Core;
using TonSdk.Core.Block;
using TonSdk.Core.Boc;

namespace TonSdk.Contracts.Wallet;

public abstract class WalletBase {
    protected Cell _code;
    protected byte[] _publicKey;
    protected StateInit _stateInit;
    protected Address _address;

    public Cell Code {
        get => _code;
    }

    public byte[] PublicKey {
        get => _publicKey;
    }

    public StateInit StateInit {
        get => _stateInit;
    }

    public Address Address {
        get => _address;
    }

    protected abstract StateInit buildStateInit();
}

public class WalletTransfer {
    public MessageX Message;
    public byte Mode;
}

public static class WalletTraits {
    public static uint SUBWALLET_ID = 698983191;
    // why 698983191?? -> https://github.com/ton-blockchain/ton/blob/4b940f8bad9c2d3bf44f196f6995963c7cee9cc3/tonlib/tonlib/TonlibClient.cpp#L2420
}
