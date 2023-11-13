using System;
using System.Linq;
using System.Security.Cryptography;
using TonSdk.Core.Boc;

namespace TonSdk.Adnl
{
    internal class AdnlPacket
    {
        internal const byte PacketMinSize = 68; // 4 (size) + 32 (nonce) + 32 (hash)

        private byte[] _payload;
        private byte[] _nonce;

        internal AdnlPacket(byte[] payload, byte[]? nonce = null)
        {
            _nonce = nonce ?? AdnlKeys.GenerateRandomBytes(32);
            _payload = payload;
        }

        internal byte[] Payload => _payload;

        private byte[] Nonce => _nonce;

        private byte[] Hash
        {
            get 
            { 
                using SHA256 sha256 = SHA256.Create();
                byte[] sha256Hash = sha256.ComputeHash(_nonce.Concat(_payload).ToArray());
                return sha256Hash; 
            }
        }

        private byte[] Size
        {
            get
            {
                uint size = (uint)(32 + 32 + _payload.Length);
                Bits builder = new BitsBuilder().StoreUInt32LE(size).Build();
                return builder.ToBytes();
            }
        }

        internal byte[] Data => Size.Concat(Nonce).Concat(Payload).Concat(Hash).ToArray();

        internal int Length => PacketMinSize + _payload.Length;

        internal static AdnlPacket? Parse(byte[] data)
        {
            if (data.Length < 4) return null;
            int cursor = 0;

            BitsSlice slice = new Bits(data).Parse();

            uint size = (uint)slice.LoadUInt32LE();
            cursor += 4;

            if (data.Length - 4 < size) return null;

            byte[] nonce = new byte[32];
            Array.Copy(data, cursor, nonce, 0, 32);
            cursor += 32;

            byte[] payload = new byte[size - (32 + 32)];
            Array.Copy(data, cursor, payload, 0, size - (32 + 32));
            cursor += (int)size - (32 + 32);

            byte[] hash = new byte[32];
            Array.Copy(data, cursor, hash, 0, 32);
            
            using SHA256 sha256 = SHA256.Create();
            byte[] target = sha256.ComputeHash(nonce.Concat(payload).ToArray());

            if (!hash.SequenceEqual(target)) throw new Exception("ADNLPacket: Bad packet hash.");

            return new AdnlPacket(payload, nonce);
        }
    }
}