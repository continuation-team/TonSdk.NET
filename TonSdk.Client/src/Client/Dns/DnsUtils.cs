using Org.BouncyCastle.Crypto.Digests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using TonSdk.Client.Stack;
using TonSdk.Core;
using TonSdk.Core.Boc;

namespace TonSdk.Client
{

    public class DnsUtils
    {
        public const string DNS_CATEGORY_NEXT_RESOLVER = "dns_next_resolver";
        public const string DNS_CATEGORY_WALLET = "wallet";
        public const string DNS_CATEGORY_SITE = "site";

        public static async Task<object> DnsResolve(TonClient client, Address rootDnsAddress, string domain, string category = null, bool? oneStep = null, BlockIdExtended? blockIdExtended = null)
        {
            byte[] domainBuffer = DomainToBuffer(domain);
            return await DnsResolveImpl(client, rootDnsAddress, domainBuffer, category, oneStep, blockIdExtended);
        }

        public static byte[] DomainToBuffer(string domain)
        {
            if (string.IsNullOrEmpty(domain)) throw new Exception("Empty domain");
            if (domain == ".") return new byte[] { 0 };

            string domainLower = domain.ToLower();

            if (domainLower.Where((t, i) => char.ConvertToUtf32(domainLower, i) < 32).Any())
            {
                throw new Exception("Bytes in range 0..32 are not allowed in domain names");
            }

            foreach (char s in domainLower)
            {
                for (int c = 127; c <= 159; c++)
                {
                    if (s == (char)c)
                    {
                        throw new Exception("Bytes in range 127..159 are not allowed in domain names");
                    }
                }
            }

            string[] domainPair = domain.Split('.');

            if (domainPair.Any(domainPart => domain.Length == 0))
                throw new Exception("Domain name cannot have an empty component");

            string rawDomain = string.Join("\0", domainPair.Reverse()) + "\0";
            byte[] buffer = Encoding.UTF8.GetBytes(rawDomain);
            return buffer;
        }

        private static async Task<object> DnsResolveImpl(TonClient client, Address dnsAddress, byte[] domainBytes, string category = null, bool? oneStep = null, BlockIdExtended? blockIdExtended = null)
        {
            int length = domainBytes.Length * 8;

            CellBuilder sliceBuilder = new CellBuilder();

            foreach (byte b in domainBytes)
            {
                sliceBuilder.StoreUInt(b, 8);
            }
            Cell domainCell = sliceBuilder.Build();
            BigInteger categoryBigInt = CategoryToBigInt(category);
            
            var stackItems = new List<IStackItem>()
            {
                new VmStackSlice()
                {
                    Value = domainCell.Parse()
                },
                new VmStackInt()
                {
                    Value = categoryBigInt
                }
            };
            var runGetMethodResult = await client.RunGetMethod(dnsAddress, "dnsresolve", stackItems.ToArray(), blockIdExtended);
            
            if(runGetMethodResult == null) throw new Exception("Cannot retrieve DNS resolve data.");
            if (runGetMethodResult.Value.ExitCode != 0 && runGetMethodResult.Value.ExitCode != 1) throw new Exception("Cannot retrieve DNS resolve data.");

            if (((client.GetClientType() == TonClientType.HTTP_TONCENTERAPIV2 
                  || client.GetClientType() == TonClientType.HTTP_TONWHALESAPI|| client.GetClientType() == TonClientType.HTTP_TONCENTERAPIV3) 
                 && runGetMethodResult.Value.Stack.Length != 2) 
                || client.GetClientType() == TonClientType.LITECLIENT &&
                runGetMethodResult.Value.StackItems.Length != 2)
                throw new Exception("Invalid dnsresolve response.");

            BigInteger lenBig = BigInteger.Zero;
            if (client.GetClientType() == TonClientType.HTTP_TONCENTERAPIV2 || client.GetClientType() == TonClientType.HTTP_TONWHALESAPI|| client.GetClientType() == TonClientType.HTTP_TONCENTERAPIV3)
                lenBig = (BigInteger)runGetMethodResult.Value.Stack[0];
            else
            {
                if (runGetMethodResult.Value.StackItems[0] is VmStackInt)
                    lenBig = ((VmStackInt)runGetMethodResult.Value.StackItems[0]).Value;
                else if (runGetMethodResult.Value.StackItems[0] is VmStackTinyInt)
                    lenBig = ((VmStackTinyInt)runGetMethodResult.Value.StackItems[0]).Value;
            }

            if (lenBig < 0) lenBig *= -1;
            uint resultLen = (uint)lenBig;
            
            Cell cell = client.GetClientType() == TonClientType.HTTP_TONCENTERAPIV2 || client.GetClientType() == TonClientType.HTTP_TONWHALESAPI|| client.GetClientType() == TonClientType.HTTP_TONCENTERAPIV3 
                ? (Cell)runGetMethodResult.Value.Stack[1] : ((VmStackCell)runGetMethodResult.Value.StackItems[1]).Value;

            if (cell == null || cell.Bits == null) throw new Exception("Invalid dnsresolve response.");
            if (resultLen == 0) return null;
            if (resultLen % 8 != 0) throw new Exception("domain split not at a component boundary");
            if (resultLen > length) throw new Exception($"invalid response {resultLen} /{length}");
            else if (resultLen == length)
            {
                if (category == DNS_CATEGORY_NEXT_RESOLVER) return cell != null ? ParseNextResolverRecord(cell) : null;
                else if (category == DNS_CATEGORY_WALLET) return cell != null ? ParseSmartContractAddressRecord(cell) : null;
                else if (category == DNS_CATEGORY_SITE) return cell ?? null;
                return cell;
            }
            else
            {
                if (cell == null) return null; // domain cannot be resolved
                Address nextAddress = ParseNextResolverRecord(cell)!;
                if (oneStep == true)
                {
                    if (category == DNS_CATEGORY_NEXT_RESOLVER) return nextAddress;
                    return null;
                }

                byte[] result = new byte[domainBytes.Length - (resultLen / 8)];
                Array.Copy(domainBytes, resultLen / 8, result, 0, result.Length);
                return await DnsResolveImpl(client, nextAddress, result, category, false, blockIdExtended);
            }
        }

        public static BigInteger CategoryToBigInt(string category)
        {
            if (category == null) return BigInteger.Zero;
            byte[] categoryBytes = Encoding.UTF8.GetBytes(category);
            string categoryHash = CalculateSHA256(categoryBytes);
            return new Bits(categoryHash).Parse().LoadUInt(256);
        }

        public static string CalculateSHA256(byte[] bytes)
        {
            var sha256 = new Sha256Digest();
            byte[] hashBytes = new byte[sha256.GetDigestSize()];
            sha256.BlockUpdate(bytes, 0, bytes.Length);
            sha256.DoFinal(hashBytes, 0);
            StringBuilder builder = new StringBuilder();

            for (int i = 0; i < hashBytes.Length; i++)
            {
                builder.Append(hashBytes[i].ToString("x2"));
            }

            return builder.ToString();
        }

        public static Address ParseSmartContractAddressImpl(Cell cell, int prefix0, int prefix1)
        {
            CellSlice ds = cell.Parse();
            if (ds.LoadUInt(8) != prefix0 || ds.LoadUInt(8) != prefix1) throw new Exception("Invalid dns record value prefix");
            return ds.LoadAddress();
        }
        public static Address ParseSmartContractAddressRecord(Cell cell)
        {
            return ParseSmartContractAddressImpl(cell, 0x9f, 0xd3);
        }
        public static Address ParseNextResolverRecord(Cell cell)
        {
            return ParseSmartContractAddressImpl(cell, 0xba, 0x93);
        }
    }
}
