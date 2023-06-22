using System.Text;
using System.Text.RegularExpressions;
using TonSdk.Core.Crypto;
namespace TonSdk.Core;
public interface IAddressRewriteOptions
{
    int? Workchain { get; set; }
    bool? Bounceable { get; set; }
    bool? TestOnly { get; set; }
}

public interface IAddressStringifyOptions : IAddressRewriteOptions
{
    bool? UrlSafe { get; set; }
}

public class AddressStringifyOptions : IAddressStringifyOptions
{
    public int? Workchain { get; set; }
    public bool? Bounceable { get; set; }
    public bool? TestOnly { get; set; }
    public bool? UrlSafe { get; set; }

    public AddressStringifyOptions(bool bounceable, bool testOnly, bool urlSafe, int workchain = 0)
    {
        Workchain = workchain;
        Bounceable = bounceable;
        TestOnly = testOnly;
        UrlSafe = urlSafe;
    }
}

public class AddressTag
{
    public bool Bounceable { get; set; }
    public bool TestOnly { get; set; }
}

public class AddressData : AddressTag
{
    public int Workchain { get; set; }
    public byte[] Hash { get; set; }
}

public enum AddressType
{
    Base64,
    Raw
}

public class Address
{
    private const byte FLAG_BOUNCEABLE = 0x11;
    private const byte FLAG_NON_BOUNCEABLE = 0x51;
    private const byte FLAG_TEST_ONLY = 0x80;

    private readonly byte[] _hash;
    private readonly int _workchain;
    private readonly bool _bounceable;
    private readonly bool _testOnly;

    public Address(Address address, IAddressRewriteOptions? options = null)
    {
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

    public Address(string address, IAddressRewriteOptions? options = null)
    {
        bool isEncoded = IsEncoded(address);
        bool isRaw = IsRaw(address);

        AddressData result = true switch
        {
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

    public byte[] GetHash() => _hash;

    public int GetWorkchain() => _workchain;

    public bool IsBounceable() => _bounceable;

    public bool IsTestOnly() => _testOnly;

    private static bool IsAddress(object address)
    {
        return address is Address;
    }

    private static bool IsEncoded(object address)
    {
        const string pattern = "^([a-zA-Z0-9_-]{48}|[a-zA-Z0-9\\/\\+]{48})$";
        return address is string str && Regex.IsMatch(str, pattern);
    }

    private static bool IsRaw(object address)
    {
        const string pattern = "^-?[0-9]:[a-zA-Z0-9]{64}$";
        return address is string str && Regex.IsMatch(str, pattern);
    }

    private static byte EncodeTag(AddressTag options)
    {
        bool bounceable = options.Bounceable;
        bool testOnly = options.TestOnly;
        byte tag = bounceable ? FLAG_BOUNCEABLE : FLAG_NON_BOUNCEABLE;

        return (byte)(testOnly ? (tag | FLAG_TEST_ONLY) : tag);
    }

    private static AddressTag DecodeTag(int tag)
    {
        int data = tag;
        bool testOnly = (data & FLAG_TEST_ONLY) != 0;

        if (testOnly)
        {
            data ^= FLAG_TEST_ONLY;
        }

        if (!(new int[] { FLAG_BOUNCEABLE, FLAG_NON_BOUNCEABLE }).Contains(data))
        {
            throw new Exception("Address: bad address tag.");
        }

        bool bounceable = data == FLAG_BOUNCEABLE;

        return new AddressTag()
        {
            Bounceable = bounceable,
            TestOnly = testOnly
        };
    }

    private static AddressData ParseAddress(Address value)
    {
        int workchain = value._workchain;
        bool bounceable = value._bounceable;
        bool testOnly = value._testOnly;
        byte[] hash = value._hash.ToArray();

        return new AddressData
        {
            Workchain = workchain,
            Bounceable = bounceable,
            TestOnly = testOnly,
            Hash = hash
        };
    }

    private static AddressData ParseEncoded(string value)
    {
        string base64 = value.Replace("-", "+").Replace("_", "/");
        byte[] bytes = Convert.FromBase64String(base64);
        List<byte> data = new List<byte>(bytes);
        byte[] address = data.Take(34).ToArray();
        data.RemoveRange(0, 34);
        byte[] checksum = data.Take(2).ToArray();
        data.RemoveRange(0, 2);
        byte[] crc = Utils.Crc16BytesBigEndian(address);

        if (!crc.SequenceEqual(checksum))
        {
            throw new Exception("Address: can't parse address. Wrong checksum.");
        }

        byte[] firstTwoBytes = address.Take(2).ToArray();
        address = address.Skip(2).ToArray();
        byte tag = firstTwoBytes[0];
        sbyte workchain = (sbyte)firstTwoBytes[1];
        byte[] hash = address.Take(32).ToArray();

        var decodeTagResult = Address.DecodeTag(tag);

        return new AddressData
        {
            Bounceable = decodeTagResult.Bounceable,
            TestOnly = decodeTagResult.TestOnly,
            Workchain = workchain,
            Hash = hash
        };
    }

    private static AddressData ParseRaw(string value)
    {
        var data = value.Split(':');
        var workchain = int.Parse(data[0]);
        var hash = HexToBytes(data[1]);
        var bounceable = true;
        var testOnly = false;

        return new AddressData()
        {
            Bounceable = bounceable,
            TestOnly = testOnly,
            Workchain = workchain,
            Hash = hash
        };
    }

    private static byte[] HexToBytes(string hexString)
    {
        if (hexString.Length % 2 != 0)
        {
            throw new ArgumentException("Hex string must have an even number of characters.");
        }

        var byteArray = new byte[hexString.Length / 2];

        for (int i = 0; i < byteArray.Length; i++)
        {
            byteArray[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
        }

        return byteArray;
    }

    private static string BytesToHex(byte[] bytes)
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < bytes.Length; i++)
        {
            sb.Append(bytes[i].ToString("X2"));
        }
        return sb.ToString();
    }

    private static bool BytesCompare(byte[] a1, byte[] a2)
    {
        if (a1 == null || a2 == null)
        {
            return false;
        }

        if (a1.Length != a2.Length)
        {
            return false;
        }

        for (int i = 0; i < a1.Length; i++)
        {
            if (a1[i] != a2[i])
            {
                return false;
            }
        }

        return true;
    }

    public bool Equals (Address address)
    {
        return (address == this) || (
            BytesCompare(_hash, address._hash) &&
            _workchain == address._workchain
        );
    }

    public string ToString(AddressType type = AddressType.Base64, IAddressStringifyOptions? options = null)
    {
        int workchain;
        bool bounceable;
        bool testOnly;
        bool urlSafe;

        if (options == null)
        {
            workchain = _workchain;
            bounceable = _bounceable;
            testOnly = _testOnly;
            urlSafe = true;
        }
        else
        {
            workchain = options.Workchain ?? _workchain;
            bounceable = options.Bounceable ?? _bounceable;
            testOnly = options.TestOnly ?? _testOnly;
            urlSafe = options.UrlSafe ?? true;
        }

        if (workchain < -128 || workchain >= 128)
        {
            throw new Exception("Address: workchain must be int8.");
        }

        if (bounceable.GetType() != typeof(bool))
        {
            throw new Exception("Address: bounceable flag must be a boolean.");
        }

        if (testOnly.GetType() != typeof(bool))
        {
            throw new Exception("Address: testOnly flag must be a boolean.");
        }

        if (urlSafe.GetType() != typeof(bool))
        {
            throw new Exception("Address: urlSafe flag must be a boolean.");
        }

        if (type == AddressType.Raw)
        {
            return $"{workchain}:{BytesToHex(_hash)}".ToLower();
        }

        byte tag = EncodeTag(new AddressTag() { Bounceable = bounceable, TestOnly = testOnly});
        byte[] address = new byte[1 + 1 + _hash.Length];
        address[0] = tag;
        address[1] = (byte)workchain;
        Array.Copy(_hash, 0, address, 2, _hash.Length);

        var checksum = Utils.Crc16BytesBigEndian(address);
        byte[] data = new byte[address.Length + checksum.Length];

        Array.Copy(address, 0, data, 0, address.Length);
        Array.Copy(checksum, 0, data, address.Length, checksum.Length);

        string base64 = Convert.ToBase64String(data);
        if (urlSafe)
        {
            base64 = base64.Replace('/', '_').Replace('+', '-');
        }
        else
        {
            base64 = base64.Replace('_', '/').Replace('-', '+');
        }

        return base64;
    }
}
