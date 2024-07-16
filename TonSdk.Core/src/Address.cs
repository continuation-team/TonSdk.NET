using System;
using System.Linq;
using System.Text.RegularExpressions;
using Org.BouncyCastle.Math;
using TonSdk.Core.Block;
using TonSdk.Core.Boc;
using TonSdk.Core.Crypto;

namespace TonSdk.Core {
    public interface IAddressRewriteOptions {
        int? Workchain { get; set; }
        bool? Bounceable { get; set; }
        bool? TestOnly { get; set; }
    }

    public interface IAddressStringifyOptions : IAddressRewriteOptions {
        bool? UrlSafe { get; set; }
    }

    public class AddressStringifyOptions : IAddressStringifyOptions {
        public int? Workchain { get; set; }
        public bool? Bounceable { get; set; }
        public bool? TestOnly { get; set; }
        public bool? UrlSafe { get; set; }

        public AddressStringifyOptions(bool bounceable, bool testOnly, bool urlSafe, int workchain = 0) {
            Workchain = workchain;
            Bounceable = bounceable;
            TestOnly = testOnly;
            UrlSafe = urlSafe;
        }
    }

    public class AddressTag {
        public bool Bounceable { get; set; }
        public bool TestOnly { get; set; }
    }

    public class AddressData : AddressTag {
        public int Workchain { get; set; }
        public byte[] Hash { get; set; }
    }

    public enum AddressType {
        Base64,
        Raw
    }

    public class Address {
        private const byte FLAG_BOUNCEABLE = 0x11;
        private const byte FLAG_NON_BOUNCEABLE = 0x51;
        private const byte FLAG_TEST_ONLY_BOUNCEABLE = 0x91;
        private const byte FLAG_TEST_ONLY_NON_BOUNCEABLE = 0xd1;

        private readonly byte[] _hash;
        private int _workchain;
        private bool _bounceable;
        private bool _testOnly;

        public Address(int workchain, StateInit stateInit, IAddressRewriteOptions? options = null) {
            _hash = stateInit.Cell.Hash.Parse().LoadBytes(32);
            _workchain = options?.Workchain ?? workchain;
            _bounceable = options?.Bounceable ?? true;
            _testOnly = options?.TestOnly ?? false;
        }

        /// <summary>
        /// Initializes a new instance of the Address class.
        /// </summary>
        /// <param name="workchain">The workchain of the address.</param>
        /// <param name="hash">The hash value of the address.</param>
        /// <param name="options">An optional IAddressRewriteOptions object specifying custom options.</param>
        public Address(int workchain, byte[] hash, IAddressRewriteOptions? options = null) {
            _hash = hash;
            _workchain = options?.Workchain ?? workchain;
            _bounceable = options?.Bounceable ?? true;
            _testOnly = options?.TestOnly ?? false;
        }

        /// <summary>
        /// Initializes a new instance of the Address class based on an existing address.
        /// </summary>
        /// <param name="address">The existing address to create a new address from.</param>
        /// <param name="options">An optional IAddressRewriteOptions object specifying custom options.</param>
        /// <exception cref="Exception">Thrown when the address cannot be parsed.</exception>
        public Address(Address address, IAddressRewriteOptions? options = null) {
            bool isAddress = IsAddress(address);
            AddressData result;

            if (isAddress) result = ParseAddress(address);
            else throw new Exception("Address: can\t parse address. Unknown type.");

            var workchain = options?.Workchain ?? result.Workchain;
            var bounceable = options?.Bounceable ?? result.Bounceable;
            var testOnly = options?.TestOnly ?? result.TestOnly;

            _hash = result.Hash;
            _workchain = workchain;
            _bounceable = bounceable;
            _testOnly = testOnly;
        }

        /// <summary>
        /// Initializes a new instance of the Address class based on a string representation of the address.
        /// </summary>
        /// <param name="address">The string representation of the address.</param>
        /// <param name="options">An optional IAddressRewriteOptions object specifying custom options.</param>
        /// <exception cref="Exception">Thrown when the address cannot be parsed.</exception>
        public Address(string address, IAddressRewriteOptions? options = null) {
            bool isEncoded = IsEncoded(address);
            bool isRaw = IsRaw(address);

            AddressData result = true switch {
                true when isEncoded => ParseEncoded(address),
                true when isRaw => ParseRaw(address),
                _ => throw new Exception("Address: can\t parse address. Unknown type."),
            };

            var workchain = options?.Workchain ?? result.Workchain;
            var bounceable = options?.Bounceable ?? result.Bounceable;
            var testOnly = options?.TestOnly ?? result.TestOnly;

            _hash = result.Hash;
            _workchain = workchain;
            _bounceable = bounceable;
            _testOnly = testOnly;
        }

        /// <summary>
        /// Return the hash value of the address.
        /// </summary>
        /// <returns>The hash value of the address.</returns>
        public byte[] GetHash() => _hash;

        /// <summary>
        /// Return the workchain of the address.
        /// </summary>
        /// <returns>The workchain of the address.</returns>
        public int GetWorkchain() => _workchain;

        /// <summary>
        /// Checks if the address is bounceable.
        /// </summary>
        /// <returns>True if the address is bounceable; otherwise, false.</returns>
        public bool IsBounceable() => _bounceable;

        /// <summary>
        /// Sets bounceable property and return it.
        /// </summary>
        /// <returns>True if the address is bounceable; otherwise, false.</returns>
        public bool IsBounceable(bool state) {
            return _bounceable = state;
        }

        /// <summary>
        /// Checks if the address is testonly.
        /// </summary>
        /// <returns>True if the address is testonly; otherwise, false.</returns>
        public bool IsTestOnly() => _testOnly;

        /// <summary>
        /// Sets testonly property and return it.
        /// </summary>
        /// <returns>True if the address is testonly; otherwise, false.</returns>
        public bool IsTestOnly(bool state) {
            return _testOnly = state;
        }

        private static bool IsAddress(object address) {
            return address is Address;
        }

        private static bool IsEncoded(object address) {
            return address is string str && str.Length == 48 &&
                   (BitsPatterns.isBase64(str) || BitsPatterns.isBase64url(str));
        }

        private static bool IsRaw(object address) {
            const string pattern = "^-?[0-9]:[a-zA-Z0-9]{64}$";
            return address is string str && Regex.IsMatch(str, pattern);
        }

        private static byte EncodeTag(AddressTag options) {
            bool bounceable = options.Bounceable;
            bool testOnly = options.TestOnly;

            if (bounceable && !testOnly) return FLAG_BOUNCEABLE;
            if (!bounceable && !testOnly) return FLAG_NON_BOUNCEABLE;
            if (bounceable && testOnly) return FLAG_TEST_ONLY_BOUNCEABLE;
            return FLAG_TEST_ONLY_NON_BOUNCEABLE;
        }

        private static AddressTag DecodeTag(int tag) {
            switch (tag) {
                case FLAG_BOUNCEABLE: {
                    return new AddressTag() { Bounceable = true, TestOnly = false };
                }
                case FLAG_NON_BOUNCEABLE: {
                    return new AddressTag() { Bounceable = false, TestOnly = false };
                }
                case FLAG_TEST_ONLY_BOUNCEABLE: {
                    return new AddressTag() { Bounceable = true, TestOnly = true };
                }
                case FLAG_TEST_ONLY_NON_BOUNCEABLE: {
                    return new AddressTag() { Bounceable = false, TestOnly = true };
                }
                default: throw new Exception("Address: bad address tag.");
            }
        }

        private static AddressData ParseAddress(Address value) {
            int workchain = value._workchain;
            bool bounceable = value._bounceable;
            bool testOnly = value._testOnly;
            byte[] hash = value._hash;

            return new AddressData {
                Workchain = workchain,
                Bounceable = bounceable,
                TestOnly = testOnly,
                Hash = hash
            };
        }

        private static AddressData ParseEncoded(string value) {

            BitsSlice slice = new Bits(value).Parse();
            byte[] crcBytes = slice.ReadBits(16 + 256).ToBytes();

            byte tag = (byte)slice.LoadUInt(8);
            sbyte workchain = (sbyte)slice.LoadInt(8);
            byte[] hash = slice.LoadBytes(32);
            byte[] checksum = slice.LoadBits(16).ToBytes();
            byte[] crc = Crypto.Utils.Crc16BytesBigEndian(crcBytes);

            if (!crc.SequenceEqual(checksum)) throw new Exception("Address: can't parse address. Wrong checksum.");

            //string base64 = value.Replace("-", "+").Replace("_", "/");
            //byte[] bytes = Convert.FromBase64String(base64);
            //List<byte> data = new List<byte>(bytes);
            //byte[] address = data.Take(34).ToArray();
            //data.RemoveRange(0, 34);
            //byte[] checksum = data.Take(2).ToArray();
            //data.RemoveRange(0, 2);
            //byte[] crc = Crypto.Utils.Crc16BytesBigEndian(address);
            //byte[] firstTwoBytes = address.Take(2).ToArray();
            //address = address.Skip(2).ToArray();
            //byte tag = firstTwoBytes[0];
            //sbyte workchain = (sbyte)firstTwoBytes[1];
            //byte[] hash = address.Take(32).ToArray();

            var decodeTagResult = DecodeTag(tag);

            return new AddressData {
                Bounceable = decodeTagResult.Bounceable,
                TestOnly = decodeTagResult.TestOnly,
                Workchain = workchain,
                Hash = hash
            };
        }

        private static AddressData ParseRaw(string value) {
            var data = value.Split(':');
            var workchain = int.Parse(data[0]);
            var hash = new Bits(data[1]).Parse().LoadBytes(32);
            var bounceable = true;
            var testOnly = false;

            return new AddressData() {
                Bounceable = bounceable,
                TestOnly = testOnly,
                Workchain = workchain,
                Hash = hash
            };
        }
        
        private bool CompareBytes(byte[] array, byte[] compareWith)
        {
            if (array.Length != compareWith.Length) return false;
        
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] != compareWith[i]) return false;
            }

            return true;
        }

        /// <summary>
        /// Determines whether the specified Address object is equal to the current Address.
        /// </summary>
        /// <param name="address">The Address object to compare with the current Address.</param>
        /// <returns>True if the specified Address is equal to the current Address; otherwise, false.</returns>
        public bool Equals(Address address) {
            return (address == this) || (
                CompareBytes(_hash, address._hash) &&
                _workchain == address._workchain
            );
        }

        /// <summary>
        /// Converts the Address to its BOC (Bag of Cells) representation.
        /// </summary>
        /// <returns>The BOC representation of the Address.</returns>
        public string ToBOC() {
            return new CellBuilder().StoreAddress(this).Build().Serialize().ToString();
        }

        /// <summary>
        /// Converts the Address to its string representation.
        /// </summary>
        /// <returns>The string representation of the Address.</returns>
        public override string ToString() {
            return ToString();
        }

        /// <summary>
        /// Converts the Address to its string representation.
        /// </summary>
        /// <param name="type">The type of string representation to use (default is AddressType.Base64).</param>
        /// <param name="options">Optional options for customizing the string representation.</param>
        /// <returns>The string representation of the Address.</returns>
        public string ToString(AddressType type = AddressType.Base64, IAddressStringifyOptions? options = null) {
            int workchain;
            bool bounceable;
            bool testOnly;
            bool urlSafe;

            if (options == null) {
                workchain = _workchain;
                bounceable = _bounceable;
                testOnly = _testOnly;
                urlSafe = true;
            }
            else {
                workchain = options.Workchain ?? _workchain;
                bounceable = options.Bounceable ?? _bounceable;
                testOnly = options.TestOnly ?? _testOnly;
                urlSafe = options.UrlSafe ?? true;
            }

            if (workchain < -128 || workchain >= 128) {
                throw new Exception("Address: workchain must be int8.");
            }

            if (type == AddressType.Raw) {
                return $"{workchain}:{Utils.BytesToHex(_hash).ToLower()}";
            }

            byte tag = EncodeTag(new AddressTag() { Bounceable = bounceable, TestOnly = testOnly });
            
            var addressBits = new BitsBuilder(8 + 8 + 256 + 16).StoreUInt(tag, 8).StoreInt(workchain, 8)
                .StoreBytes(_hash);

            var checksum = Crypto.Utils.Crc16(addressBits.Data.ToBytes());
            addressBits.StoreUInt(checksum, 16);


            if (urlSafe) {
                return addressBits.Build().ToString("base64url");
            }
            else {
                return addressBits.Build().ToString("base64");
            }
        }
    }
    
    public class ExternalAddress
    {
        public static bool IsAddress(object src) 
            => src is ExternalAddress;

        private readonly int _len;
        private readonly Bits _value;

        public ExternalAddress(int len, Bits value)
        {
            _value = value;
            _len = len;
        }

        public override string ToString() 
            => $"External<{_len}:{_value.ToString("base64")}>";
    }
}
