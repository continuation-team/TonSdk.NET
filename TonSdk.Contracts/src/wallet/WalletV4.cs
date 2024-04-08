using System;
using TonSdk.Core;
using TonSdk.Core.Block;
using TonSdk.Core.Boc;

namespace TonSdk.Contracts.Wallet {
    public struct WalletV4Options {
        public byte[] PublicKey;
        public int? Workchain;
        public uint? SubwalletId;
    }

    public struct WalletV4Storage {
        public uint Seqno;
        public uint SubwalletId;
        public byte[] PublicKey;
        public HashmapE<Address, bool> Plugins;
    }


    public class WalletV4 : WalletBase {
        private uint _subwalletId;

        public uint SubwalletId => _subwalletId;

        public WalletV4(WalletV4Options opt, uint revision = 2) {
            if (revision != 1 && revision != 2) throw new Exception("Unsupported revision. Only 1 and 2 are supported");
            _code = revision == 1 ? Cell.From(WalletSources.V4R1) : Cell.From(WalletSources.V4R2);
            _publicKey = opt.PublicKey;
            _subwalletId = opt.SubwalletId ?? WalletTraits.SUBWALLET_ID;
            _stateInit = buildStateInit();
            _address = new Address(opt.Workchain ?? 0, _stateInit);
        }

        public WalletV4Storage ParseStorage(CellSlice slice) {
            BlockUtils.CheckUnderflow(slice, 32 + 32 + 256 + 1, null);
            return new WalletV4Storage {
                Seqno = (uint)slice.LoadUInt(32),
                SubwalletId = (uint)slice.LoadUInt(32),
                PublicKey = slice.LoadBytes(32),
                Plugins = slice.LoadDict(new HashmapOptions<Address, bool> {
                    KeySize = 8 + 256,
                    Serializers = new HashmapSerializers<Address, bool> {
                        Key = k => new BitsBuilder(8 + 256)
                            .StoreInt(k.GetWorkchain(), 8)
                            .StoreBytes(k.GetHash())
                            .Build(),
                        Value = v => new CellBuilder().Build()
                    },
                    Deserializers = new HashmapDeserializers<Address, bool> {
                        Key = kb => {
                            var ks = kb.Parse();
                            return new Address((int)ks.LoadInt(8), ks.LoadBytes(32));
                        },
                        Value = _ => true
                    }
                })
            };
        }

        protected sealed override StateInit buildStateInit() {
            var data = new CellBuilder()
                .StoreUInt(0, 32)
                .StoreUInt(_subwalletId, 32)
                .StoreBytes(_publicKey)
                .StoreBit(false)
                .Build();
            return new StateInit(new StateInitOptions { Code = _code, Data = data });
        }

        public ExternalInMessage CreateTransferMessage(WalletTransfer[] transfers, uint seqno, uint timeout = 60) {
            if (transfers.Length == 0 || transfers.Length > 4) {
                throw new Exception("WalletV4: can make only 1 to 4 transfers per operation.");
            }

            var bodyBuilder = new CellBuilder()
                .StoreUInt(_subwalletId, 32)
                .StoreUInt(DateTimeOffset.Now.ToUnixTimeSeconds() + timeout, 32)
                .StoreUInt(seqno, 32)
                .StoreUInt(0, 8); // op::simple_send

            foreach (var transfer in transfers) {
                bodyBuilder
                    .StoreUInt(transfer.Mode, 8)
                    .StoreRef(transfer.Message.Cell);
            }

            return new ExternalInMessage(new ExternalInMessageOptions {
                Info = new ExtInMsgInfo(new ExtInMsgInfoOptions { Dest = Address }),
                Body = bodyBuilder.Build(),
                StateInit = seqno == 0 ? _stateInit : null
            });
        }

        public ExternalInMessage CreateDeployMessage() {
            var bodyBuilder = new CellBuilder()
                .StoreUInt(_subwalletId, 32)
                .StoreInt(-1, 32)
                .StoreUInt(0, 32) // seqno = 0
                .StoreUInt(0, 8); // op::simple_send

            return new ExternalInMessage(new ExternalInMessageOptions {
                Info = new ExtInMsgInfo(new ExtInMsgInfoOptions { Dest = Address }),
                Body = bodyBuilder.Build(),
                StateInit = _stateInit
            });
        }
    }
}
