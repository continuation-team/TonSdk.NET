using System;
using System.Numerics;
using TonSdk.Core;

namespace TonSdk.Adnl.LiteClient
{
    public class BlockIdExtended
    {
        public int Workchain { get; set; }
        public long Shard { get; set; }
        public int Seqno { get; set; }
        public byte[] RootHash { get; set; }
        public byte[] FileHash { get; set; }

        public BlockIdExtended(int workchain, byte[] rootHash, byte[] fileHash, long shard, int seqno)
        {
            Workchain = workchain;
            RootHash = rootHash;
            FileHash = fileHash;
            Shard = shard;
            Seqno = seqno;
        }

        public BlockIdExtended(int workchain, long shard, int seqno)
        {
            Workchain = workchain;
            Shard = shard;
            Seqno = seqno;
            RootHash = Array.Empty<byte>();
            FileHash = Array.Empty<byte>();
        }
    }

    public class MasterChainInfo
    {
        public BlockIdExtended LastBlockId { get; set; }
        public BlockIdExtended InitBlockId { get; set; }
        public BigInteger StateRootHash { get; set; }

        public MasterChainInfo(BlockIdExtended lastBlockId, BlockIdExtended initBlockId, BigInteger stateRootHash)
        {
            LastBlockId = lastBlockId;
            InitBlockId = initBlockId;
            StateRootHash = stateRootHash;
        }
    }

    public class BlockHeader
    {
        public BlockIdExtended BlockId { get; set; }
        public byte[] HeaderProof { get; set; }
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
    
    public class MasterChainInfoExtended
    {
        public int Version { get; set; }
        public long Capabilities { get; set; }
        public int LastUTime { get; set; }
        public int Now { get; set; }
        public BlockIdExtended LastBlockId { get; set; }
        public BlockIdExtended InitBlockId { get; set; }
        public BigInteger StateRootHash { get; set; }

        public MasterChainInfoExtended(int version, long capabilities, int lastUTime, int now, BlockIdExtended lastBlockId, BlockIdExtended initBlockId, BigInteger stateRootHash)
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
    
    public interface ITransactionId {}

    public class TransactionId : ITransactionId
    {
        public byte[] Account { get; set; }
        public long Lt { get; set; }
        
        public byte[] Hash { get; set; }
        
    }
    
    
    public class TransactionId3 : ITransactionId
    {
        public Address Account { get; set; }
        public long Lt { get; set; }

        public TransactionId3(Address account, long lt)
        {
            Account = account;
            Lt = lt;
        }
    }
    
    public class ListBlockTransactionsExtendedResult
    {
        public bool InComplete { get; set; }
        public byte[] Transactions { get; set; }
        public byte[] Proof { get; set; }

        public ListBlockTransactionsExtendedResult(bool inComplete, byte[] transactions, byte[] proof)
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
        
        public BlockIdExtended ShardBlock { get; set; }

        public ShardInfo(byte[] shardProof, byte[] shardDescr, BlockIdExtended shardBlock)
        {
            ShardBlock = shardBlock;
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
        public long Seqno { get; set; }

        public BlockId(int workchain, long shard, long seqno)
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
        public BlockIdExtended From { get; set; }
        public BlockIdExtended To { get; set; }
        public byte[] DestProof { get; set; }
        public byte[] Proof { get; set; }
        public byte[] StateProof { get; set; }
    }
    
    public class BlockLinkForward : IBlockLink
    {
        public bool ToKeyBlock { get; set; }
        public BlockIdExtended From { get; set; }
        public BlockIdExtended To { get; set; }
        public byte[] DestProof { get; set; }
        public byte[] ConfigProof { get; set; }
        public int ValidatorSetHash { get; set; }
        public int CatchainSeqno { get; set; }
        public Signature[] Signatures { get; set; }
    }

    public class PartialBlockProof
    {
        public bool Complete { get; set; }
        public BlockIdExtended From { get; set; }
        public BlockIdExtended To { get; set; }
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
        public BlockIdExtended BlockIdExtended { get; set; }
        public byte[] Proof { get; set; }
    }

    public class ShardBlockProof
    {
        public BlockIdExtended MasterChainId { get; set; }
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