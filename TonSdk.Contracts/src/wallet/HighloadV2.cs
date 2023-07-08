using TonSdk.Core;
using TonSdk.Core.Block;
using TonSdk.Core.Boc;

namespace TonSdk.Contracts.Wallet;

public struct HighloadV2Options {
    public byte[] PublicKey;
    public int? Workchain;
    public uint? SubwalletId;
}

public struct HighloadV2Storage {
    public uint SubwalletId;
    public ulong LastCleaned;
    public byte[] PublicKey;
    public HashmapE<int, WalletTransfer> OldQueries;
}

public class HighloadV2 : WalletBase {
    private uint _subwalletId;

    public uint SubwalletId => _subwalletId;

    private HashmapOptions<int, WalletTransfer> _oldQueries_hmapOptions;

    public HighloadV2(HighloadV2Options opt) {
        _code = Cell.From(WalletSources.HighloadV2);
        _publicKey = opt.PublicKey;
        _subwalletId = opt.SubwalletId ?? WalletTraits.SUBWALLET_ID;
        _stateInit = buildStateInit();
        _address = new Address(opt.Workchain ?? 0, _stateInit);

        _oldQueries_hmapOptions = new() {
            KeySize = 16,
            Serializers = new() {
                Key = k => new BitsBuilder(16).StoreInt(k, 16).Build(),
                Value = v => new CellBuilder().StoreUInt(v.Mode, 8).StoreRef(v.Message.Cell).Build()
            },
            Deserializers = new() {
                Key = kb => (int)kb.Parse().LoadInt(16),
                Value = v => {
                    var vs = v.Parse();
                    return new WalletTransfer {
                        Mode = (byte)vs.LoadUInt(8),
                        Message = MessageX.Parse(vs)
                    };
                }
            }
        };
    }

    public HighloadV2Storage ParseStorage(CellSlice slice) {
        BlockUtils.CheckUnderflow(slice, 32 + 64 + 256 + 1, null);
        return new HighloadV2Storage {
            SubwalletId = (uint)slice.LoadUInt(32),
            LastCleaned = (ulong)slice.LoadUInt(64),
            PublicKey = slice.LoadBytes(32),
            OldQueries = slice.LoadDict(_oldQueries_hmapOptions)
        };
    }

    protected sealed override StateInit buildStateInit() {
        var data = new CellBuilder()
            .StoreUInt(_subwalletId, 32)
            .StoreUInt(0, 64) // last_cleaned
            .StoreBytes(_publicKey)
            .StoreBit(false) // old_queries
            .Build();
        return new StateInit(new StateInitOptions{ Code = _code, Data = data });
    }

    public ExternalInMessage CreateTransferMessage(
        WalletTransfer[] transfers, ulong? queryId = null, bool deploy = false
    ) {
        if (transfers.Length is 0 or > 255) {
            throw new Exception("HighloadV2: can make only 1 to 255 transfers per operation.");
        }

        var bodyBuilder = new CellBuilder()
            .StoreUInt(_subwalletId, 32)
            .StoreUInt(queryId ?? SmcUtils.GenerateQueryId(60), 64);

        var dict = new HashmapE<int, WalletTransfer>(_oldQueries_hmapOptions);

        for (int i = 0; i < transfers.Length; i++) {
            dict.Set(i, transfers[i]);
        }

        bodyBuilder.StoreDict(dict);

        return new ExternalInMessage(new ExternalInMessageOptions {
            Info = new ExtInMsgInfo(new ExtInMsgInfoOptions { Dest = Address }),
            Body = bodyBuilder.Build(),
            StateInit = deploy ? _stateInit : null
        });
    }

    public ExternalInMessage CreateDeployMessage() {
        var bodyBuilder = new CellBuilder()
            .StoreUInt(_subwalletId, 32)
            .StoreUInt(SmcUtils.GenerateQueryId(60), 64)
            .StoreBit(false);

        return new ExternalInMessage(new ExternalInMessageOptions {
            Info = new ExtInMsgInfo(new ExtInMsgInfoOptions { Dest = Address }),
            Body = bodyBuilder.Build(),
            StateInit = _stateInit
        });
    }
}
