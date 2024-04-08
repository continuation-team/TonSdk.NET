using System;
using System.Collections;
using System.Collections.Generic;
using TonSdk.Core.Boc;

namespace TonSdk.Client
{
    internal delegate void DataExtractor(ref WalletInformationResult result, Cell data);
    internal class WalletUtils
    {
        internal static readonly List<KnownWallet> KnownWallets = new List<KnownWallet>()
        {
            new KnownWallet("Wallet v1 r1", "oM/CxIruFqJx8s/AtzgtgXVs7LEBfQd/qqs7tgL2how=", SeqnoExtractor),
            new KnownWallet("Wallet v1 r2", "1JAvzJ+tdGmPqONTIgpo2g3PcuMryy657gQhfBfTBiw=", SeqnoExtractor),
            new KnownWallet("Wallet v1 r3", "WHzHie/xyE9G7DeX5F/ICaFP9a4k8eDHpqmcydyQYf8=", SeqnoExtractor),
            new KnownWallet("Wallet v2 r1", "XJpeaMEI4YchoHxC+ZVr+zmtd+xtYktgxXbsiO7mUyk=", SeqnoExtractor),
            new KnownWallet("Wallet v2 r2", "/pUw0yQ4Uwg+8u8LTCkIwKv2+hwx6iQ6rKpb+MfXU/E=", SeqnoExtractor),
            new KnownWallet("Wallet v3 r1", "thBBpYp5gLlG6PueGY48kE0keZ/6NldOpCUcQaVm9YE=", V3Extractor),
            new KnownWallet("Wallet v3 r2", "hNr6RJ+Ypph3ibojI1gHK8D3bcRSQAKl0JGLmnXS1Zk=", V3Extractor),
            new KnownWallet("Wallet v4 r1", "ZN1UgFUixb6KnbWc6gEFzPDQh4bKeb64y3nogKjXMi0=", V3Extractor),
            new KnownWallet("Wallet v4 r2", "/rX/aCDi/w2Ug+fg1iyBfYRniftK5YDIeIZtlZ2r1cA=", V3Extractor),
            new KnownWallet("Nominator Pool v1", "mj7BS8CY9rRAZMMFIiyuooAPF92oXuaoGYpwle3hDc8=", NoneExtractor)
        };

        private static void NoneExtractor(ref WalletInformationResult result, Cell data) {}
        
        private static void SeqnoExtractor(ref WalletInformationResult result, Cell data) =>
            result.Seqno = (long)data.Parse().LoadUInt(32);
        
        private static void V3Extractor(ref WalletInformationResult result, Cell data)
        {
            SeqnoExtractor(ref result, data);
            var slice = data.Parse();
            slice.LoadUInt(32);
            result.WalletId = (long)slice.LoadUInt(32);
        }
        
    }

    internal struct KnownWallet
    {
        internal string Type { get; set; }
        internal string CodeHash { get; set; }
        internal DataExtractor DataExtractor { get; set; }

        internal KnownWallet(string type, string hash, DataExtractor dataExtractor)
        {
            Type = type;
            CodeHash = hash;
            DataExtractor = dataExtractor;
        }
    }
}