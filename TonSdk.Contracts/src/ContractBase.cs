using TonSdk.Core;
using TonSdk.Core.Block;
using TonSdk.Core.Boc;

namespace TonSdk.Contracts
{
    public interface ContractBaseOptions
    {
        public Cell Code { get; set; }
    }

    public abstract class ContractBase
    {
        protected Cell _code;
        protected StateInit _stateInit;

        public Cell Code => _code;
        public StateInit StateInit => _stateInit;


        /// <summary>
        ///  Get wallet address with default AddressStringifyOptions
        /// </summary>
        /// <param name="options">
        /// AddressStringifyOptions
        /// <remarks>Default options: AddressStringifyOptions(bounceable: false,testOnly: false,urlSafe: true)</remarks>
        /// </param>
        /// <returns></returns>
        public virtual Address ToAddress(IAddressRewriteOptions? options = null)
        {
            var defaultOptions = new AddressStringifyOptions(false, false, true);
            return new Address(options?.Workchain ?? defaultOptions.Workchain!.Value, _stateInit,
                options ?? defaultOptions);
        }

        protected abstract StateInit BuildStateInit();
    }
}