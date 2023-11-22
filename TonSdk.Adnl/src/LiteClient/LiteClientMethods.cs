using System;
using System.Numerics;
using System.Text;
using TonSdk.Adnl.TL;
using TonSdk.Core.Crypto;

namespace TonSdk.Adnl.LiteClient
{
    internal class LiteClientMethods
    {
        internal static (byte[], byte[]) EncodeGetMasterchainInfo()
        {
            byte[] queryId = AdnlKeys.GenerateRandomBytes(32);
            TLWriteBuffer buffer = new TLWriteBuffer();
            buffer.WriteUInt32(
                BitConverter.ToUInt32(
                    Crc32.ComputeChecksum(
                        Encoding.UTF8.GetBytes("adnl.message.query query_id:int256 query:bytes = adnl.Message")),0));
            buffer.WriteInt256(queryId);
            
            buffer.WriteUInt8(12); // 1
            buffer.WriteUInt32(BitConverter.ToUInt32(
                Crc32.ComputeChecksum(
                    Encoding.UTF8.GetBytes("liteServer.query data:bytes = Object")),0)); // 4
                
            buffer.WriteUInt8(4); // 1
            buffer.WriteUInt32(
                BitConverter.ToUInt32(
                    Crc32.ComputeChecksum(
                        Encoding.UTF8.GetBytes("liteServer.getMasterchainInfo = liteServer.MasterchainInfo")),0)); // 4
            buffer.WriteUInt8(0);
            buffer.WriteUInt8(0);
            buffer.WriteUInt8(0);
                    
            buffer.WriteUInt8(0);
            buffer.WriteUInt8(0);
            buffer.WriteUInt8(0);
            return (queryId, buffer.Build());
        }

        internal static void DecodeGetMasterchainInfo(byte[] data)
        {
            TLReadBuffer buffer = new TLReadBuffer(data);

            buffer.ReadUInt8(); // size
            buffer.ReadUInt32Le(); // liteQuery
            
            // last:tonNode.blockIdExt
            int workchain = buffer.ReadInt32Le();
            long shard = buffer.ReadInt64Le();
            int seqno = buffer.ReadInt32Le();
            BigInteger rootHash = buffer.ReadInt256Le();
            BigInteger fileHash = buffer.ReadInt256Le();

            // state_root_hash:int256
            BigInteger stateRootHash = buffer.ReadInt256Le();
            
            // init:tonNode.zeroStateIdExt
            int workchainI = buffer.ReadInt32Le();
            BigInteger rootHashI = buffer.ReadInt256Le();
            BigInteger fileHashI = buffer.ReadInt256Le();
        }
    }
}