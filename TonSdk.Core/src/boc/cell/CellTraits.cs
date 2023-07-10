namespace TonSdk.Core.Boc {

    public enum CellType : int {
        ORDINARY = -1,
        PRUNED_BRANCH = 1,
        LIBRARY_REFERENCE = 2,
        MERKLE_PROOF = 3,
        MERKLE_UPDATE = 4
    }

    public static class CellTraits {
        public const int max_refs = 4;
        public const int max_bytes = 128;
        public const int max_bits = 1023;
        public const int hash_bytes = 32;
        public const int hash_bits = hash_bytes * 8;
        public const int depth_bytes = 2;
        public const int depth_bits = depth_bytes * 8;
        public const int max_level = 3;
        public const int max_depth = 1024;
        public const int max_virtualization = 7;
        public const int max_serialized_bytes = 2 + max_bytes + (max_level + 1) * (hash_bytes + depth_bytes);
    }
}
