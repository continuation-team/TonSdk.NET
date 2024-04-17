using System;
using TonSdk.Contracts.Jetton;
using TonSdk.Core;
using TonSdk.Core.Block;
using TonSdk.Core.Boc;

namespace TonSdk.Contracts.nft
{
    public struct NftCollectionOptions : ContractBaseOptions
    {
        public int? Workchain { get; set; }
        public Cell? Code { get; set; }
        
        public Cell? NftItemCode { get; set; }
        public Address OwnerAddress { get; set; }
        public Address RoyaltyAddress { get; set; }
        public string CollectionContentUri { get; set; }
        public string NftItemContentBaseUri { get; set; }
        public double Royalty { get; set; }
    }

    public struct NftMintOptions
    {
        public ulong ItemIndex { get; set; }
        public Coins Amount { get; set; }
        public Address ItemOwnerAddress { get; set; }
        public string ItemContentUri { get; set; }
        public ulong? QueryId { get; set; }
    }
    
    public struct NftEditContentOptions
    {
        public string CollectionContentUri { get; set; }
        public string NftItemContentBaseUri { get; set; }
        public Address RoyaltyAddress { get; set; }
        public double Royalty { get; set; }
        public ulong? QueryId { get; set; }
    }
    
    public class NftCollection : ContractBase
    {
        private double _royalty;
        private Address _ownerAddress;
        private Address _royaltyAddress;
        private string _collectionContentUri;
        private string _nftItemContentBaseUri;
        private Cell _nftItemCode;
        
        public NftCollection(NftCollectionOptions options)
        {
            if (options.Royalty > 1)
                throw new ArgumentException("Royalty cannot be greater than 1");
            
            _royalty = options.Royalty;
            _ownerAddress = options.OwnerAddress;
            _royaltyAddress = options.RoyaltyAddress;
            _collectionContentUri = options.CollectionContentUri;
            _nftItemContentBaseUri = options.NftItemContentBaseUri;
            _nftItemCode = options.NftItemCode ?? Cell.From(Models.NFT_ITEM_CODE_HEX);
            
            _code = options.Code ?? Cell.From(Models.NFT_COLLECTION_CODE_HEX);
            _stateInit = BuildStateInit();
            _address = new Address(options.Workchain ?? 0, _stateInit);
        }
        
        public static Cell CreateMintRequest(NftMintOptions opt)
        {
            var body = new CellBuilder()
                .StoreUInt(1, 32)
                .StoreUInt(opt.QueryId ?? SmcUtils.GenerateQueryId(60), 64)
                .StoreUInt(opt.ItemIndex, 64)
                .StoreCoins(opt.Amount);
            
            var nftItemContent = new CellBuilder()
                .StoreAddress(opt.ItemOwnerAddress)
                .StoreRef(new CellBuilder()
                    .StoreString(opt.ItemContentUri)
                    .Build())
                .Build();
            
            body.StoreRef(nftItemContent);
            return body.Build();
        }
        
        public static Cell CreateGetRoyaltyParamsRequest(ulong? queryId)
        {
            var body = new CellBuilder()
                .StoreUInt(0x693d3950, 32)
                .StoreUInt(queryId ?? SmcUtils.GenerateQueryId(60), 64);
            return body.Build();
        }
        
        public static Cell CreateChangeOwnerRequest(Address address, ulong? queryId)
        {
            var body = new CellBuilder()
                .StoreUInt(3, 32)
                .StoreUInt(queryId ?? SmcUtils.GenerateQueryId(60), 64)
                .StoreAddress(address);
            return body.Build();
        }
        
        public static Cell CreateEditContentRequest(NftEditContentOptions opt)
        {
            if (opt.Royalty > 1)
                throw new ArgumentException("Royalty cannot be greater than 1");
            
            // make content ref
            var collectionContentCell = SmcUtils.CreateOffChainUriCell(opt.CollectionContentUri);
            var commonContentCell = new CellBuilder()
                .StoreString(opt.NftItemContentBaseUri)
                .Build();
            var contentCell = new CellBuilder()
                .StoreRef(collectionContentCell)
                .StoreRef(commonContentCell).Build();
            
            // make royalty ref
            var royaltyCell = new CellBuilder()
                .StoreUInt((int)Math.Floor(opt.Royalty * 1000), 16)
                .StoreUInt(1000, 16)
                .StoreAddress(opt.RoyaltyAddress).Build();
            
            var body = new CellBuilder()
                .StoreUInt(4, 32)
                .StoreUInt(opt.QueryId ?? SmcUtils.GenerateQueryId(60), 64)
                .StoreRef(contentCell)
                .StoreRef(royaltyCell);
            return body.Build();
        }
        
        protected sealed override StateInit BuildStateInit()
        {
            // make content ref
            var collectionContentCell = SmcUtils.CreateOffChainUriCell(_collectionContentUri);
            var commonContentCell = new CellBuilder()
                .StoreString(_nftItemContentBaseUri)
                .Build();
            var contentCell = new CellBuilder()
                .StoreRef(collectionContentCell)
                .StoreRef(commonContentCell).Build();
            
            // make royalty ref
            var royaltyCell = new CellBuilder()
                .StoreUInt((int)Math.Floor(_royalty * 1000), 16)
                .StoreUInt(1000, 16)
                .StoreAddress(_royaltyAddress).Build();

            var data = new CellBuilder()
                .StoreAddress(_ownerAddress)
                .StoreUInt(0, 64)
                .StoreRef(contentCell)
                .StoreRef(_nftItemCode)
                .StoreRef(royaltyCell).Build();
            return new StateInit(new StateInitOptions { Code = _code, Data = data });
        }
    }
}