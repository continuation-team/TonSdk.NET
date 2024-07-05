
using System;
using Newtonsoft.Json;
using System.Numerics;
using System.Threading.Tasks;
using TonSdk.Client.Stack;
using TonSdk.Core;

namespace TonSdk.Client
{
    public class Wallet
    {
        private readonly TonClient client;

        public Wallet(TonClient client)
        {
            this.client = client;
        }

        /// <summary>
        /// Retrieves the sequence number (seqno) of the specified address.
        /// </summary>
        /// <param name="address">The address for which to retrieve the sequence number.</param>
        /// <param name="block">Can be provided to fetch in specific block, requires LiteClient (optional).</param>
        /// <returns>The sequence number of the address, or null if the retrieval failed or the sequence number is not available.</returns>
        public async Task<uint?> GetSeqno(Address address, BlockIdExtended? block = null)
        {
            var result = await client.RunGetMethod(address, "seqno", Array.Empty<IStackItem>(), block);
            
            if(result == null) return null;
            if (result.Value.ExitCode != 0 && result.Value.ExitCode != 1) return null;

            uint seqno = 0;
            if (client.GetClientType() == TonClientType.HTTP_TONCENTERAPIV2 || client.GetClientType() == TonClientType.HTTP_TONWHALESAPI || client.GetClientType() == TonClientType.HTTP_TONCENTERAPIV3)
                seqno = uint.Parse(result.Value.Stack[0].ToString());
            else
            {
                if (result.Value.StackItems[0] is VmStackInt)
                    seqno = (uint)((VmStackInt)result.Value.StackItems[0]).Value;
                else if (result.Value.StackItems[0] is VmStackTinyInt)
                    seqno = (uint)((VmStackTinyInt)result.Value.StackItems[0]).Value;
            }
            return seqno;
        }

        /// <summary>
        /// Retrieves the subwallet ID of the specified address.
        /// </summary>
        /// <param name="address">The address for which to retrieve the subwallet ID.</param>
        /// <param name="block">Can be provided to fetch in specific block, requires LiteClient (optional).</param>
        /// <returns>The subwallet ID of the address, or null if the retrieval failed or the subwallet ID is not available.</returns>
        public async Task<uint?> GetSubwalletId(Address address, BlockIdExtended? block = null)
        {
            var result = await client.RunGetMethod(address, "get_subwallet_id", Array.Empty<IStackItem>(), block);
            
            if(result == null) return null;
            if (result.Value.ExitCode != 0 && result.Value.ExitCode != 1) return null;
            
            uint id = 0;
            if (client.GetClientType() == TonClientType.HTTP_TONCENTERAPIV2 || client.GetClientType() == TonClientType.HTTP_TONWHALESAPI|| client.GetClientType() == TonClientType.HTTP_TONCENTERAPIV3)
                id = (uint)(BigInteger)result.Value.Stack[0];
            else
            {
                if (result.Value.StackItems[0] is VmStackInt)
                    id = (uint)((VmStackInt)result.Value.StackItems[0]).Value;
                else if (result.Value.StackItems[0] is VmStackTinyInt)
                    id = (uint)((VmStackTinyInt)result.Value.StackItems[0]).Value;
            }
            return id;
        }

        /// <summary>
        /// Retrieves the list of plugins associated with the specified address.
        /// </summary>
        /// <param name="address">The address for which to retrieve the plugin list.</param>
        /// <param name="block">Can be provided to fetch in specific block, requires LiteClient (optional).</param>
        /// <returns>
        /// The list of plugins associated with the address, or null if the retrieval failed or the list is not available.
        /// </returns>
        public async Task<object[]> GetPluginList(Address address, BlockIdExtended? block = null)
        {
            var result = await client.RunGetMethod(address, "get_plugin_list", Array.Empty<IStackItem>(), block);
            if(result == null) return null;
            if (result.Value.ExitCode != 0 && result.Value.ExitCode != 1) return null;
            return client.GetClientType() == TonClientType.HTTP_TONCENTERAPIV2 || client.GetClientType() == TonClientType.HTTP_TONWHALESAPI || client.GetClientType() == TonClientType.HTTP_TONCENTERAPIV3
                ? result.Value.Stack : result.Value.StackItems;
        }
        
        /// <summary>
        /// Retrieves the public key ow the wallet with the specified address.
        /// </summary>
        /// <param name="address">The address for which to retrieve the public key.</param>
        /// <param name="block">Can be provided to fetch in specific block, requires LiteClient (optional).</param>
        /// <returns>
        /// The public key associated with the address, or empty byte[] if the retrieval failed or wrong contract.
        /// </returns>
        public async Task<byte[]> GetPublicKey(Address address, BlockIdExtended? block = null)
        {
            var result = await client.RunGetMethod(address, "get_public_key", Array.Empty<IStackItem>(), block);
            if(result == null) return null;
            if (result.Value.ExitCode != 0 && result.Value.ExitCode != 1) return null;
            byte[] publicKey = Array.Empty<byte>();

            if (client.GetClientType() == TonClientType.HTTP_TONCENTERAPIV2 ||
                client.GetClientType() == TonClientType.HTTP_TONWHALESAPI ||
                client.GetClientType() == TonClientType.HTTP_TONCENTERAPIV3)
            {
                byte[] key = ((BigInteger)result.Value.Stack[0]).ToByteArray();
                Array.Reverse(key);
                publicKey = new byte[key.Length - 1];
                Array.Copy(key, 1, publicKey, 0, key.Length - 1);
                return publicKey;
            }
            else
            {
                if (!(result.Value.StackItems[0] is VmStackInt))
                    return publicKey;
                
                byte[] key = ((VmStackInt)result.Value.StackItems[0]).Value.ToByteArray();
                Array.Reverse(key);
                publicKey = new byte[key.Length - 1];
                Array.Copy(key, 1, publicKey, 0, key.Length - 1);
                return publicKey;
            }
        }
    }
}
