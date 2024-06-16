using TonSdk.Core;
using TonSdk.Core.Boc;

namespace TonSdk.DeFi.DeDust
{
    public class DeDustAsset
    {
        private readonly DeDustAssetType _type;
        private readonly Address? _address;

        private DeDustAsset(DeDustAssetType type, Address? address = null)
        {
            _type = type;
            _address = address;
        }

        public static DeDustAsset Native() =>
            new DeDustAsset(DeDustAssetType.Native);
    
        public static DeDustAsset Jetton(Address minter) =>
            new DeDustAsset(DeDustAssetType.Jetton, minter);

        public Cell ToCell()
        {
            var builder = new CellBuilder();
            switch (_type)
            {
                case DeDustAssetType.Native:
                    builder
                        .StoreUInt((uint)DeDustAssetType.Native, 4);
                    break;
                case DeDustAssetType.Jetton:
                    builder
                        .StoreUInt((uint)DeDustAssetType.Jetton, 4)
                        .StoreInt(_address.GetWorkchain(), 8)
                        .StoreBytes(_address.GetHash());
                    break;
            }

            return builder.Build();
        }

    }
}