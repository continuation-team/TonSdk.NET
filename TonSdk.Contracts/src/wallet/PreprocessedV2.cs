using System;
using TonSdk.Core;
using TonSdk.Core.Block;
using TonSdk.Core.Boc;

namespace TonSdk.Contracts.Wallet {
    public struct PreprocessedV2Options {
        public byte[] PublicKey;
        public int? Workchain;
    }

    public struct PreprocessedV2Storage {
        public byte[] PublicKey;
        public uint Seqno;
    }


    public class PreprocessedV2 : WalletBase {

        public PreprocessedV2(PreprocessedV2Options opt) {
            _code = Cell.From(WalletSources.PreprocessedV2);
            _publicKey = opt.PublicKey;
            _stateInit = buildStateInit();
            _address = new Address(opt.Workchain ?? 0, _stateInit);
        }

        public PreprocessedV2Storage ParseStorage(CellSlice slice) {
            BlockUtils.CheckUnderflow(slice, 16 + 256, null);
            return new PreprocessedV2Storage {
                PublicKey = slice.LoadBytes(32),
                Seqno = (uint)slice.LoadUInt(16),
            };
        }

        protected sealed override StateInit buildStateInit() {
            var data = new CellBuilder()
                .StoreBytes(_publicKey)
                .StoreUInt(0, 16)
                .Build();
            return new StateInit(new StateInitOptions { Code = _code, Data = data });
        }

        public ExternalInMessage CreateTransferMessage(WalletTransfer[] transfers, uint seqno, uint timeout = 60) {
            if (transfers.Length == 0 || transfers.Length > 255) {
                throw new Exception("PreprocessedV2: can make only 1 to 255 transfers per operation.");
            }

            var bodyBuilder = new CellBuilder()
                .StoreUInt(DateTimeOffset.Now.ToUnixTimeSeconds() + timeout, 64)
                .StoreUInt(seqno, 16);

            var actions = new OutAction[transfers.Length];

            for (var i = 0; i < transfers.Length; i++) {
                var transfer = transfers[i];
                var action = new ActionSendMsg(new ActionSendMsgOptions {
                    Mode = transfer.Mode,
                    OutMsg = transfer.Message
                });
                actions[i] = action;
            }

            bodyBuilder.StoreRef(new OutList(new OutListOptions { Actions = actions }).Cell);

            return new ExternalInMessage(new ExternalInMessageOptions {
                Info = new ExtInMsgInfo(new ExtInMsgInfoOptions { Dest = Address }),
                Body = bodyBuilder.Build(),
                StateInit = seqno == 0 ? _stateInit : null
            });
        }

        public ExternalInMessage CreateDeployMessage() {
            var bodyBuilder = new CellBuilder()
                .StoreInt(-1, 64)
                .StoreUInt(0, 16) // seqno = 0
                .StoreRef(new CellBuilder().Build()); // empty out_list

            return new ExternalInMessage(new ExternalInMessageOptions {
                Info = new ExtInMsgInfo(new ExtInMsgInfoOptions { Dest = Address }),
                Body = bodyBuilder.Build(),
                StateInit = _stateInit
            });
        }
    }
}
