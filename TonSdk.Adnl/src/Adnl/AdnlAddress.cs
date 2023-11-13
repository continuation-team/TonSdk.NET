using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Utils = TonSdk.Core.Crypto.Utils;

namespace TonSdk.Adnl
{
    internal partial class AdnlAddress
    {
        private byte[] _publicKey;

        internal AdnlAddress(byte[] publicKey)
        {
            _publicKey = publicKey;
            if (_publicKey.Length != 32)
                throw new Exception("ADNLAddress: Bad peer public key. Must contain 32 bytes.");
        }

        internal AdnlAddress(string publicKey)
        {
            publicKey = publicKey.Trim();

            if (IsHex(publicKey)) _publicKey = Utils.HexToBytes(publicKey);
            else if (IsBase64(publicKey)) _publicKey = Convert.FromBase64String(publicKey);
            else throw new Exception("ADNLAddress: Bad peer public key.");
            if (_publicKey.Length != 32)
                throw new Exception("ADNLAddress: Bad peer public key. Must contain 32 bytes.");
        }

        internal byte[] PublicKey => _publicKey;

        internal byte[] Hash
        {
            get
            {
                using SHA256 sha256 = SHA256.Create();
                byte[] sha256Hash = sha256.ComputeHash(new byte[] { 0xc6, 0xb4, 0x13, 0x48 }.Concat(_publicKey).ToArray());
                return sha256Hash;
            }
        }

        private static bool IsHex(string? data)
        {
            if (data == null) return false;
            Regex re = new Regex("^[a-fA-F0-9]+$");
            return re.IsMatch(data);
        }

        private static bool IsBase64(string? data)
        {
            if (data == null) return false;
            Regex re = new Regex("^(?:[A-Za-z0-9+/]{4})*(?:[A-Za-z0-9+/]{2}==|[A-Za-z0-9+/]{3}=)?$");
            return re.IsMatch(data);
        }
    }
}