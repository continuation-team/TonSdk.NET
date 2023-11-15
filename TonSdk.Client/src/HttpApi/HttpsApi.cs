﻿using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using TonSdk.Core;
using TonSdk.Core.Block;
using TonSdk.Core.Boc;
using static TonSdk.Client.Transformers;

namespace TonSdk.Client
{
    public class HttpApiParameters
    {
        public string Endpoint { get; set; }
        public int? Timeout { get; set; }
        public string ApiKey { get; set; }
    }

    public abstract class HttpApi : IDisposable
    {
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Initializes a new instance of the HttpApi class with the specified options.
        /// </summary>
        /// <param name="httpApiParameters">The HTTP API parameters.</param>
        /// <exception cref="ArgumentNullException">Thrown when the endpoint field in the options is null.</exception>
        public HttpApi(HttpApiParameters httpApiParameters)
        {
            if (string.IsNullOrEmpty(httpApiParameters.Endpoint))
            {
                throw new ArgumentNullException("Endpoint field in Http options cannot be null.");
            }

            _httpClient = new HttpClient();

            _httpClient.Timeout = TimeSpan.FromMilliseconds(Convert.ToDouble(httpApiParameters.Timeout ?? 30000));
            //httpClient.DefaultRequestHeaders.Add("Content-Type", "application/json");

            if (!string.IsNullOrEmpty(httpApiParameters.ApiKey))
                _httpClient.DefaultRequestHeaders.Add("X-API-Key", httpApiParameters.ApiKey);

            _httpClient.BaseAddress = new Uri(httpApiParameters.Endpoint);
        }

        /// <summary>
        /// Retrieves the address information for the specified address.
        /// </summary>
        /// <param name="address">The address object to retrieve information for.</param>
        /// <returns>An object containing the address information.</returns>
        public async Task<AddressInformationResult> GetAddressInformation(Address address)
        {
            InAdressInformationBody requestBody =
                new InAdressInformationBody(address.ToString());
            var result = await new TonRequest(new RequestParameters("getAddressInformation", requestBody), _httpClient)
                .Call();
            RootAddressInformation resultAddressInformation =
                JsonConvert.DeserializeObject<RootAddressInformation>(result);
            AddressInformationResult addressInformationResult =
                new AddressInformationResult(resultAddressInformation.Result);
            return addressInformationResult;
        }
        
        /// <summary>
        /// Retrieves the wallet information for the specified address.
        /// </summary>
        /// <param name="address">The address object to retrieve information for.</param>
        /// <returns>An object containing the wallet information.</returns>
        public async Task<WalletInformationResult> GetWalletInformation(Address address)
        {
            InAdressInformationBody requestBody =
                new InAdressInformationBody(address.ToString());
            var result = await new TonRequest(new RequestParameters("getWalletInformation", requestBody), _httpClient)
                .Call();
            RootWalletInformation resultWalletInformation =
                JsonConvert.DeserializeObject<RootWalletInformation>(result);
            if (!resultWalletInformation.Ok) throw new Exception("An error occured when requesting a method.");
            WalletInformationResult walletInformationResult =
                new WalletInformationResult(resultWalletInformation.Result);
            return walletInformationResult;
        }

        /// <summary>
        /// Get up-to-date masterchain state.
        /// </summary>
        /// <returns>An object containing the masterchain information.</returns>
        public async Task<MasterchainInformationResult> GetMasterchainInfo()
        {
            var result = await new TonRequest(new RequestParameters("getMasterchainInfo", new EmptyBody()), _httpClient)
                .Call();
            RootMasterchainInformation rootMasterchainInformation =
                JsonConvert.DeserializeObject<RootMasterchainInformation>(result);
            MasterchainInformationResult masterchainInformationResult =
                new MasterchainInformationResult(rootMasterchainInformation.Result);
            return masterchainInformationResult;
        }

        /// <summary>
        /// Get shards information.
        /// </summary>
        /// <param name="seqno">Masterchain seqno to fetch shards of.</param>
        /// <returns>An object containing the shards information.</returns>
        public async Task<ShardsInformationResult> Shards(long seqno)
        {
            var requestBody = new InShardsBody(seqno);
            var result = await new TonRequest(new RequestParameters("shards", requestBody), _httpClient).Call();
            RootShardsInformation rootShardsInformation = JsonConvert.DeserializeObject<RootShardsInformation>(result);
            ShardsInformationResult shardsInformationResult = new ShardsInformationResult(rootShardsInformation.Result);
            return shardsInformationResult;
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
        public async Task<BlockTransactionsResult> GetBlockTransactions(
            int workchain,
            long shard,
            long seqno,
            string rootHash = null,
            string fileHash = null,
            ulong? afterLt = null,
            string afterHash = null,
            int? count = null)
        {
            var requestBody =
                new InBlockTransactions(workchain, shard, seqno, rootHash, fileHash, afterLt, afterHash, count);
            var result = await new TonRequest(new RequestParameters("getBlockTransactions", requestBody), _httpClient)
                .Call();
            RootBlockTransactions rootBlockTransactions = JsonConvert.DeserializeObject<RootBlockTransactions>(result);
            BlockTransactionsResult blockTransactionsResult = new BlockTransactionsResult(rootBlockTransactions.Result);
            return blockTransactionsResult;
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
        public async Task<TransactionsInformationResult[]> GetTransactions(Address address, int limit = 10,
            ulong? lt = null, string hash = null, ulong? to_lt = null, bool? archival = null)
        {
            InTransactionsBody requestBody = new InTransactionsBody()
            {
                address = address.ToString(AddressType.Base64, new AddressStringifyOptions(true, false, false)),
                limit = limit
            };
            if (lt != null) requestBody.lt = (ulong)lt;
            if (hash != null) requestBody.hash = hash;
            if (to_lt != null) requestBody.to_lt = (ulong)to_lt;
            if (archival != null) requestBody.archival = (bool)archival;

            var result = await new TonRequest(new RequestParameters("getTransactions", requestBody), _httpClient)
                .Call();
            RootTransactions resultRoot = JsonConvert.DeserializeObject<RootTransactions>(result);

            TransactionsInformationResult[] transactionsInformationResult =
                new TransactionsInformationResult[resultRoot.Result.Length];
            for (int i = 0; i < resultRoot.Result.Length; i++)
            {
                transactionsInformationResult[i] = new TransactionsInformationResult(resultRoot.Result[i]);
            }

            return transactionsInformationResult;
        }

        /// <summary>
        /// Executes a specific method on the specified address.
        /// </summary>
        /// <param name="address">The address object to execute the method on.</param>
        /// <param name="method">The name of the method to execute.</param>
        /// <param name="stack">The stack parameters for the method (optional).</param>
        /// <returns>The result of the executed method.</returns>
        public async Task<RunGetMethodResult> RunGetMethod(Address address, string method, string[][] stack = null)
        {
            InRunGetMethodBody requestBody = new InRunGetMethodBody()
            {
                address = address.ToString(),
                method = method,
                stack = stack ?? Array.Empty<string[]>()
            };
            var result = await new TonRequest(new RequestParameters("runGetMethod", requestBody), _httpClient).Call();
            RootRunGetMethod resultRoot = JsonConvert.DeserializeObject<RootRunGetMethod>(result);
            RunGetMethodResult outRunGetMethod = new RunGetMethodResult(resultRoot.Result);
            return outRunGetMethod;
        }

        /// <summary>
        /// Sends a Bag of Cells (BoC) to the network.
        /// </summary>
        /// <param name="boc">The Cell object representing the Bag of Cells.</param>
        /// <returns>The result of sending the Bag of Cells.</returns>
        public async Task<SendBocResult> SendBoc(Cell boc)
        {
            InSendBocBody requestBody = new InSendBocBody()
            {
                boc = boc.ToString("base64")
            };
            var result = await new TonRequest(new RequestParameters("sendBoc", requestBody), _httpClient).Call();
            RootSendBoc resultRoot = JsonConvert.DeserializeObject<RootSendBoc>(result);
            SendBocResult outSendBoc = resultRoot.Result;
            return outSendBoc;
        }

        /// <summary>
        /// Estimates fee for the message
        /// </summary>
        /// <param name="transfer">The message for which you need to calculate the fees</param>
        /// <returns>The result of estimation fees.</returns>
        public async Task<EstimateFeeResult> EstimateFee(MessageX message, bool ignoreChksig = true)
        {
            var dataMsg = message.Data;

            Address address = (message.Data.Info.Data is IntMsgInfoOptions info) ? info.Dest :
                      (message.Data.Info.Data is ExtInMsgInfoOptions info2) ? info2.Dest : null;

            Cell body = dataMsg.Body;
            Cell init_code = dataMsg.StateInit?.Data.Code;
            Cell init_data = dataMsg.StateInit?.Data.Data;

            InEstimateFeeBody requestBody = new InEstimateFeeBody()
            {
                address = address.ToString(),
                body = body?.ToString("base64") ?? string.Empty,
                init_code = init_code?.ToString("base64") ?? string.Empty,
                init_data = init_data?.ToString("base64") ?? string.Empty,
                ignore_chksig = ignoreChksig,
            };

            var result = await new TonRequest(new RequestParameters("estimateFee", requestBody), _httpClient).Call();
            RootEstimateFee resultRoot = JsonConvert.DeserializeObject<RootEstimateFee>(result);
            EstimateFeeResult outEstimateFee = resultRoot.Result;
            return outEstimateFee;
        }

        /// <summary>
        /// Retrieves a configuration parameter by its ID.
        /// </summary>
        /// <param name="configId">The ID of the configuration parameter to retrieve.</param>
        /// <param name="seqno">The sequence number of the configuration parameter (optional).</param>
        /// <returns>The result of the configuration parameter retrieval.</returns>
        public async Task<ConfigParamResult> GetConfigParam(int configId, int? seqno = null)
        {
            InGetConfigParamBody requestBody = new InGetConfigParamBody()
            {
                config_id = configId,
            };
            if (seqno != null)
            {
                requestBody.seqno = (int)seqno;
            }

            var result = await new TonRequest(new RequestParameters("getConfigParam", requestBody), _httpClient).Call();
            RootGetConfigParam resultRoot = JsonConvert.DeserializeObject<RootGetConfigParam>(result);
            ConfigParamResult outConfigParam = new ConfigParamResult(resultRoot.Result.Config);
            return outConfigParam;
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}