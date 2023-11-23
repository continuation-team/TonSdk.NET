using System.Numerics;

namespace TonSdk.Adnl.LiteClient
{
    public class BlockIdExternal
    {
        public int Workchain { get; set; }
        public long Shard { get; set; }
        public int Seqno { get; set; }
        public BigInteger RootHash { get; set; }
        public BigInteger FileHash { get; set; }

        public BlockIdExternal(int workchain, BigInteger rootHash, BigInteger fileHash, long shard, int seqno)
        {
            Workchain = workchain;
            RootHash = rootHash;
            FileHash = fileHash;
            Shard = shard;
            Seqno = seqno;
        }
    }

    public class MasterChainInfo
    {
        public BlockIdExternal LastBlockId { get; set; }
        public BlockIdExternal InitBlockId { get; set; }
        public BigInteger StateRootHash { get; set; }

        public MasterChainInfo(BlockIdExternal lastBlockId, BlockIdExternal initBlockId, BigInteger stateRootHash)
        {
            LastBlockId = lastBlockId;
            InitBlockId = initBlockId;
            StateRootHash = stateRootHash;
        }
    }

    public class ChainVersion
    {
        public int Version { get; set; }
        public long Capabilities { get; set; }
        public int Now { get; set; }

        public ChainVersion(int version, long capabilities, int now)
        {
            Version = version;
            Capabilities = capabilities;
            Now = now;
        }
    }
    
    public class MasterChainInfoExternal
    {
        public int Version { get; set; }
        public long Capabilities { get; set; }
        public int LastUTime { get; set; }
        public int Now { get; set; }
        public BlockIdExternal LastBlockId { get; set; }
        public BlockIdExternal InitBlockId { get; set; }
        public BigInteger StateRootHash { get; set; }

        public MasterChainInfoExternal(int version, long capabilities, int lastUTime, int now, BlockIdExternal lastBlockId, BlockIdExternal initBlockId, BigInteger stateRootHash)
        {
            Version = version;
            Capabilities = capabilities;
            LastUTime = lastUTime;
            Now = now;
            LastBlockId = lastBlockId;
            InitBlockId = initBlockId;
            StateRootHash = stateRootHash;
        }
    }

    public class BlockData
    {
        public BlockIdExternal BlockId { get; set; }
    }
}