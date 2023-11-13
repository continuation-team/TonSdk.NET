using System.Linq;
using System.Security.Cryptography;

namespace TonSdk.Adnl
{
    internal class AdnlAesParams
    {
        private byte[] _bytes = AdnlKeys.GenerateRandomBytes(160);

        internal byte[] Bytes => _bytes;

        internal byte[] RxKey => _bytes.Take(32).ToArray();

        internal byte[] TxKey => _bytes.Skip(32).Take(32).ToArray();

        internal byte[] RxNonce => _bytes.Skip(64).Take(16).ToArray();

        internal byte[] TxNonce => _bytes.Skip(80).Take(16).ToArray();

        internal byte[] Padding => _bytes.Skip(96).Take(64).ToArray();

        internal byte[] Hash
        {
            get 
            { 
                using SHA256 sha256 = SHA256.Create();
                byte[] sha256Hash = sha256.ComputeHash(_bytes);
                return sha256Hash; 
            }
        }
    }
}