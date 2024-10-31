﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using TonSdk.Client.Stack;
using TonSdk.Core;
using TonSdk.Core.Block;
using TonSdk.Core.Boc;
using static TonSdk.Client.Transformers;

namespace TonSdk.Client
{
    public class HttpApiV3 : IDisposable
    {
        private readonly HttpClient _httpClient;
        
        public HttpApiV3(HttpParameters httpApiParameters)
        {
            if (string.IsNullOrEmpty(httpApiParameters.Endpoint))
            {
                throw new ArgumentNullException("Endpoint field in Http options cannot be null.");
            }

            _httpClient = new HttpClient();

            _httpClient.Timeout = TimeSpan.FromMilliseconds(Convert.ToDouble(httpApiParameters.Timeout ?? 30000));
            
            _httpClient.DefaultRequestHeaders.Accept.Clear();
           
            
            if (!string.IsNullOrEmpty(httpApiParameters.ApiKey))
                _httpClient.DefaultRequestHeaders.Add("X-API-Key", httpApiParameters.ApiKey);

            _httpClient.BaseAddress = new Uri(httpApiParameters.Endpoint);
        }
        
        internal async Task<AddressInformationResult> GetAddressInformation(Address address)
        {
            string result = await new TonRequestV3(new RequestParametersV3("account", new Dictionary<string, object>()
                {
                    {
                        "address", address.ToString()
                    }
                }), _httpClient).CallGet();
            
            var addressInformationResult = new AddressInformationResult(JsonConvert.DeserializeObject<OutV3AddressInformationResult>(result));
            return addressInformationResult;
        }
        
        internal async Task<WalletInformationResult> GetWalletInformation(Address address)
        {
            string result = await new TonRequestV3(new RequestParametersV3("wallet", new Dictionary<string, object>()
            {
                {
                    "address", address.ToString()
                }
            }), _httpClient).CallGet();
            return result == "conflict" 
                ? new WalletInformationResult(await GetAddressInformation(address)) 
                : new WalletInformationResult(JsonConvert.DeserializeObject<OutV3WalletInformationResult>(result));
        }
        
        internal async Task<MasterchainInformationResult> GetMasterchainInfo()
        {
            string result = await new TonRequestV3(new RequestParametersV3("masterchainInfo", new Dictionary<string, object>()), _httpClient).CallGet();
            return new MasterchainInformationResult(JsonConvert.DeserializeObject<OutV3MasterchainInformationResult>(result));
        }
        
        internal async Task<BlockIdExtended> LookUpBlock(int workchain, long shard, long? seqno = null, ulong? lt = null, ulong? unixTime = null)
        {
            var req = new Dictionary<string, object>()
            {
                {
                    "workchain", workchain.ToString()
                },
                {
                    "shard", shard.ToString()
                },
                {
                    "offset", "0"
                },
                {
                    "limit", "1"
                },
                {
                    "sort", "asc"
                }
            };
            if (seqno != null)
                req.Add("seqno", seqno.Value.ToString());
            if (lt != null)
                req.Add("start_lt", lt.Value.ToString());
            if (unixTime != null)
                req.Add("start_utime", unixTime.Value.ToString());
            
            var result = await new TonRequestV3(new RequestParametersV3("blocks", req), _httpClient).CallGet();
            var blocks = JsonConvert.DeserializeObject<RootV3LookUpBlock>(result).Blocks;
            
            return blocks.Length != 0 ? blocks[0] : null;
        }
        
        internal async Task<ShardsInformationResult> Shards(long seqno)
        {
            string result = await new TonRequestV3(new RequestParametersV3("masterchainBlockShardState", new Dictionary<string, object>()
            {
                {
                    "seqno", seqno.ToString()
                }
            }), _httpClient).CallGet();
            return new ShardsInformationResult(JsonConvert.DeserializeObject<OutV3ShardsInformationResult>(result));
        }
        
        internal async Task<TransactionsInformationResult[]> GetTransactions(Address address, uint limit = 10,
            ulong? lt = null, string hash = null, ulong? to_lt = null, bool? archival = null)
        {
            var addresses = address is null ? null : new[] { address };
            var result = await GetTransactionsInternal(addresses: addresses, hash: hash, startLt: lt, endLt: to_lt, limit: limit);
            
            var data = JsonConvert.DeserializeObject<RootTransactions>(result).Transactions;
            return data.Select(t => new TransactionsInformationResult(t)).ToArray();
        }

        internal Task<string> GetTransactionsInternal(
            int? workchain = null,
            long? shard = null,
            long? seqno = null,
            IEnumerable<Address> addresses = null, 
            string hash = null,
            ulong? startLt = null,
            ulong? endLt = null,
            uint limit = 10,
            int offset = 0,
            SortDirection sort = SortDirection.DESC)
        {
            var dict = new Dictionary<string, object>
            {
                { "offset", offset.ToString() },
                { "limit", limit.ToString() },
                { "sort", sort.ToString().ToLower() }
            };

            if (workchain.HasValue) dict.Add("workchain", workchain.ToString());
            if (shard.HasValue) dict.Add("shard", shard.Value.ToString("X"));
            if (seqno.HasValue) dict.Add("seqno", seqno.ToString());

            if (addresses != null)
                foreach (var address in addresses)
                    dict.Add("account", address.ToString(AddressType.Raw));

            if (!string.IsNullOrEmpty(hash)) dict.Add("hash", hash);
            
            if (startLt != null) dict.Add("start_lt", startLt.ToString());
            if (endLt != null) dict.Add("end_lt", endLt.ToString());

            return new TonRequestV3(new RequestParametersV3("transactions", dict), _httpClient).CallGet();
        }
        
        internal async Task<BlockTransactionsResult> GetBlockTransactions(
            int workchain,
            long shard,
            long seqno,
            string rootHash = null,
            string fileHash = null,
            ulong? afterLt = null,
            string afterHash = null,
            uint? count = null)
        {
            var result = await GetTransactionsInternal(workchain: workchain, shard: shard, seqno: seqno, startLt: afterLt, limit: count ?? 10);
            
            var data = JsonConvert.DeserializeObject<RootBlockTransactions>(result).Transactions;
            var transactions = data.Select(item => new ShortTransactionsResult() { Account = item.Account, Hash = item.Hash, Lt = item.Lt, Mode = item.Description.ComputePh.Mode }).ToList();
            return new BlockTransactionsResult()
            {
                Id = new BlockIdExtended()
                {
                    Seqno = seqno,
                    Workchain = workchain,
                    Shard = shard
                },
                ReqCount = count != null ? (int)count : 10,
                Incomplete = false,
                Transactions = transactions.ToArray()
            };
        }
        
        internal async Task<TransactionsInformationResult[]> GetTransactionsByMessage(string msgHash, string bodyHash, string opcode, MessageDirection? direction = null, int? offset = null, int? count = null)
        {
            var dict = new Dictionary<string, object>();
                
            if (!string.IsNullOrEmpty(msgHash)) 
                dict.Add("msg_hash", msgHash);
                
            if (!string.IsNullOrEmpty(bodyHash)) 
                dict.Add("body_hash", bodyHash);
                
            if (!string.IsNullOrEmpty(opcode)) 
                dict.Add("opcode", opcode);
            
            if (direction.HasValue) 
                dict.Add("direction", direction.Value.ToString().ToLower());

            if (offset.HasValue) 
                dict.Add("offset", offset);
            
            if (count.HasValue) 
                dict.Add("limit", count);
            
            string result = await new TonRequestV3(new RequestParametersV3("transactionsByMessage", dict), _httpClient).CallGet();
            
            var data = JsonConvert.DeserializeObject<RootTransactions>(result, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            }).Transactions;
            return data.Select(t => new TransactionsInformationResult(t)).ToArray();
        }
        
        internal async Task<SendBocResult> SendBoc(Cell boc)
        {
            string result = await new TonRequestV3(new RequestParametersV3("message", new Dictionary<string, object>()
            {
                {
                    "boc", boc.ToString("base64")
                }
            }), _httpClient).CallPost();
            var resultRoot = JsonConvert.DeserializeObject<RootSendBoc>(result);
            return new SendBocResult()
                {
                    Hash = resultRoot.MessageHash,
                    Type = "1"
                };
        }
        
        internal async Task<RunGetMethodResult> RunGetMethod(Address address, string method, StackJsonItem[] items)
        {
            string result = await new TonRequestV3(new RequestParametersV3("runGetMethod", new Dictionary<string, object>()
            {
                {
                    "address", address.ToString()
                },
                {
                    "method", method
                },
                {
                    "stack", items
                }
            }), _httpClient).CallPost();
            return new RunGetMethodResult(JsonConvert.DeserializeObject<OutV3RunGetMethod>(result));
        }
        
        internal async Task<EstimateFeeResultExtended> EstimateFee(MessageX message, bool ignoreChksig = true)
        {
            var dataMsg = message.Data;

            Address address = (message.Data.Info.Data is IntMsgInfoOptions info) ? info.Dest :
                (message.Data.Info.Data is ExtInMsgInfoOptions info2) ? info2.Dest : null;

            Cell body = dataMsg.Body;
            Cell init_code = dataMsg.StateInit?.Data.Code;
            Cell init_data = dataMsg.StateInit?.Data.Data;

            string result = await new TonRequestV3(new RequestParametersV3("estimateFee", new Dictionary<string, object>()
            {
                {
                    "address", address.ToString()
                },
                {
                    "body", body?.ToString("base64") ?? string.Empty
                },
                {
                    "init_code", init_code?.ToString("base64") ?? string.Empty
                },
                {
                    "init_data", init_data?.ToString("base64") ?? string.Empty
                },
                {
                    "ignore_chksig", ignoreChksig
                }
            }), _httpClient).CallPost();
            
            return JsonConvert.DeserializeObject<EstimateFeeResultExtended>(result);
        }

        public async Task<string> CustomGetMethodCall(string request, List<string[]> body)
        {
            return await new TonRequestV3(new RequestParametersV3(request, new Dictionary<string, object>()), _httpClient).CallGetList(body);
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}