using System;
using TonSdk.Core;
using TonSdk.Core.Block;
using TonSdk.Core.Boc;

namespace TonSdk.Contracts.Wallet
{
    public struct HighloadV1Options
    {
        public byte[] PublicKey;
        public int? Workchain;
        public uint? SubwalletId;
    }

    public struct HighloadV1Storage
    {
        public uint Seqno;
        public uint? SubwalletId;
        public byte[] PublicKey;
    }


    public class HighloadV1 : WalletBase
    {
        private uint _subwalletId;

        public uint SubwalletId => _subwalletId;

        public HighloadV1(HighloadV1Options opt)
        {
            _code = Cell.From(WalletSources.HighloadV1);
            _subwalletId = opt.SubwalletId ?? WalletTraits.SUBWALLET_ID;
            _publicKey = opt.PublicKey;
            _stateInit = buildStateInit();
        }

        public HighloadV1Storage ParseStorage(CellSlice slice)
        {
            BlockUtils.CheckUnderflow(slice, 32 + 32 + 256, null);
            return new HighloadV1Storage
            {
                Seqno = (uint)slice.LoadUInt(32),
                SubwalletId = (uint)slice.LoadUInt(32),
                PublicKey = slice.LoadBytes(32)
            };
        }

        protected sealed override StateInit buildStateInit()
        {
            var data = new CellBuilder()
                .StoreUInt(0, 32) // seqno
                .StoreUInt(_subwalletId, 32)
                .StoreBytes(_publicKey)
                .Build();
            return new StateInit(new StateInitOptions { Code = _code, Data = data });
        }

        public ExternalInMessage CreateTransferMessage(WalletTransfer[] transfers, uint seqno, uint timeout = 60)
        {
            if (transfers.Length == 0 || transfers.Length > 255)
            {
                throw new Exception("HighloadV1: can make only 1 to 255 transfers per operation.");
            }

            var bodyBuilder = new CellBuilder()
                .StoreUInt(_subwalletId, 32)
                .StoreUInt(DateTimeOffset.Now.ToUnixTimeSeconds() + timeout, 32)
                .StoreUInt(seqno, 32);

            var actions = new OutAction[transfers.Length];

            for (var i = 0; i < transfers.Length; i++)
            {
                var transfer = transfers[i];
                var action = new ActionSendMsg(new ActionSendMsgOptions
                {
                    Mode = transfer.Mode,
                    OutMsg = transfer.Message
                });
                actions[i] = action;
            }

            bodyBuilder.StoreRef(new OutList(new OutListOptions { Actions = actions }).Cell);

            return new ExternalInMessage(new ExternalInMessageOptions
            {
                Info = new ExtInMsgInfo(new ExtInMsgInfoOptions { Dest = ToAddress() }),
                Body = bodyBuilder.Build(),
                StateInit = seqno == 0 ? _stateInit : null
            });
        }


        public ExternalInMessage CreateDeployMessage()
        {
            var bodyBuilder = new CellBuilder()
                .StoreUInt(_subwalletId, 32)
                .StoreInt(-1L, 32)
                .StoreUInt(0, 32) // seqno = 0
                .StoreBit(false); // empty dict

            return new ExternalInMessage(new ExternalInMessageOptions
            {
                Info = new ExtInMsgInfo(new ExtInMsgInfoOptions { Dest = ToAddress() }),
                Body = bodyBuilder.Build(),
                StateInit = _stateInit
            });
        }
    }
}