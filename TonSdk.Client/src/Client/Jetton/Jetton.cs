using System.Numerics;
using TonSdk.Core;
using TonSdk.Core.Boc;

namespace TonSdk.Client;

public class MetadataKeys : Dictionary<string, BigInteger>{}

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
        RunGetMethodResult runGetMethodResult = await client.RunGetMethod(jettonWallet, "get_wallet_data");
        if (runGetMethodResult.ExitCode == -13) throw new Exception("Jetton wallet is not deployed");
        if (runGetMethodResult.ExitCode != 0 && runGetMethodResult.ExitCode != 1) throw new Exception("Cannot retrieve jetton wallet data.");

        Address jettonMasterAddress = ((Cell)runGetMethodResult.Stack[2]).Parse().LoadAddress()!;
        uint decimals = await GetDecimals(jettonMasterAddress);

        JettonWalletData jettonWalletData = new()
        {
            Balance = new((decimal)(BigInteger)runGetMethodResult.Stack[0], new CoinsOptions(true, (int)decimals)),
            OwnerAddress = ((Cell)runGetMethodResult.Stack[1]).Parse().LoadAddress()!,
            JettonMasterAddress = jettonMasterAddress,
            JettonWalletCode = (Cell)runGetMethodResult.Stack[3]
        };

        return jettonWalletData;
    }

    private async Task<uint> GetDecimals(Address jettonMasterContract)
    {
        JettonData jettonData = await GetData(jettonMasterContract);

        // TODO: return ~~(content.decimals) || 9;
        return 9;
    }

    public async Task<JettonData> GetData(Address jettonMasterContract, MetadataKeys? metadateKeys = null)
    {
        RunGetMethodResult runGetMethodResult = await client.RunGetMethod(jettonMasterContract, "get_jetton_data");
        if (runGetMethodResult.ExitCode != 0 && runGetMethodResult.ExitCode != 1) throw new Exception("Cannot retrieve jetton wallet data.");
        JettonData jettonData = new()
        {
            TotalSupply = new Coins((decimal)(BigInteger)runGetMethodResult.Stack[0], new CoinsOptions(true, 9)),
            AdminAddress = ((Cell)runGetMethodResult.Stack[2]).Parse().LoadAddress()!,
            Content = await JettonUtils.ParseMetadata((Cell)runGetMethodResult.Stack[3]!),
            JettonWalletCode = (Cell)runGetMethodResult.Stack[4]!
        };
        return jettonData;
    }

    private async Task<uint> GetDecimalsByWallet(Address jettonWallet)
    {
        JettonWalletData jettonWalletData = await GetWalletData(jettonWallet);
        return await GetDecimals(jettonWalletData.JettonMasterAddress);
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
        TransactionsInformationResult[] transactionsInformationResults = await client.GetTransactions(jettonWallet, limit);
        uint jettonDecimals = decimals ?? await GetDecimalsByWallet(jettonWallet);

        IJettonTransaction[] parsedTransactions = new IJettonTransaction[transactionsInformationResults.Length];
        var j = 0;
        for (var i = 0; i < transactionsInformationResults.Length; i++)
        {
            IJettonTransaction? parsedTransaction = TransactionParser.ParseTransaction(transactionsInformationResults[i], jettonDecimals);

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
        string[][] stack = new string[1][] { Transformers.PackRequestStack(walletOwner) };
        RunGetMethodResult runGetMethodResult = await client.RunGetMethod(jettonMasterContract, "get_wallet_address", stack);
        Address resultAddress = ((Cell)runGetMethodResult.Stack[0]).Parse().LoadAddress()!;
        return resultAddress;
    }


}