using Org.BouncyCastle.Crypto.Parameters;
using System;
using System.Security.Cryptography;
using System.Text;
using TonSdk.Core.Crypto;

namespace TonSdk.Connect
{

    public class CryptedSessionInfo
    {
        public const int NONCE_SIZE = 24;
        public KeyPair KeyPair { get; private set; }
        public string SesionId { get; private set; }

        public CryptedSessionInfo(string? seed = null)
        {
            byte[] seedBytes = (seed != null ? Utils.HexToBytes(seed) : GenerateRandomBytes(32));
            KeyPair = GenerateKeyPair(seedBytes);
            SesionId = Utils.BytesToHex(KeyPair.PublicKey);
        }

        public string Encrypt(string message, string receiverPubKeyHex)
        {
            byte[] nonce = Sodium.PublicKeyBox.GenerateNonce();
            byte[] receiverPkBytes = Sodium.Utilities.HexToBinary(receiverPubKeyHex);

            byte[] encrypted = Sodium.PublicKeyBox.Create(message, nonce, KeyPair.PrivateKey, receiverPkBytes);

            byte[] result = new byte[NONCE_SIZE + encrypted.Length];
            Array.Copy(nonce, 0, result, 0, NONCE_SIZE);
            Array.Copy(encrypted, 0, result, NONCE_SIZE, encrypted.Length);

            return Convert.ToBase64String(result);
        }

        public string Decrypt(byte[] message, string senderPubKeyHex)
        {
            byte[] nonce = new byte[NONCE_SIZE];
            byte[] internalMessage = new byte[message.Length - NONCE_SIZE];

            Array.Copy(message, 0, nonce, 0, NONCE_SIZE);
            Array.Copy(message, NONCE_SIZE, internalMessage, 0, internalMessage.Length);

            byte[] senderPkBytes = Sodium.Utilities.HexToBinary(senderPubKeyHex);
            byte[] decrypted = Sodium.PublicKeyBox.Open(internalMessage, nonce, KeyPair.PrivateKey, senderPkBytes);

            return Encoding.UTF8.GetString(decrypted);
        }

        public static KeyPair GenerateKeyPair(byte[] seed)
        {
            X25519PrivateKeyParameters x25519PrivateKeyParameters = new X25519PrivateKeyParameters(seed, 0);
            X25519PublicKeyParameters x25519PublicKeyParameters = x25519PrivateKeyParameters.GeneratePublicKey();
            byte[] encoded = x25519PrivateKeyParameters.GetEncoded();
            byte[] encoded2 = x25519PublicKeyParameters.GetEncoded();
            return new KeyPair(encoded, encoded2);
        }

        public static byte[] GenerateRandomBytes(int byteSize)
        {
            using RandomNumberGenerator randomNumberGenerator = RandomNumberGenerator.Create();
            byte[] randomBytes = new byte[byteSize];
            randomNumberGenerator.GetBytes(randomBytes);
            return randomBytes;
        }
    }

    public struct SessionInfo
    {
        public string? SessionPrivateKey { get; set; }
        public string? WalletPublicKey { get; set; }
        public string? BridgeUrl { get; set; }
    }
}