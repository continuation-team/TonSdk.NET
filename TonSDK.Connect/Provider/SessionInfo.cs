using NaCl;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
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
            byte[] receiverPkBytes = Utils.HexToBytes(receiverPubKeyHex);
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            Curve25519XSalsa20Poly1305 box = new Curve25519XSalsa20Poly1305(KeyPair.PrivateKey, receiverPkBytes);

            using var rng = RandomNumberGenerator.Create();
            byte[] nonce = new byte[XSalsa20Poly1305.NonceLength];
            rng.GetBytes(nonce);

            byte[] cipherText = new byte[message.Length + XSalsa20Poly1305.TagLength];
            
            box.Encrypt(cipherText, messageBytes, nonce);

            byte[] result = new byte[NONCE_SIZE + cipherText.Length];
            Array.Copy(nonce, 0, result, 0, NONCE_SIZE);
            Array.Copy(cipherText, 0, result, NONCE_SIZE, cipherText.Length);

            return Convert.ToBase64String(result);
        }

        public string Decrypt(byte[] message, string senderPubKeyHex)
        {
            byte[] nonce = new byte[NONCE_SIZE];
            byte[] internalMessage = new byte[message.Length - NONCE_SIZE];

            Array.Copy(message, 0, nonce, 0, NONCE_SIZE);
            Array.Copy(message, NONCE_SIZE, internalMessage, 0, internalMessage.Length);

            byte[] senderPkBytes = Utils.HexToBytes(senderPubKeyHex);
            Curve25519XSalsa20Poly1305 box = new Curve25519XSalsa20Poly1305(KeyPair.PrivateKey, senderPkBytes);
            byte[] decryptedMessage = new byte[internalMessage.Length - XSalsa20Poly1305.TagLength];

            bool isDecrypted = box.TryDecrypt(decryptedMessage, internalMessage, nonce);
            string messageText = Encoding.UTF8.GetString(decryptedMessage);

            return messageText;
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