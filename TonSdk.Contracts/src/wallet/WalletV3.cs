using System;
using TonSdk.Core;
using TonSdk.Core.Block;
using TonSdk.Core.Boc;

namespace TonSdk.Contracts.Wallet {
    public struct WalletV3Options {
        public byte[] PublicKey;
        public int? Workchain;
        public uint? SubwalletId;
    }

    public struct WalletV3Storage {
        public uint Seqno;
        public uint SubwalletId;
        public byte[] PublicKey;
    }
    
    public class WalletV3 : WalletBase {
        private uint _subwalletId;

        public uint SubwalletId => _subwalletId;

        public WalletV3(WalletV3Options opt, uint revision = 2) {
            if (revision != 1 && revision != 2) {
                throw new Exception("Unsupported revision. Only 1 and 2 are supported");
            }

            _code = revision == 1 ? Cell.From(WalletSources.V3R1) : Cell.From(WalletSources.V3R2);
            _publicKey = opt.PublicKey;
            _subwalletId = opt.SubwalletId ?? WalletTraits.SUBWALLET_ID;
            _stateInit = buildStateInit();
            _address = new Address(opt.Workchain ?? 0, _stateInit);
        }

        public WalletV3Storage ParseStorage(CellSlice slice) {
            BlockUtils.CheckUnderflow(slice, 32 + 32 + 256, null);
            return new WalletV3Storage {
                Seqno = (uint)slice.LoadUInt(32),
                SubwalletId = (uint)slice.LoadUInt(32),
                PublicKey = slice.LoadBytes(32)
            };
        }

        protected sealed override StateInit buildStateInit() {
            var data = new CellBuilder()
                .StoreUInt(0, 32)
                .StoreUInt(_subwalletId, 32)
                .StoreBytes(_publicKey)
                .Build();
            return new StateInit(new StateInitOptions { Code = _code, Data = data });
        }

        public ExternalInMessage CreateTransferMessage(WalletTransfer[] transfers, uint seqno, uint timeout = 60) {
            if (transfers.Length == 0 || transfers.Length > 4) {
                throw new Exception("WalletV3: can make only 1 to 4 transfers per operation.");
            }

            var bodyBuilder = new CellBuilder()
                .StoreUInt(_subwalletId, 32)
                .StoreUInt(DateTimeOffset.Now.ToUnixTimeSeconds() + timeout, 32)
                .StoreUInt(seqno, 32);

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
                .StoreInt(-1L, 32)
                .StoreUInt(0, 32); // seqno = 0

            return new ExternalInMessage(new ExternalInMessageOptions {
                Info = new ExtInMsgInfo(new ExtInMsgInfoOptions { Dest = Address }),
                Body = bodyBuilder.Build(),
                StateInit = _stateInit
            });
        }
    }
}
