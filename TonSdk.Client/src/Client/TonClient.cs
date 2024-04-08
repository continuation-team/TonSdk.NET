﻿using System;
using System.Threading.Tasks;
using TonSdk.Client.Stack;
using TonSdk.Core;
using TonSdk.Core.Block;
using TonSdk.Core.Boc;

namespace TonSdk.Client
{
    public class TonClient
    {
        private TonClientType _type;
        private HttpApi _httpApi;
        private LiteClientApi _liteClientApi;
        
        public Jetton Jetton { get; private set; }
        public Nft Nft { get; private set; }
        public Wallet Wallet { get; private set; }
        public Dns Dns { get; private set; }
        
        public TonClient(TonClientType clientType, ITonClientOptions options)
        {
            Jetton = new Jetton(this);
            Nft = new Nft(this);
            Wallet = new Wallet(this);
            Dns = new Dns(this);
            
            switch (clientType)
            {
                case TonClientType.HTTP_TONCENTERAPIV2:
                {
                    if (!(options is HttpParameters)) 
                        throw new Exception("Wrong provided options for HTTP Client type, use HttpParameters instead.");
                    var opts = (HttpParameters)options;
                    _httpApi = new HttpApi(opts);
                    break;
                }
                case TonClientType.LITECLIENT:
                {
                    if (!(options is LiteClientParameters)) 
                        throw new Exception("Wrong provided options for Lite Client type, use LiteClientParameters instead.");
                    
                    var opts = (LiteClientParameters)options;
                    _liteClientApi = new LiteClientApi(opts.Host, opts.Port, opts.PeerPublicKey);
                    break;
                }
            }

            _type = clientType;
        }

        public TonClientType GetClientType() => _type;
        
        /// <summary>
        /// Retrieves the balance of the specified address.
        /// </summary>
        /// <param name="address">The address to retrieve the balance for.</param>
        /// <returns>The task result contains the balance as a Coins instance.</returns>
        public async Task<Coins> GetBalance(Address address)
        {
            return (await GetAddressInformation(address))?.Balance;
        }

        /// <summary>
        /// Checks if a contract is deployed at the specified address.
        /// </summary>
        /// <param name="address">The address to check.</param>
        /// <returns>The task result indicates whether a contract is deployed (true) or not (false) at the specified address.</returns>
        public async Task<bool> IsContractDeployed(Address address)
        {
            return (await GetAddressInformation(address))?.State == AccountState.Active;
        }

        /// <summary>
        /// Retrieves the address information for the specified address.
        /// </summary>
        /// <param name="address">The address object to retrieve information for.</param>
        /// <returns>An object containing the address information.</returns>
        public async Task<AddressInformationResult?> GetAddressInformation(Address address)
        {
            return _type switch
            {
                TonClientType.HTTP_TONCENTERAPIV2 => await _httpApi.GetAddressInformation(address),
                TonClientType.LITECLIENT => await _liteClientApi.GetAddressInformation(address),
                _ => null
            };
        }
        
        /// <summary>
        /// Retrieves the wallet information for the specified address.
        /// </summary>
        /// <param name="address">The address object to retrieve information for.</param>
        /// <returns>An object containing the wallet information.</returns>
        public async Task<WalletInformationResult?> GetWalletInformation(Address address)
        {
            return _type switch
            {
                TonClientType.HTTP_TONCENTERAPIV2 => await _httpApi.GetWalletInformation(address),
                TonClientType.LITECLIENT => await _liteClientApi.GetWalletInformation(address),
                _ => null
            };
        }
        
        /// <summary>
        /// Get up-to-date masterchain state.
        /// </summary>
        /// <returns>An object containing the masterchain information.</returns>
        public async Task<MasterchainInformationResult?> GetMasterchainInfo()
        {
            return _type switch
            {
                TonClientType.HTTP_TONCENTERAPIV2 => await _httpApi.GetMasterchainInfo(),
                TonClientType.LITECLIENT => await _liteClientApi.GetMasterchainInfo(),
                _ => null
            };
        }

        /// <summary>
        /// Look up block by either seqno, lt or unixTime.
        /// </summary>
        /// <returns>An object containing the block information.</returns>
        public async Task<BlockIdExtended?> LookUpBlock(int workchain, long shard, long? seqno = null, ulong? lt = null, ulong? unixTime = null)
        {
            if (seqno == null && lt == null && unixTime == null)
                throw new ArgumentException("Seqno, lt or unixTime should be defined");
            
            return _type switch
            {
                TonClientType.HTTP_TONCENTERAPIV2 => await _httpApi.LookUpBlock(workchain, shard, seqno, lt, unixTime),
                TonClientType.LITECLIENT => await _liteClientApi.LookUpBlock(workchain, shard, seqno, lt, unixTime),
                _ => null
            };
        }
        
        /// <summary>
        /// Get metadata of a given block.
        /// </summary>
        /// <returns>An object containing the block metadata.</returns>
        public async Task<BlockDataResult?> GetBlockData(int workchain,
            long shard,
            long seqno,
            string rootHash = null,
            string fileHash = null)
        {
            return _type switch
            {
                TonClientType.HTTP_TONCENTERAPIV2 => await _httpApi.GetBlockHeader(workchain, shard, seqno, rootHash, fileHash),
                TonClientType.LITECLIENT => throw new Exception("Method not supported yet."),
                _ => null
            };
        }

        /// <summary>
        /// Get transactions of the given block.
        /// </summary>
        /// <param name="workchain"></param>
        /// <param name="shard"></param>
        /// <param name="seqno"></param>
        /// <param name="rootHash"></param>
        /// <param name="fileHash"></param>
        /// <param name="afterLt"></param>
        /// <param name="afterHash"></param>
        /// <param name="count"></param>
        /// <returns>An object containing the shards information.</returns>
        public async Task<BlockTransactionsResult?> GetBlockTransactions(
            int workchain,
            long shard,
            long seqno,
            string rootHash = null,
            string fileHash = null,
            ulong? afterLt = null,
            string afterHash = null,
            uint count = 10)
        {
            return _type switch
            {
                TonClientType.HTTP_TONCENTERAPIV2 => await _httpApi.GetBlockTransactions(workchain, shard, seqno, rootHash, fileHash, afterLt, afterHash, count),
                TonClientType.LITECLIENT => await _liteClientApi.GetBlockTransactions(workchain, shard, seqno, rootHash, fileHash, afterLt, afterHash, count),
                _ => null
            };
        }
        
        /// <summary>
        /// Retrieves transaction information for the specified address.
        /// </summary>
        /// <param name="address">The address object to retrieve transaction information for.</param>
        /// <param name="limit">The maximum number of transactions to retrieve (default: 10).</param>
        /// <param name="lt">The logical time of the transaction to start retrieving from (optional).</param>
        /// <param name="hash">The hash of the transaction to start retrieving from (optional).</param>
        /// <param name="to_lt">The logical time of the transaction to retrieve up to (optional).</param>
        /// <param name="archival">Specifies whether to retrieve transactions from archival nodes (optional).</param>
        /// <returns>An array of transaction information results.</returns>
        public async Task<TransactionsInformationResult[]?> GetTransactions(Address address, uint limit = 10,
            ulong? lt = null, string hash = null, ulong? to_lt = null, bool? archival = null)
        {
            if(_type == TonClientType.LITECLIENT &&  (lt == null || hash == null))
                throw new ArgumentException("From lt and hash, must be defined for LiteClient type.");
            
            return _type switch
            {
                TonClientType.HTTP_TONCENTERAPIV2 => await _httpApi.GetTransactions(address, limit, lt, hash, to_lt, archival),
                TonClientType.LITECLIENT => await _liteClientApi.GetTransactions(address, limit, (long)lt, hash),
                _ => null
            };
        }
        
        /// <summary>
        /// Executes a specific method on the specified address.
        /// </summary>
        /// <param name="address">The address object to execute the method on.</param>
        /// <param name="method">The name of the method to execute.</param>
        /// <param name="stackItems">The stack parameters for the method (optional).</param>
        /// <returns>The result of the executed method.</returns>
        public async Task<RunGetMethodResult?> RunGetMethod(Address address, string method, IStackItem[] stackItems)
        {
            if (_type != TonClientType.LITECLIENT)
                throw new ArgumentException("IStackItem[] stackItems, must be defined with LiteClient type.");
            return await _liteClientApi.RunGetMethod(address, method, stackItems);
        }
        
        /// <summary>
        /// Executes a specific method on the specified address.
        /// </summary>
        /// <param name="address">The address object to execute the method on.</param>
        /// <param name="method">The name of the method to execute.</param>
        /// <param name="stack">The stack parameters for the method (optional).</param>
        /// <returns>The result of the executed method.</returns>
        public async Task<RunGetMethodResult?> RunGetMethod(Address address, string method, string[][] stack)
        {
            if (_type != TonClientType.HTTP_TONCENTERAPIV2)
                throw new ArgumentException("string[][] stack, must be defined with HTTP_TONCENTERAPIV2 type.");
            return await _httpApi.RunGetMethod(address, method, stack);
        }
        
        /// <summary>
        /// Sends a Bag of Cells (BoC) to the network.
        /// </summary>
        /// <param name="boc">The Cell object representing the Bag of Cells.</param>
        /// <returns>The result of sending the Bag of Cells.</returns>
        public async Task<SendBocResult?> SendBoc(Cell boc)
        {
            return _type switch
            {
                TonClientType.HTTP_TONCENTERAPIV2 => await _httpApi.SendBoc(boc),
                TonClientType.LITECLIENT => await _liteClientApi.SendBoc(boc),
                _ => null
            };
        } 
        
        /// <summary>
        /// Retrieves a configuration parameter by its ID.
        /// </summary>
        /// <param name="configId">The ID of the configuration parameter to retrieve.</param>
        /// <param name="seqno">The sequence number of the configuration parameter (optional).</param>
        /// <returns>The result of the configuration parameter retrieval.</returns>
        public async Task<ConfigParamResult?> GetConfigParam(int configId, int? seqno = null)
        {
            return _type switch
            {
                TonClientType.HTTP_TONCENTERAPIV2 => await _httpApi.GetConfigParam(configId, seqno),
                TonClientType.LITECLIENT => await _liteClientApi.GetConfigParam(configId),
                _ => null
            };
        } 
        
        /// <summary>
        /// Get shards information.
        /// </summary>
        /// <param name="seqno">Masterchain seqno to fetch shards of.</param>
        /// <returns>An object containing the shards information.</returns>
        public async Task<ShardsInformationResult?> Shards(long seqno)
        {
            return _type switch
            {
                TonClientType.HTTP_TONCENTERAPIV2 => await _httpApi.Shards(seqno),
                TonClientType.LITECLIENT => await _liteClientApi.GetShards(seqno),
                _ => null
            };
        }

        /// <summary>
        /// Estimates fee for the message
        /// </summary>
        /// <param name="message">The message for which you need to calculate the fees</param>
        /// <returns>The result of estimation fees.</returns>
        public async Task<EstimateFeeResult?> EstimateFee(MessageX message, bool ignoreChksig = true)
        {
            return _type switch
            {
                TonClientType.HTTP_TONCENTERAPIV2 => await _httpApi.EstimateFee(message, ignoreChksig),
                TonClientType.LITECLIENT => throw new Exception("Method cannot be called with LiteClient. Use other client type instead."),
                _ => null
            };
        }
    }
}