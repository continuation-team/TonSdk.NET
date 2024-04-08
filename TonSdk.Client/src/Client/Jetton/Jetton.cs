﻿using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using TonSdk.Client.Stack;
using TonSdk.Core;
using TonSdk.Core.Boc;

namespace TonSdk.Client
{
    public struct JettonData
    {
        public Coins TotalSupply;
        public Address AdminAddress;
        public JettonContent Content;
        public Cell JettonWalletCode;
    }

    public class Jetton
    {
        private readonly TonClient client;
        public Jetton(TonClient client)
        {
            this.client = client;
        }

        private struct JettonWalletData
        {
            public Coins Balance;
            public Address OwnerAddress;
            public Address JettonMasterAddress;
            public Cell JettonWalletCode;
        }

        private async Task<JettonWalletData> GetWalletData(Address jettonWallet)
        {
            RunGetMethodResult? runGetMethodResult;
                if(client.GetClientType() == TonClientType.HTTP_TONCENTERAPIV2) 
                    runGetMethodResult = await client.RunGetMethod(jettonWallet, "get_wallet_data", stack: null);
                else
                    runGetMethodResult = await client.RunGetMethod(jettonWallet, "get_wallet_data", Array.Empty<IStackItem>());
                
            if(runGetMethodResult == null) throw new Exception("Cannot retrieve jetton wallet data.");
            if (runGetMethodResult.Value.ExitCode == -13) throw new Exception("Jetton wallet is not deployed");
            if (runGetMethodResult.Value.ExitCode != 0 && runGetMethodResult.Value.ExitCode != 1) throw new Exception("Cannot retrieve jetton wallet data.");

            
            Address jettonMasterAddress = 
                client.GetClientType() == TonClientType.HTTP_TONCENTERAPIV2 ?
                ((Cell)runGetMethodResult.Value.Stack[2]).Parse().LoadAddress()! 
                : ((VmStackSlice)runGetMethodResult.Value.StackItems[2]).Value.LoadAddress();
            uint decimals = await GetDecimals(jettonMasterAddress);

            JettonWalletData jettonWalletData = new JettonWalletData()
            {
                Balance = client.GetClientType() == TonClientType.HTTP_TONCENTERAPIV2 ?
                    new Coins((decimal)(BigInteger)runGetMethodResult.Value.Stack[0], new CoinsOptions(true, (int)decimals)):
                    new Coins((decimal)((VmStackTinyInt)runGetMethodResult.Value.StackItems[0]).Value, new CoinsOptions(true, (int)decimals)),
                OwnerAddress = client.GetClientType() == TonClientType.HTTP_TONCENTERAPIV2 ? 
                    ((Cell)runGetMethodResult.Value.Stack[1]).Parse().LoadAddress()! :
                    ((VmStackSlice)runGetMethodResult.Value.StackItems[1]).Value.LoadAddress(),
                JettonMasterAddress = jettonMasterAddress,
                JettonWalletCode = client.GetClientType() == TonClientType.HTTP_TONCENTERAPIV2 ? 
                    (Cell)runGetMethodResult.Value.Stack[3] : ((VmStackCell)runGetMethodResult.Value.StackItems[3]).Value
            };

            return jettonWalletData;
        }

        private async Task<uint> GetDecimals(Address jettonMasterContract)
        {
            JettonData jettonData = await GetData(jettonMasterContract);
            return jettonData.Content.Decimals;
        }

        private async Task<uint> GetDecimalsByWallet(Address jettonWallet)
        {
            JettonWalletData jettonWalletData = await GetWalletData(jettonWallet);
            return await GetDecimals(jettonWalletData.JettonMasterAddress);
        }

        /// <summary>
        /// Retrieves the jetton data
        /// </summary>
        /// <param name="jettonMasterContract">Jetton master contract address.</param>
        /// <returns>JettonData object with jetton data.</returns>
        /// <exception cref="Exception">Throws when cannot retrieve jetton data.</exception>
        public async Task<JettonData> GetData(Address jettonMasterContract)
        {
            RunGetMethodResult? result;
            if(client.GetClientType() == TonClientType.HTTP_TONCENTERAPIV2) 
                result = await client.RunGetMethod(jettonMasterContract, "get_jetton_data", stack: null);
            else
                result = await client.RunGetMethod(jettonMasterContract, "get_jetton_data", Array.Empty<IStackItem>());
            
            if(result == null) throw new Exception("Cannot retrieve jetton wallet data.");
            if (result.Value.ExitCode != 0 && result.Value.ExitCode != 1) throw new Exception("Cannot retrieve jetton data.");
            
            Address admin;
            var totalSupply = new Coins(0);
            if (client.GetClientType() == TonClientType.HTTP_TONCENTERAPIV2)
            {
                admin = ((Cell)result.Value.Stack[2]).Parse().LoadAddress()!;
                totalSupply = new Coins((decimal)(BigInteger)result.Value.Stack[0], new CoinsOptions(true));
            }
            else
            {
                if (result.Value.StackItems[0] is VmStackInt)
                    totalSupply = new Coins((decimal)((VmStackInt)result.Value.StackItems[0]).Value, new CoinsOptions(true));
                else if (result.Value.StackItems[0] is VmStackTinyInt)
                    totalSupply = new Coins((decimal)((VmStackTinyInt)result.Value.StackItems[0]).Value, new CoinsOptions(true));
                try
                {
                    admin = ((VmStackSlice)result.Value.StackItems[2]).Value.LoadAddress();
                }
                catch
                {
                    admin = null;
                }
            }
            
            var jettonData = new JettonData()
            {
                TotalSupply = totalSupply,
                AdminAddress = admin,
                Content = client.GetClientType() == TonClientType.HTTP_TONCENTERAPIV2 ? 
                    await JettonUtils.ParseMetadata((Cell)result.Value.Stack[3]!) :
                    await JettonUtils.ParseMetadata(((VmStackCell)result.Value.StackItems[3]).Value),
                JettonWalletCode = client.GetClientType() == TonClientType.HTTP_TONCENTERAPIV2 ?
                    (Cell)result.Value.Stack[4]! :
                    ((VmStackCell)result.Value.StackItems[4]).Value
            };
            return jettonData;
        }

        /// <summary>
        /// Retrieves the jetton transactions for a given jetton wallet address.
        /// </summary>
        /// <param name="jettonWallet">The jetton wallet address.</param>
        /// <param name="limit">The maximum number of transactions to retrieve. Defaults to 5.</param>
        /// <param name="decimals">Optional decimals value to parse the transaction amounts. If not provided, it will be fetched from the wallet.</param>
        /// <returns>An array of parsed jetton transactions.</returns>
        public async Task<IJettonTransaction[]> GetTransactions(Address jettonWallet, int limit = 5, uint? decimals = null)
        {
            TransactionsInformationResult[] transactionsInformationResults = await client.GetTransactions(jettonWallet, (uint)limit);
            uint jettonDecimals = decimals ?? await GetDecimalsByWallet(jettonWallet);

            IJettonTransaction[] parsedTransactions = new IJettonTransaction[transactionsInformationResults.Length];
            var j = 0;
            for (var i = 0; i < transactionsInformationResults.Length; i++)
            {
                IJettonTransaction parsedTransaction = TransactionParser.ParseTransaction(transactionsInformationResults[i], jettonDecimals);

                if (parsedTransaction == null) j++;
                else parsedTransactions[i - j] = parsedTransaction;
            }
            return parsedTransactions;
        }

        /// <summary>
        /// Retrieves the balance of a jetton wallet.
        /// </summary>
        /// <param name="jettonWallet">The jetton wallet address.</param>
        /// <returns>The balance of the jetton wallet.</returns>
        public async Task<Coins> GetBalance(Address jettonWallet)
        {
            JettonWalletData jettonWalletData = await GetWalletData(jettonWallet);
            return jettonWalletData.Balance;
        }

        /// <summary>
        /// Retrieves the wallet address associated with the specified jetton master contract and wallet owner.
        /// </summary>
        /// <param name="jettonMasterContract">The jetton master contract address.</param>
        /// <param name="walletOwner">The wallet owner address.</param>
        /// <returns>The wallet address.</returns>
        public async Task<Address> GetWalletAddress(Address jettonMasterContract, Address walletOwner)
        {
            RunGetMethodResult? result;
            if (client.GetClientType() == TonClientType.HTTP_TONCENTERAPIV2)
            {
                string[][] stack = new string[1][] { Transformers.PackRequestStack(walletOwner) };
                result = await client.RunGetMethod(jettonMasterContract, "get_wallet_address", stack);
            }
            else
            {
                var stackItems = new List<IStackItem>()
                {
                    new VmStackSlice()
                    {
                        Value = new CellBuilder().StoreAddress(walletOwner).Build().Parse()
                    }
                };
                result = await client.RunGetMethod(jettonMasterContract, "get_wallet_address", stackItems.ToArray());
            }
            if (result.Value.ExitCode != 0 && result.Value.ExitCode != 1) Console.WriteLine("error");    
            return client.GetClientType() == TonClientType.HTTP_TONCENTERAPIV2 
                ? ((Cell)result.Value.Stack[0]).Parse().LoadAddress()! 
                : ((VmStackSlice)result.Value.StackItems[0]).Value.LoadAddress();
        }
    }
}