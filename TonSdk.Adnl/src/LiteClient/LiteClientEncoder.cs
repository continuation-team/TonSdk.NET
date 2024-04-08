using System;
using System.Numerics;
using System.Text;
using System.Transactions;
using TonSdk.Adnl.TL;
using TonSdk.Core;
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
            writer.WriteInt256(new BigInteger(queryId));
            writer.WriteBuffer(liteQueryWriter.Build());

            return (queryId, writer.Build());
        }

        internal static byte[] EncodePingPong()
        {
            TLWriteBuffer writer = new TLWriteBuffer();
            writer.WriteUInt32(
                BitConverter.ToUInt32(
                    Crc32.ComputeChecksum(
                        Encoding.UTF8.GetBytes("tcp.ping random_id:long = tcp.Pong")),0));
            
            var random = new Random();
            int firstPart = random.Next();
            int secondPart = random.Next();
            long randomInt64 = ((long)firstPart << 32) | (uint)secondPart;
            writer.WriteInt64(randomInt64);
            return writer.Build();
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

        internal static (byte[], byte[]) EncodeGetBlock(BlockIdExtended block, string functionName)
        {
            TLWriteBuffer writer = new TLWriteBuffer();
            writer.WriteUInt32(BitConverter.ToUInt32(Crc32.ComputeChecksum(Encoding.UTF8.GetBytes(functionName)),0));
            writer.WriteInt32(block.Workchain);
            writer.WriteInt64(block.Shard);
            writer.WriteInt32(block.Seqno);
            writer.WriteBytes(block.RootHash, 32);
            writer.WriteBytes(block.FileHash, 32);
            return EncodeBase(writer);
        }

        internal static (byte[], byte[]) EncodeGetBlockHeader(BlockIdExtended block)
        {
            TLWriteBuffer writer = new TLWriteBuffer();
            writer.WriteUInt32(BitConverter.ToUInt32(Crc32.ComputeChecksum(
                Encoding.UTF8.GetBytes("liteServer.getBlockHeader id:tonNode.blockIdExt mode:# = liteServer.BlockHeader")),0));
            
            writer.WriteInt32(block.Workchain);
            
            writer.WriteInt64(block.Shard);
            writer.WriteInt32(block.Seqno);
            writer.WriteBytes(block.RootHash, 32);
            writer.WriteBytes(block.FileHash, 32);
            
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

        internal static (byte[], byte[]) EncodeGetAccountState(BlockIdExtended block, Address account, string query)
        {
            TLWriteBuffer writer = new TLWriteBuffer();
            writer.WriteUInt32(BitConverter.ToUInt32(Crc32.ComputeChecksum(
                Encoding.UTF8.GetBytes(query)),0));
            
            writer.WriteInt32(block.Workchain); 
            writer.WriteInt64(block.Shard);
            writer.WriteInt32(block.Seqno);
            writer.WriteBytes(block.RootHash, 32);
            writer.WriteBytes(block.FileHash, 32);
            
            writer.WriteInt32(account.GetWorkchain());
            writer.WriteBytes(account.GetHash(), 32);
            return EncodeBase(writer);
        }

        internal static (byte[], byte[]) EncodeRunSmcMethod(BlockIdExtended block, Address account, long methodId,
            byte[] stack, uint mode)
        {
            TLWriteBuffer writer = new TLWriteBuffer();
            writer.WriteUInt32(BitConverter.ToUInt32(Crc32.ComputeChecksum(
                Encoding.UTF8.GetBytes("liteServer.runSmcMethod mode:# id:tonNode.blockIdExt account:liteServer.accountId method_id:long params:bytes = liteServer.RunMethodResult")),0));
            writer.WriteUInt32(mode);
            
            writer.WriteInt32(block.Workchain); 
            writer.WriteInt64(block.Shard);
            writer.WriteInt32(block.Seqno);
            writer.WriteBytes(block.RootHash, 32);
            writer.WriteBytes(block.FileHash, 32);
            
            writer.WriteInt32(account.GetWorkchain());
            writer.WriteBytes(account.GetHash(), 32);
            
            writer.WriteInt64(methodId);
            writer.WriteBuffer(stack);
            
            return EncodeBase(writer);
        }

        internal static (byte[], byte[]) EncodeGetShardInfo(BlockIdExtended block, int workchain, long shard,
            bool exact = false)
        {
            TLWriteBuffer writer = new TLWriteBuffer();
            writer.WriteUInt32(BitConverter.ToUInt32(Crc32.ComputeChecksum(
                Encoding.UTF8.GetBytes("liteServer.getShardInfo id:tonNode.blockIdExt workchain:int shard:long exact:Bool = liteServer.ShardInfo")),0));
            
            writer.WriteInt32(block.Workchain); 
            writer.WriteInt64(block.Shard);
            writer.WriteInt32(block.Seqno);
            writer.WriteBytes(block.RootHash, 32);
            writer.WriteBytes(block.FileHash, 32);
            
            writer.WriteInt32(workchain);
            writer.WriteInt64(shard);
            writer.WriteBool(exact);
            return EncodeBase(writer);
        }

        internal static (byte[], byte[]) EncodeGetAllShardsInfo(BlockIdExtended block)
        {
            TLWriteBuffer writer = new TLWriteBuffer();
            writer.WriteUInt32(BitConverter.ToUInt32(Crc32.ComputeChecksum(
                Encoding.UTF8.GetBytes("liteServer.getAllShardsInfo id:tonNode.blockIdExt = liteServer.AllShardsInfo")),0));
            
            writer.WriteInt32(block.Workchain); 
            writer.WriteInt64(block.Shard);
            writer.WriteInt32(block.Seqno);
            writer.WriteBytes(block.RootHash, 32);
            writer.WriteBytes(block.FileHash, 32);
            
            return EncodeBase(writer);
        }

        internal static (byte[], byte[]) EncodeGetTransactions(uint count, Address account, long lt, byte[] hash)
        {
            TLWriteBuffer writer = new TLWriteBuffer();
            writer.WriteUInt32(BitConverter.ToUInt32(Crc32.ComputeChecksum(
                Encoding.UTF8.GetBytes("liteServer.getTransactions count:# account:liteServer.accountId lt:long hash:int256 = liteServer.TransactionList")),0));
            writer.WriteInt32((int)count);
            
            writer.WriteInt32(account.GetWorkchain());
            writer.WriteBytes(account.GetHash(), 32);
            
            writer.WriteInt64(lt);
            writer.WriteBytes(hash, 32);
            
            return EncodeBase(writer);
        }

        internal static (byte[], byte[]) EncodeLookUpBlock(int workchain, long shard, long? seqno, ulong? lt, ulong? uTime)
        {
            TLWriteBuffer writer = new TLWriteBuffer();
            writer.WriteUInt32(BitConverter.ToUInt32(Crc32.ComputeChecksum(
                Encoding.UTF8.GetBytes("liteServer.lookupBlock mode:# id:tonNode.blockId lt:mode.1?long utime:mode.2?int = liteServer.BlockHeader")),0));
            
            uint mode = 0;
            if(seqno != null) mode |= 1;
            if (lt != null) mode |= 2;
            if (uTime != null) mode |= 4;
            
            writer.WriteUInt32(mode);
            
            writer.WriteInt32(workchain); 
            writer.WriteInt64(shard);
            
            if (seqno != null) writer.WriteUInt32((uint)seqno);
            else writer.WriteUInt32(0);
            if (lt != null) writer.WriteInt64((long)lt!.Value);
            if (uTime != null) writer.WriteInt32((int)uTime!.Value);
            
            return EncodeBase(writer);
        }

        internal static (byte[], byte[]) EncodeListBlockTransactions(BlockIdExtended block, uint count, ITransactionId? after, bool? reverseOrder, bool? wantProof, string query)
        {
            TLWriteBuffer writer = new TLWriteBuffer();
            writer.WriteUInt32(BitConverter.ToUInt32(Crc32.ComputeChecksum(
                Encoding.UTF8.GetBytes(query)),0));
            
            writer.WriteInt32(block.Workchain); 
            writer.WriteInt64(block.Shard);
            writer.WriteInt32(block.Seqno);
            writer.WriteBytes(block.RootHash, 32);
            writer.WriteBytes(block.FileHash, 32);
            
            if(after == null)
            {
                writer.WriteUInt32(7);
                writer.WriteUInt32(count);
            }
            else
            {
                writer.WriteUInt32(7 + 128);
                writer.WriteUInt32(count);
                switch (after)
                {
                    case TransactionId id:
                        writer.WriteBytes(id.Hash, 32);
                        writer.WriteInt64(id.Lt);
                        break;
                    case TransactionId3 id3:
                        writer.WriteBytes(id3.Account.GetHash(), 32);
                        writer.WriteInt64(id3.Lt);
                        break;
                }
            }
            return EncodeBase(writer);
        }
        
        internal static (byte[], byte[]) EncodeGetBlockProof(BlockIdExtended knownBlock, BlockIdExtended targetBlock)
        {
            TLWriteBuffer writer = new TLWriteBuffer();
            writer.WriteUInt32(BitConverter.ToUInt32(Crc32.ComputeChecksum(
                Encoding.UTF8.GetBytes("liteServer.getBlockProof mode:# known_block:tonNode.blockIdExt target_block:mode.0?tonNode.blockIdExt = liteServer.PartialBlockProof")),0));
            
            uint mode = 0;
            if (targetBlock != null) mode |= 1u << 0;
            writer.WriteUInt32(mode);
            
            writer.WriteInt32(knownBlock.Workchain); 
            writer.WriteInt64(knownBlock.Shard);
            writer.WriteInt32(knownBlock.Seqno);
            writer.WriteBytes(knownBlock.RootHash, 32);
            writer.WriteBytes(knownBlock.FileHash, 32);

            if ((mode & (1 << 0)) == 0) return EncodeBase(writer);
            
            writer.WriteInt32(targetBlock!.Workchain); 
            writer.WriteInt64(targetBlock.Shard);
            writer.WriteInt32(targetBlock.Seqno);
            writer.WriteBytes(targetBlock.RootHash, 32);
            writer.WriteBytes(targetBlock.FileHash, 32);
            return EncodeBase(writer);
        }

        internal static (byte[], byte[]) EncodeGetConfigAll(BlockIdExtended block)
        {
            TLWriteBuffer writer = new TLWriteBuffer();
            writer.WriteUInt32(BitConverter.ToUInt32(Crc32.ComputeChecksum(
                Encoding.UTF8.GetBytes("liteServer.getConfigAll mode:# id:tonNode.blockIdExt = liteServer.ConfigInfo")),0));
            
            writer.WriteUInt32(1);
            writer.WriteInt32(block.Workchain); 
            writer.WriteInt64(block.Shard);
            writer.WriteInt32(block.Seqno);
            writer.WriteBytes(block.RootHash, 32);
            writer.WriteBytes(block.FileHash, 32);
            return EncodeBase(writer);
        }

        internal static (byte[], byte[]) EncodeGetConfigParams(BlockIdExtended block, int[] ids)
        {
            TLWriteBuffer writer = new TLWriteBuffer();
            writer.WriteUInt32(705764377);
            writer.WriteUInt32(1);
            
            writer.WriteInt32(block.Workchain); 
            writer.WriteInt64(block.Shard);
            writer.WriteInt32(block.Seqno);
            writer.WriteBytes(block.RootHash, 32);
            writer.WriteBytes(block.FileHash, 32);
            
            writer.WriteInt32(ids.Length);
            foreach (var item in ids)
            {
                writer.WriteInt32(item);
            }

            return EncodeBase(writer);
        }

        internal static (byte[], byte[]) EncodeGetLibraries(BigInteger[] list)
        {
            TLWriteBuffer writer = new TLWriteBuffer();
            writer.WriteUInt32(BitConverter.ToUInt32(Crc32.ComputeChecksum(
                Encoding.UTF8.GetBytes("liteServer.getLibraries library_list:(vector int256) = liteServer.LibraryResult")),0));

            writer.WriteUInt32((uint)list.Length);
            foreach (var t in list)
            {
                writer.WriteInt256(t);
            }
            return EncodeBase(writer);
        }

        internal static (byte[], byte[]) EncodeGetShardBlockProof(BlockIdExtended block)
        {
            TLWriteBuffer writer = new TLWriteBuffer();
            writer.WriteUInt32(BitConverter.ToUInt32(Crc32.ComputeChecksum(
                Encoding.UTF8.GetBytes("liteServer.getShardBlockProof id:tonNode.blockIdExt = liteServer.ShardBlockProof")),0));
            
            writer.WriteInt32(block.Workchain); 
            writer.WriteInt64(block.Shard);
            writer.WriteInt32(block.Seqno);
            writer.WriteBytes(block.RootHash, 32);
            writer.WriteBytes(block.FileHash, 32);
            return EncodeBase(writer);
        }
    }
}