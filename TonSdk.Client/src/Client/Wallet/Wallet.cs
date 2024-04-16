
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
        /// <returns>The sequence number of the address, or null if the retrieval failed or the sequence number is not available.</returns>
        public async Task<uint?> GetSeqno(Address address)
        {
            var result = await client.RunGetMethod(address, "seqno", Array.Empty<IStackItem>());
            
            if(result == null) return null;
            if (result.Value.ExitCode != 0 && result.Value.ExitCode != 1) return null;

            uint seqno = 0;
            if (client.GetClientType() == TonClientType.HTTP_TONCENTERAPIV2)
                seqno = (uint)(BigInteger)result.Value.Stack[0];
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
        /// <returns>The subwallet ID of the address, or null if the retrieval failed or the subwallet ID is not available.</returns>
        public async Task<uint?> GetSubwalletId(Address address)
        {
            var result = await client.RunGetMethod(address, "get_subwallet_id", Array.Empty<IStackItem>());
            
            if(result == null) return null;
            if (result.Value.ExitCode != 0 && result.Value.ExitCode != 1) return null;
            
            uint id = 0;
            if (client.GetClientType() == TonClientType.HTTP_TONCENTERAPIV2)
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
        /// <returns>
        /// The list of plugins associated with the address, or null if the retrieval failed or the list is not available.
        /// </returns>
        public async Task<object[]> GetPluginList(Address address)
        {
            var result = await client.RunGetMethod(address, "get_plugin_list", Array.Empty<IStackItem>());
            if(result == null) return null;
            if (result.Value.ExitCode != 0 && result.Value.ExitCode != 1) return null;
            return client.GetClientType() == TonClientType.HTTP_TONCENTERAPIV2 ? result.Value.Stack : result.Value.StackItems;
        }
    }
}
