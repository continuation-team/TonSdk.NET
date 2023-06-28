using System.Numerics;
using TonSdk.Core;
using TonSdk.Core.Boc;

namespace TonSdk.Client;

public class MetadataKeys : Dictionary<string, BigInteger>{}

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

    public struct JettonData
    {
        public Coins TotalSupply;
        public Address AdminAddress;
        public Cell Content;
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

    private async Task<JettonData> GetData(Address jettonMasterContract, MetadataKeys? metadateKeys = null)
    {
        RunGetMethodResult runGetMethodResult = await client.RunGetMethod(jettonMasterContract, "get_jetton_data");
        if (runGetMethodResult.ExitCode != 0 && runGetMethodResult.ExitCode != 1) throw new Exception("Cannot retrieve jetton wallet data.");
        JettonData jettonData = new()
        {
            TotalSupply = new Coins((decimal)(BigInteger)runGetMethodResult.Stack[0], new CoinsOptions(true, 9)),
            AdminAddress = ((Cell)runGetMethodResult.Stack[2]).Parse().LoadAddress()!,
            Content = (Cell)runGetMethodResult.Stack[3]!,
            JettonWalletCode = (Cell)runGetMethodResult.Stack[4]!
        };
        return jettonData;
    }

    private async Task<uint> GetDecimalsByWallet(Address jettonWallet)
    {
        JettonWalletData jettonWalletData = await GetWalletData(jettonWallet);
        return await GetDecimals(jettonWalletData.JettonMasterAddress);
    }

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

    public async Task<Coins> GetBalance(Address jettonWallet)
    {
        JettonWalletData jettonWalletData = await GetWalletData(jettonWallet);
        return jettonWalletData.Balance;
    }

    public async Task<Address> GetWalletAddress(Address jettonMasterContract, Address walletOwner)
    {
        string[][] stack = new string[1][] { Transformers.PackRequestStack(walletOwner) };
        RunGetMethodResult runGetMethodResult = await client.RunGetMethod(jettonMasterContract, "get_wallet_address", stack);
        Address resultAddress = ((Cell)runGetMethodResult.Stack[0]).Parse().LoadAddress()!;
        return resultAddress;
    }
}