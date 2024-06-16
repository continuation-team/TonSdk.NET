using System;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;
using TonSdk.Core.Boc;

namespace TonSdk.Core.Crypto
{
    public class KeyPair
    {
        private readonly byte[] _privateKey;
        private readonly byte[] _publicKey;
        public byte[] PrivateKey => _privateKey;
        public byte[] PublicKey => _publicKey;

        public KeyPair(byte[] privateKey, byte[] publicKey)
        {
            _privateKey = privateKey;
            _publicKey = publicKey;
        }

        private static byte[] SignDetached(byte[] hash, byte[] privateKey)
        {
            Ed25519PrivateKeyParameters privateKeyParams = new Ed25519PrivateKeyParameters(privateKey, 0);

            ISigner signer = new Ed25519Signer();
            signer.Init(true, privateKeyParams);

            signer.BlockUpdate(hash, 0, hash.Length);
            byte[] signature = signer.GenerateSignature();

            return signature;
        }

        /// <summary>
        /// Signs the hash of a Cell data using the specified key.
        /// </summary>
        /// <param name="data">The Cell data to sign.</param>
        /// <param name="key">The key used for signing.</param>
        /// <returns>The signature of the hashed Cell data.</returns>
        public static byte[] Sign(Cell data, byte[] key)
        {
            byte[] hash = data.Hash.ToBytes();
            byte[] signature = SignDetached(hash, key);

            return signature;
        }
    }
}