using System;
using System.Numerics;
using System.Text;
using TonSdk.Adnl.TL;
using TonSdk.Core.Crypto;

namespace TonSdk.Adnl.LiteClient
{
    internal class LiteClientEncoder
    {
        private static (byte[], byte[]) EncodeBase(TLWriteBuffer methodWriter)
        {
            byte[] queryId = AdnlKeys.GenerateRandomBytes(32);
            TLWriteBuffer writer = new TLWriteBuffer();
            
            TLWriteBuffer liteQueryWriter = new TLWriteBuffer();
            liteQueryWriter.WriteUInt32(
                BitConverter.ToUInt32(
                    Crc32.ComputeChecksum(
                        Encoding.UTF8.GetBytes("liteServer.query data:bytes = Object")),0));
            liteQueryWriter.WriteBuffer(methodWriter.Build());
            
            writer.WriteUInt32(
                BitConverter.ToUInt32(
                    Crc32.ComputeChecksum(
                        Encoding.UTF8.GetBytes("adnl.message.query query_id:int256 query:bytes = adnl.Message")),0));
            writer.WriteBytes(queryId);
            writer.WriteBuffer(liteQueryWriter.Build());

            return (queryId, writer.Build());
        }
        
        internal static (byte[], byte[]) EncodeGetMasterchainInfo()
        {
            TLWriteBuffer writer = new TLWriteBuffer();
            writer.WriteUInt32(
                BitConverter.ToUInt32(
                    Crc32.ComputeChecksum(
                        Encoding.UTF8.GetBytes("liteServer.getMasterchainInfo = liteServer.MasterchainInfo")),0));

            return EncodeBase(writer);
        }
        
        internal static (byte[], byte[]) EncodeGetMasterchainInfoExt()
        {
            TLWriteBuffer writer = new TLWriteBuffer();
            writer.WriteUInt32(
                BitConverter.ToUInt32(
                    Crc32.ComputeChecksum(
                        Encoding.UTF8.GetBytes("liteServer.getMasterchainInfoExt mode:# = liteServer.MasterchainInfoExt")),0));
            writer.WriteUInt32(0);
            return EncodeBase(writer);
        }

        internal static (byte[], byte[]) EncodeGetTime()
        {
            TLWriteBuffer writer = new TLWriteBuffer();
            writer.WriteUInt32(
                BitConverter.ToUInt32(
                    Crc32.ComputeChecksum(
                        Encoding.UTF8.GetBytes("liteServer.getTime = liteServer.CurrentTime")),0));
            return EncodeBase(writer);
        }

        internal static (byte[], byte[]) EncodeGetVersion()
        {
            TLWriteBuffer writer = new TLWriteBuffer();
            writer.WriteUInt32(
                BitConverter.ToUInt32(
                    Crc32.ComputeChecksum(
                        Encoding.UTF8.GetBytes("liteServer.getVersion = liteServer.Version")),0));
            return EncodeBase(writer);
        }

        internal static (byte[], byte[]) EncodeGetBlock(BlockIdExternal block, string functionName)
        {
            TLWriteBuffer writer = new TLWriteBuffer();
            writer.WriteUInt32(BitConverter.ToUInt32(Crc32.ComputeChecksum(Encoding.UTF8.GetBytes(functionName)),0));
            writer.WriteInt32(block.Workchain);
            writer.WriteInt64(block.Shard);
            writer.WriteInt32(block.Seqno);
            writer.WriteInt256(block.RootHash);
            writer.WriteInt256(block.FileHash);
            return EncodeBase(writer);
        }

        internal static (byte[], byte[]) EncodeGetBlockHeader(BlockIdExternal block)
        {
            TLWriteBuffer writer = new TLWriteBuffer();
            writer.WriteUInt32(BitConverter.ToUInt32(Crc32.ComputeChecksum(
                Encoding.UTF8.GetBytes("liteServer.getBlockHeader id:tonNode.blockIdExt mode:# = liteServer.BlockHeader")),0));
            
            writer.WriteInt32(block.Workchain); 
            writer.WriteInt64(block.Shard);
            writer.WriteInt32(block.Seqno);
            writer.WriteInt256(block.RootHash);
            writer.WriteInt256(block.FileHash);
            
            writer.WriteUInt32(1);
            return EncodeBase(writer);
        }

        internal static (byte[], byte[]) EncodeSendMessage(byte[] body)
        {
            TLWriteBuffer writer = new TLWriteBuffer();
            writer.WriteUInt32(BitConverter.ToUInt32(Crc32.ComputeChecksum(
                Encoding.UTF8.GetBytes("liteServer.sendMessage body:bytes = liteServer.SendMsgStatus")),0));
            writer.WriteBuffer(body);
            return EncodeBase(writer);
        }
    }
}