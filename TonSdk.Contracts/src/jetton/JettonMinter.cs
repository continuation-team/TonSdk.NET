using TonSdk.Core;
using TonSdk.Core.Block;
using TonSdk.Core.Boc;

namespace TonSdk.Contracts.Jetton
{
    public struct JettonMinterOptions : ContractBaseOptions
    {
        public Address AdminAddress { get; set; }
        public IJettonContentStorage JettonContent { get; set; }
        public int? Workchain { get; set; }
        public Cell? Code { get; set; }
    }
    
    public interface IJettonContentStorage { }

    public struct JettonOffChainContent : IJettonContentStorage
    {
        public string ContentUri { get; set; }
    }
    
    public struct JettonOnChainContent : IJettonContentStorage
    {
        public string? Uri  { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string? Image { get; set; }
        public string Symbol { get; set; }
        public uint? Decimals { get; set; }
        public string RenderType { get; set; }
        public string AmountStyle { get; set; }
        public string? ImageData { get; set; }
        
    }
    
    public struct JettonMintOptions 
    {
        public ulong? QueryId;
        public Coins JettonAmount;
        public Coins Amount;
        public Address Destination;
    }
    
    public struct JettonChangeAdminOptions 
    {
        public ulong? QueryId;
        public Address NewOwner;
    }
    
    public struct JettonEditContentOptions 
    {
        public ulong? QueryId;
        public IJettonContentStorage JettonContent { get; set; }
    }
    
    public class JettonMinter : ContractBase
    {
        private Address _adminAddress;
        private IJettonContentStorage _content;

        public JettonMinter(JettonMinterOptions options)
        {
            _adminAddress = options.AdminAddress;
            _content = options.JettonContent;
            _code = options.Code ?? Cell.From(Models.JETTON_MINTER_CODE_HEX);
            _stateInit = BuildStateInit();
            _address = new Address(options.Workchain ?? 0, _stateInit);
        }

        public static Cell CreateMintRequest(JettonMintOptions opt)
        {
            var builder = new CellBuilder()
                .StoreUInt(21, 32)
                .StoreUInt(opt.QueryId ?? SmcUtils.GenerateQueryId(60), 64)
                .StoreAddress(opt.Destination)
                .StoreCoins(opt.Amount);

            var link = new CellBuilder()
                .StoreUInt(0x178d4519, 32)
                .StoreUInt(opt.QueryId ?? SmcUtils.GenerateQueryId(60), 64)
                .StoreCoins(opt.JettonAmount)
                .StoreAddress(null)
                .StoreAddress(null)
                .StoreCoins(new Coins(0))
                .StoreBit(false)
                .Build();
            
            builder.StoreRef(link);
            return builder.Build();
        }
        
        public static Cell CreateChangeAdminRequest(JettonChangeAdminOptions opt)
        {
            var builder = new CellBuilder()
                .StoreUInt(3, 32)
                .StoreUInt(opt.QueryId ?? SmcUtils.GenerateQueryId(60), 64)
                .StoreAddress(opt.NewOwner);
            
            return builder.Build();
        }
        
        public static Cell CreateEditContentRequest(JettonEditContentOptions opt)
        {
            var builder = new CellBuilder()
                .StoreUInt(4, 32)
                .StoreUInt(opt.QueryId ?? SmcUtils.GenerateQueryId(60), 64)
                .StoreRef(opt.JettonContent is JettonOffChainContent content 
                    ? SmcUtils.CreateOffChainUriCell(content.ContentUri) :
                    SmcUtils.CreateOnChainUriCell((JettonOnChainContent)opt.JettonContent));
            
            return builder.Build();
        }

        protected sealed override StateInit BuildStateInit() {
            var data = new CellBuilder()
                .StoreCoins(new Coins(0))
                .StoreAddress(_adminAddress)
                .StoreRef(_content is JettonOffChainContent content 
                    ? SmcUtils.CreateOffChainUriCell(content.ContentUri) :
                    SmcUtils.CreateOnChainUriCell((JettonOnChainContent)_content))
                .StoreRef(Cell.From(Models.JETTON_WALLET_CODE_HEX))
                .Build();
            return new StateInit(new StateInitOptions { Code = _code, Data = data });
        }
    }
}