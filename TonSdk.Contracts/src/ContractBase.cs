using TonSdk.Core;
using TonSdk.Core.Block;
using TonSdk.Core.Boc;

namespace TonSdk.Contracts
{
    public interface ContractBaseOptions
    {
        public int? Workchain { get; set; }
        public Cell Code { get; set; }
    }
    
    public abstract class ContractBase
    {
        protected Cell _code;
        protected StateInit _stateInit;
        protected Address? _address;
        
        public Cell Code => _code;

        public StateInit StateInit => _stateInit;

        public Address Address => _address;
        protected abstract StateInit BuildStateInit();
        
    }
}