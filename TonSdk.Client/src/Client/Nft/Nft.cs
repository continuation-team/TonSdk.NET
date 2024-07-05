using TonSdk.Core.Boc;
using TonSdk.Core;
using System.Numerics;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using TonSdk.Client.Stack;

namespace TonSdk.Client
{

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
        public BigInteger Index;
        public Address CollectionAddress;
        public Address OwnerAddress;
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
        /// <param name="block">Can be provided to fetch in specific block, requires LiteClient (optional).</param>
        /// <returns>The address of the item.</returns>
        public async Task<Address> GetItemAddress(Address collection, uint index, BlockIdExtended? block = null)
        {
            var stackItems = new List<IStackItem>()
            {
                new VmStackInt()
                {
                    Value = index
                }
            };
            var result = await client.RunGetMethod(collection, "get_nft_address_by_index", stackItems.ToArray(), block);
            
            if ( result == null || result.Value.ExitCode != 0 && result.Value.ExitCode != 1) throw new Exception("Cannot retrieve nft address.");
            return client.GetClientType() == TonClientType.HTTP_TONCENTERAPIV2 || client.GetClientType() == TonClientType.HTTP_TONWHALESAPI|| client.GetClientType() == TonClientType.HTTP_TONCENTERAPIV3
                ? ((Cell)result.Value.Stack[0]).Parse().LoadAddress()! 
                : ((VmStackSlice)result.Value.StackItems[0]).Value.LoadAddress();
        }

        /// <summary>
        /// Retrieves the royalty parameters of the specified NFT collection.
        /// </summary>
        /// <param name="collection">The address of the NFT collection.</param>
        /// <param name="block">Can be provided to fetch in specific block, requires LiteClient (optional).</param>
        /// <returns>The royalty parameters of the collection.</returns>
        public async Task<NftRoyaltyParams> GetRoyaltyParams(Address collection, BlockIdExtended? block = null)
        {
            RunGetMethodResult? result = await client.RunGetMethod(collection, "royalty_params", Array.Empty<IStackItem>(), block);
            
            if(result == null) throw new Exception("Cannot retrieve nft collection royalty params.");
            if (result.Value.ExitCode != 0 && result.Value.ExitCode != 1) throw new Exception("Cannot retrieve nft collection royalty params.");
            
            Address royaltyAddress;
            BigInteger numerator = BigInteger.Zero, denominator = BigInteger.Zero;
            if (client.GetClientType() == TonClientType.HTTP_TONCENTERAPIV2 || client.GetClientType() == TonClientType.HTTP_TONWHALESAPI|| client.GetClientType() == TonClientType.HTTP_TONCENTERAPIV3)
            {
                royaltyAddress = ((Cell)result.Value.Stack[2]).Parse().LoadAddress()!;
                numerator = (BigInteger)result.Value.Stack[0];
                denominator = (BigInteger)result.Value.Stack[1];
            }
            else
            {
                if (result.Value.StackItems[0] is VmStackInt)
                    numerator = ((VmStackInt)result.Value.StackItems[0]).Value;
                else if (result.Value.StackItems[0] is VmStackTinyInt)
                    numerator = ((VmStackTinyInt)result.Value.StackItems[0]).Value;
                
                if (result.Value.StackItems[1] is VmStackInt)
                    denominator = ((VmStackInt)result.Value.StackItems[1]).Value;
                else if (result.Value.StackItems[1] is VmStackTinyInt)
                    denominator = ((VmStackTinyInt)result.Value.StackItems[1]).Value;
                try
                {
                    royaltyAddress = ((VmStackSlice)result.Value.StackItems[2]).Value.LoadAddress();
                }
                catch
                {
                    royaltyAddress = null;
                }
            }
            
            NftRoyaltyParams nftRoyaltyParams = new NftRoyaltyParams
            {
                Numerator = numerator,
                Denominator = denominator,
                RoyaltyAddress = royaltyAddress
            };
            return nftRoyaltyParams;
        }

        /// <summary>
        /// Retrieves the data of the specified NFT collection.
        /// </summary>
        /// <param name="collection">The address of the NFT collection.</param>
        /// <param name="block">Can be provided to fetch in specific block, requires LiteClient (optional).</param>
        /// <returns>The data of the collection.</returns>
        public async Task<NftCollectionData> GetCollectionData(Address collection, BlockIdExtended? block = null)
        {
            RunGetMethodResult? result = await client.RunGetMethod(collection, "get_collection_data", Array.Empty<IStackItem>(), block);
            
            if(result == null) throw new Exception("Cannot retrieve nft collection data.");
            if (result.Value.ExitCode != 0 && result.Value.ExitCode != 1) throw new Exception("Cannot retrieve nft collection data.");
            
            Address ownerAddress;
            uint nextItemIndex = 0;
            Cell data;
            if (client.GetClientType() == TonClientType.HTTP_TONCENTERAPIV2 || client.GetClientType() == TonClientType.HTTP_TONWHALESAPI|| client.GetClientType() == TonClientType.HTTP_TONCENTERAPIV3)
            {
                ownerAddress = ((Cell)result.Value.Stack[2]).Parse().LoadAddress()!;
                nextItemIndex = (uint)(BigInteger)result.Value.Stack[0];
                data = (Cell)result.Value.Stack[1];
            }
            else
            {
                if (result.Value.StackItems[0] is VmStackInt)
                    nextItemIndex = (uint)((VmStackInt)result.Value.StackItems[0]).Value;
                else if (result.Value.StackItems[0] is VmStackTinyInt)
                    nextItemIndex = (uint)((VmStackTinyInt)result.Value.StackItems[0]).Value;

                data = ((VmStackCell)result.Value.StackItems[1]).Value;
                try
                {
                    ownerAddress = ((VmStackSlice)result.Value.StackItems[2]).Value.LoadAddress();
                }
                catch
                {
                    ownerAddress = null;
                }
            }
            
            var nftCollectionData = new NftCollectionData()
            {
                NextItemIndex = nextItemIndex,
                Data = data,
                OwnerAddress = ownerAddress
            };
            return nftCollectionData;
        }

        /// <summary>
        /// Retrieves the data of the specified NFT item.
        /// </summary>
        /// <param name="itemAddress">The address of the NFT item.</param>
        /// <param name="block">Can be provided to fetch in specific block, requires LiteClient (optional).</param>
        /// <returns>The data of the NFT item.</returns>
        public async Task<NftItemData> GetNftItemData(Address itemAddress, BlockIdExtended? block = null)
        {
            RunGetMethodResult? result = await client.RunGetMethod(itemAddress, "get_nft_data", Array.Empty<IStackItem>(), block);
            
            if(result == null) throw new Exception("Cannot retrieve nft item data.");
            if (result.Value.ExitCode != 0 && result.Value.ExitCode != 1) throw new Exception("Cannot retrieve nft item data.");

            Address collection;
            Address owner;
            bool init = false;
            BigInteger index = BigInteger.Zero;
            Cell content; 
            if (client.GetClientType() == TonClientType.HTTP_TONCENTERAPIV2 || client.GetClientType() == TonClientType.HTTP_TONWHALESAPI|| client.GetClientType() == TonClientType.HTTP_TONCENTERAPIV3)
            {
                collection = ((Cell)result.Value.Stack[2]).Parse().LoadAddress()!;
                owner = ((Cell)result.Value.Stack[3]).Parse().LoadAddress()!;
                init = (int)(BigInteger)result.Value.Stack[0] == -1;
                index = (BigInteger)result.Value.Stack[1];
                content = (Cell)result.Value.Stack[4];
            }
            else
            {
                if (result.Value.StackItems[0] is VmStackInt)
                    init = (int)((VmStackInt)result.Value.StackItems[0]).Value == -1;
                else if (result.Value.StackItems[0] is VmStackTinyInt)
                    init = (int)((VmStackTinyInt)result.Value.StackItems[0]).Value == -1;
                
                if (result.Value.StackItems[1] is VmStackInt)
                    index = ((VmStackInt)result.Value.StackItems[1]).Value;
                else if (result.Value.StackItems[1] is VmStackTinyInt)
                    index = ((VmStackTinyInt)result.Value.StackItems[1]).Value;
                
                content = ((VmStackCell)result.Value.StackItems[4]).Value;
                
                try
                {
                    collection = ((VmStackSlice)result.Value.StackItems[2]).Value.LoadAddress();
                }
                catch
                {
                    collection = null;
                }
                
                try
                {
                    owner = ((VmStackSlice)result.Value.StackItems[3]).Value.LoadAddress();
                }
                catch
                {
                    owner = null;
                }
            }
            
            NftItemData nftItemData = new NftItemData()
            {
                Init = init,
                Index = index,
                CollectionAddress = collection,
                OwnerAddress = owner,
                Content = content
            };
            return nftItemData;
        }
    }
}
