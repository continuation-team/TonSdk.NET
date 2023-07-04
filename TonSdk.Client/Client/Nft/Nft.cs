using System;
using TonSdk.Core.Boc;
using TonSdk.Core;
using System.Numerics;
using System.Drawing;

namespace TonSdk.Client;

public struct NftCollectionData
{
    public uint NextItemIndex;
    public Cell Data;
    public Address OwnerAddress;
}

public struct NftRoyaltyParams
{
    public BigInteger Numerator;
    public BigInteger Denominator;
    public Address RoyaltyAddress;
}

public struct NftItemData
{
    public bool Init;
    public uint Index;
    public Address CollectionAddress;
    public Address OwnerAddres;
    public Cell Content;
}

public class Nft
{
    private readonly TonClient client;
    public Nft(TonClient client)
    {
        this.client = client;
    }

    /// <summary>
    /// Retrieves the address of an item in the specified collection by its index.
    /// </summary>
    /// <param name="collection">The address of the collection.</param>
    /// <param name="index">The index of the item.</param>
    /// <returns>The address of the item.</returns>
    public async Task<Address> GetItemAddress(Address collection, uint index)
    {
        string[][] stack = new string[1][] { Transformers.PackRequestStack(index) };
        RunGetMethodResult runGetMethodResult = await client.RunGetMethod(collection, "get_nft_address_by_index", stack);
        if (runGetMethodResult.ExitCode != 0 && runGetMethodResult.ExitCode != 1) throw new Exception("Cannot retrieve nft address.");
        Address resultAddress = ((Cell)runGetMethodResult.Stack[0]).Parse().LoadAddress()!;
        return resultAddress;
    }

    /// <summary>
    /// Retrieves the royalty parameters of the specified NFT collection.
    /// </summary>
    /// <param name="collection">The address of the NFT collection.</param>
    /// <returns>The royalty parameters of the collection.</returns>
    public async Task<NftRoyaltyParams> GetRoyaltyParams(Address collection)
    {
        RunGetMethodResult runGetMethodResult = await client.RunGetMethod(collection, "royalty_params");
        if (runGetMethodResult.ExitCode != 0 && runGetMethodResult.ExitCode != 1) throw new Exception("Cannot retrieve nft collection royalty params.");
        Address royaltyAddress = ((Cell)runGetMethodResult.Stack[2]).Parse().LoadAddress()!;
        NftRoyaltyParams nftRoyaltyParams = new ()
        {
            Numerator = (BigInteger)runGetMethodResult.Stack[0],
            Denominator = (BigInteger)runGetMethodResult.Stack[1],
            RoyaltyAddress = royaltyAddress
        };
        return nftRoyaltyParams;
    }

    /// <summary>
    /// Retrieves the data of the specified NFT collection.
    /// </summary>
    /// <param name="collection">The address of the NFT collection.</param>
    /// <returns>The data of the collection.</returns>
    public async Task<NftCollectionData> GetCollectionData(Address collection)
    {
        RunGetMethodResult runGetMethodResult = await client.RunGetMethod(collection, "get_collection_data");
        if (runGetMethodResult.ExitCode != 0 && runGetMethodResult.ExitCode != 1) throw new Exception("Cannot retrieve nft collection data.");
        Address ownerAddress = ((Cell)runGetMethodResult.Stack[2]).Parse().LoadAddress()!;
        NftCollectionData nftCollectionData = new()
        {
            NextItemIndex = (uint)(BigInteger)runGetMethodResult.Stack[0],
            Data = (Cell)runGetMethodResult.Stack[1],
            OwnerAddress = ownerAddress
        };
        return nftCollectionData;
    }

    /// <summary>
    /// Retrieves the data of the specified NFT item.
    /// </summary>
    /// <param name="itemAddress">The address of the NFT item.</param>
    /// <returns>The data of the NFT item.</returns>
    public async Task<NftItemData> GetNftItemData(Address itemAddress)
    {
        RunGetMethodResult runGetMethodResult = await client.RunGetMethod(itemAddress, "get_nft_data");
        if (runGetMethodResult.ExitCode != 0 && runGetMethodResult.ExitCode != 1) throw new Exception("Cannot retrieve nft item data.");
        Address collection = ((Cell)runGetMethodResult.Stack[2]).Parse().LoadAddress()!;
        Address owner = ((Cell)runGetMethodResult.Stack[3]).Parse().LoadAddress()!;
        NftItemData nftItemData = new()
        {
            Init = (int)(BigInteger)runGetMethodResult.Stack[0] == -1,
            Index = (uint)(BigInteger)runGetMethodResult.Stack[1],
            CollectionAddress = collection,
            OwnerAddres = owner,
            Content = (Cell)runGetMethodResult.Stack[4]
        };
        return nftItemData;
    }
}
