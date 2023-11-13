using System;
using System.Security.Cryptography;

namespace TonSdk.Adnl
{

    public class AesCounter
    {
        private readonly byte[] _counter;

        public AesCounter(byte[] initialValue)
        {
            if (initialValue.Length != 16)
                throw new ArgumentException("Invalid counter bytes size (must be 16 bytes)");
            _counter = initialValue;
        }

        public AesCounter(int initialValue)
        {
            _counter = new byte[16];
            for (int i = 15; i >= 0; i--)
            {
                _counter[i] = (byte)(initialValue % 256);
                initialValue /= 256;
            }
        }

        public void Increment()
        {
            for (int i = 15; i >= 0; i--)
            {
                if (_counter[i] == 255)
                    _counter[i] = 0;
                else
                {
                    _counter[i]++;
                    break;
                }
            }
        }

        public byte[] Counter => _counter;
    }

    public class AesCtrMode
    {
        private AesCounter _counter;
        private byte[] _remainingCounter;
        private int _remainingCounterIndex;
        private Aes _aes;

        public AesCtrMode(byte[] key, AesCounter? counter)
        {
            _counter = counter ?? new AesCounter(1);
            _remainingCounter = new byte[16];
            _remainingCounterIndex = 16;

            _aes = Aes.Create();
            _aes.Key = key;
            _aes.Mode = CipherMode.ECB;
            _aes.Padding = PaddingMode.None;
        }

        public byte[] Encrypt(byte[] plaintext)
        {
            byte[] encrypted = new byte[plaintext.Length];

            for (int i = 0; i < encrypted.Length; i++)
            {
                if (_remainingCounterIndex == 16)
                {
                    _remainingCounter = EncryptCounter(_counter.Counter);
                    _remainingCounterIndex = 0;
                    _counter.Increment();
                }

                encrypted[i] = (byte)(plaintext[i] ^ _remainingCounter[_remainingCounterIndex++]);
            }

            return encrypted;
        }

        private byte[] EncryptCounter(byte[] counter)
        {
            using var encryptor = _aes.CreateEncryptor();
            return encryptor.TransformFinalBlock(counter, 0, counter.Length);
        }

        public byte[] Decrypt(byte[] ciphertext) => Encrypt(ciphertext);
    }
}