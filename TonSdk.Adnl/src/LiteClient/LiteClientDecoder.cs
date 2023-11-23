using System;
using System.Numerics;
using TonSdk.Adnl.TL;
using TonSdk.Core.Crypto;

namespace TonSdk.Adnl.LiteClient
{
    public class LiteClientDecoder
    {
        internal static MasterChainInfo DecodeGetMasterchainInfo(byte[] data)
        {
            TLReadBuffer buffer = new TLReadBuffer(data);
            
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
            
            BlockIdExternal lastBlock = new BlockIdExternal(workchain, rootHash, fileHash, shard, seqno);
            BlockIdExternal initBlock = new BlockIdExternal(workchainI, rootHashI, fileHashI, 0,0);

            return new MasterChainInfo(lastBlock, initBlock, stateRootHash);
        }
        
        internal static MasterChainInfoExternal DecodeGetMasterchainInfoExternal(byte[] data)
        {
            TLReadBuffer buffer = new TLReadBuffer(data);
            
            // mode:#
            buffer.ReadUInt32Le();
            
            // version:int
            int version = buffer.ReadInt32Le();
            
            // capabilities:long
            long capabilities = buffer.ReadInt64Le();
            
            // last:tonNode.blockIdExt
            int workchain = buffer.ReadInt32Le();
            long shard = buffer.ReadInt64Le();
            int seqno = buffer.ReadInt32Le();
            BigInteger rootHash = buffer.ReadInt256Le();
            BigInteger fileHash = buffer.ReadInt256Le();
            
            // last_uTime:int
            int lastUTime = buffer.ReadInt32Le();
            
            // now:int
            int time = buffer.ReadInt32Le();
            
            // state_root_hash:int256
            BigInteger stateRootHash = buffer.ReadInt256Le();
            
            // init:tonNode.zeroStateIdExt
            int workchainI = buffer.ReadInt32Le();
            BigInteger rootHashI = buffer.ReadInt256Le();
            BigInteger fileHashI = buffer.ReadInt256Le();
            
            BlockIdExternal lastBlock = new BlockIdExternal(workchain, rootHash, fileHash, shard, seqno);
            BlockIdExternal initBlock = new BlockIdExternal(workchainI, rootHashI, fileHashI, 0,0);

            return new MasterChainInfoExternal(version, capabilities, lastUTime, time, lastBlock, initBlock,
                stateRootHash);
        }
        
        internal static int DecodeGetTime(byte[] data)
        {
            TLReadBuffer buffer = new TLReadBuffer(data);
            
            // now:int
            int time = buffer.ReadInt32Le();
            return time;
        }

        internal static ChainVersion DecodeGetVersion(byte[] data)
        {
            TLReadBuffer buffer = new TLReadBuffer(data);
            
            // mode:#
            buffer.ReadUInt32Le();
            
            // version:int
            int version = buffer.ReadInt32Le();
            
            // capabilities:long
            long capabilities = buffer.ReadInt64Le();
            
            // now:int
            int time = buffer.ReadInt32Le();

            return new ChainVersion(version, capabilities, time);
        }

        internal static void DecodeGetBlock(byte[] data)
        {
            TLReadBuffer buffer = new TLReadBuffer(data);
            
            // id:tonNode.blockIdExt
            int workchain = buffer.ReadInt32Le();
            long shard = buffer.ReadInt64Le();
            int seqno = buffer.ReadInt32Le();
            BigInteger rootHash = buffer.ReadInt256Le();
            BigInteger fileHash = buffer.ReadInt256Le();
            
            Console.WriteLine("workchain: " + workchain);
            Console.WriteLine("shard: " + shard);
            Console.WriteLine("seqno: " + seqno);
            Console.WriteLine("rootHash: " + rootHash);
            Console.WriteLine("fileHash: " + fileHash);
            Console.WriteLine();
            
            BlockIdExternal blockId = new BlockIdExternal(workchain, rootHash, fileHash, shard, seqno);
            
           
        }
        
        internal static void DecodeGetBlockState(byte[] data)
        {
            TLReadBuffer buffer = new TLReadBuffer(data);
            
            // id:tonNode.blockIdExt
            // int workchain = buffer.ReadInt32Le();
            // long shard = buffer.ReadInt64Le();
            // int seqno = buffer.ReadInt32Le();
            // BigInteger rootHash = buffer.ReadInt256Le();
            
           
        }

        internal static void DecodeSendMessage(byte[] data)
        {
            TLReadBuffer buffer = new TLReadBuffer(data);
            int status = buffer.ReadInt32Le();
            Console.WriteLine(status);
        }
    }
}