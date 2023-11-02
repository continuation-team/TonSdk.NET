using System.Security.Cryptography;
using Org.BouncyCastle.Tls;

namespace TonSdk.Adnl;

public class AdnlPacket
{
    public const byte PacketMinSize = 68; // 4 (size) + 32 (nonce) + 32 (hash)

    private byte[] _payload;
    private byte[] _nonce;

    public AdnlPacket(byte[] payload, byte[]? nonce = null)
    {
        _nonce = nonce ?? AdnlKeys.GenerateRandomBytes(32);
        _payload = payload;
    }

    public byte[] Payload => _payload;
    
    public byte[] Nonce => _nonce;
    
    public byte[] Hash => SHA256.HashData(_nonce.Concat(_payload).ToArray());
    
    public byte[] Size {
        get {
            int size = _payload.Length + 32 + 32;
            byte[] buffer = BitConverter.GetBytes(size);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(buffer);
            }
            return buffer;
        }
    }
    
    public byte[] Data => Size.Concat(Nonce).Concat(Payload).Concat(Hash).ToArray();

    public int Length => PacketMinSize + _payload.Length;

    public static AdnlPacket? Parse(byte[] data)
    {
        if (data.Length < 4) return null;
        int cursor = 0;
        
        uint size = BitConverter.ToUInt32(data, cursor);
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
        
        byte[] target = SHA256.HashData(nonce.Concat(payload).ToArray());

        if (!hash.SequenceEqual(target)) throw new Exception("ADNLPacket: Bad packet hash.");

        return new AdnlPacket(payload, nonce);
    }
}