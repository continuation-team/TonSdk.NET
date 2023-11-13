using System;

namespace TonSdk.Adnl
{
    internal class Cipher
    {
        private readonly AesCtrMode _cipher;

        internal Cipher(byte[] key, byte[] iv)
        {
            if (key.Length != 32)
                throw new ArgumentException("Invalid key length. Key must be 256 bits.");

            if (iv.Length != 16)
                throw new ArgumentException("Invalid IV length. IV must be 128 bits.");

            _cipher = new AesCtrMode(key, new AesCounter(iv));
        }

        internal byte[] Update(byte[] data) => _cipher.Encrypt(data);
    }

    internal class Decipher
    {
        private readonly AesCtrMode _cipher;

        internal Decipher(byte[] key, byte[] iv)
        {
            if (key.Length != 32)
                throw new ArgumentException("Invalid key length. Key must be 256 bits.");

            if (iv.Length != 16)
                throw new ArgumentException("Invalid IV length. IV must be 128 bits.");

            _cipher = new AesCtrMode(key, new AesCounter(iv));
        }

        internal byte[] Update(byte[] data) => _cipher.Decrypt(data);
    }

    internal static class CipherFactory
    {
        internal static Cipher CreateCipheriv(byte[] key, byte[] iv) => new Cipher(key, iv);

        internal static Decipher CreateDecipheriv(byte[] key, byte[] iv) => new Decipher(key, iv);
    }
}
