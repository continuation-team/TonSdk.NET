using System;
using System.Numerics;
using TonSdk.Core;

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

    public class AccountState
    {
        
    }

    public class RunSmcMethodResult
    {
        public byte[] ShardProof { get; set; }
        public byte[] Proof { get; set; }
        public byte[] StateProof { get; set; }
        public byte[] InitC7 { get; set; }
        public byte[] LibExtras { get; set; }
        public int ExitCode { get; set; }
        public byte[] Result { get; set; }

        public RunSmcMethodResult(byte[] shardProof, byte[] proof, byte[] stateProof, byte[] initC7, byte[] libExtras, int exitCode, byte[] result)
        {
            ShardProof = shardProof;
            Proof = proof;
            StateProof = stateProof;
            InitC7 = initC7;
            LibExtras = libExtras;
            ExitCode = exitCode;
            Result = result;
        }
    }

    public class TransactionId
    {
        public BigInteger Account { get; set; }
        public long Lt { get; set; }
        
        public BigInteger Hash { get; set; }
        
    }
    
    public class TransactionId3
    {
        public Address Account { get; set; }
        public long Lt { get; set; }

        public TransactionId3(Address account, long lt)
        {
            Account = account;
            Lt = lt;
        }
    }
    
    public class ListBlockTransactionsExternalResult
    {
        public bool InComplete { get; set; }
        public byte[] Transactions { get; set; }
        public byte[] Proof { get; set; }

        public ListBlockTransactionsExternalResult(bool inComplete, byte[] transactions, byte[] proof)
        {
            InComplete = inComplete;
            Transactions = transactions;
            Proof = proof;
        }
    }

    public class ListBlockTransactionsResult
    {
        public bool InComplete { get; set; }
        public TransactionId[] TransactionIds { get; set; }
        public byte[] Proof { get; set; }

        public ListBlockTransactionsResult(bool inComplete, TransactionId[] transactionIds, byte[] proof)
        {
            InComplete = inComplete;
            TransactionIds = transactionIds;
            Proof = proof;
        }
    }
    
    [Flags]
    internal enum RunSmcModes
    {
        None = 0,
        ShardProof = 1 << 0, // mode.0
        Proof = 1 << 0,      // mode.0
        StateProof = 1 << 1, // mode.1
        InitC7 = 1 << 3,     // mode.3
        LibExtras = 1 << 4,  // mode.4
        Result = 1 << 2      // mode.2
    }
    
    public class RunSmcOptions
    {
        public bool ShardProof { get; set; }
        public bool Proof { get; set; }
        public bool StateProof { get; set; }
        public bool InitC7 { get; set; }
        public bool LibExtras { get; set; }
        public bool Result { get; set; }
    }

    public class ShardInfo
    {
        public byte[] ShardProof { get; set; }
        public byte[] ShardDescr { get; set; }

        public ShardInfo(byte[] shardProof, byte[] shardDescr)
        {
            ShardProof = shardProof;
            ShardDescr = shardDescr;
        }
    }
    
    public class AllShardsInfo
    {
        public byte[] Data { get; set; }

        public AllShardsInfo( byte[] data)
        {
            Data = data;
        }
    }

    public class BlockId
    {
        public int Workchain { get; set; }
        public long Shard { get; set; }
        public int Seqno { get; set; }

        public BlockId(int workchain, long shard, int seqno)
        {
            Workchain = workchain;
            Shard = shard;
            Seqno = seqno;
        }
    }

    public class Signature
    {
        public BigInteger NodeIdShort { get; set; }
        public byte[] SignatureBytes { get; set; }
    }
    
    public interface IBlockLink {}

    public class BlockLinkBack : IBlockLink
    {
        public bool ToKeyBlock { get; set; }
        public BlockIdExternal From { get; set; }
        public BlockIdExternal To { get; set; }
        public byte[] DestProof { get; set; }
        public byte[] Proof { get; set; }
        public byte[] StateProof { get; set; }
    }
    
    public class BlockLinkForward : IBlockLink
    {
        public bool ToKeyBlock { get; set; }
        public BlockIdExternal From { get; set; }
        public BlockIdExternal To { get; set; }
        public byte[] DestProof { get; set; }
        public byte[] ConfigProof { get; set; }
        public int ValidatorSetHash { get; set; }
        public int CatchainSeqno { get; set; }
        public Signature[] Signatures { get; set; }
    }

    public class PartialBlockProof
    {
        public bool Complete { get; set; }
        public BlockIdExternal From { get; set; }
        public BlockIdExternal To { get; set; }
        public IBlockLink[] BlockLinks { get; set; }
    }

    public class ConfigInfo
    {
        public byte[] StateProof { get; set; }
        public byte[] ConfigProof { get; set; }
    }

    public class LibraryEntry
    {
        public BigInteger Hash { get; set; }
        public byte[] Data { get; set; }
    }

    public class ShardBlockLink
    {
        public BlockIdExternal BlockIdExternal { get; set; }
        public byte[] Proof { get; set; }
    }

    public class ShardBlockProof
    {
        public BlockIdExternal MasterChainId { get; set; }
        public ShardBlockLink[] Links { get; set; }
    }

    public class ValidatorStats
    {
        public int Count { get; set; }
        public bool Complete { get; set; }
        public byte[] StateProof { get; set; }
        public byte[] DataProof { get; set; }
    }
}