using System.Security.Cryptography;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Agreement;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;

namespace TonSdk.Adnl;

public class AdnlKeys
{
    private byte[] _peer;
    private byte[] _public;
    private byte[] _shared;

    public AdnlKeys(byte[] peerPublicKey)
    {
        _peer = peerPublicKey;
    }

    public byte[] Public() => _public;
    public byte[] Shared() => _shared;

    public void Generate()
    {
        byte[] privateKey = GenerateRandomBytes(32);
        X25519PrivateKeyParameters privateKeyParams = new X25519PrivateKeyParameters(privateKey, 0); 
        X25519PublicKeyParameters publicKey = privateKeyParams.GeneratePublicKey();
        byte[] sharedSecret = new byte[32];
        privateKeyParams.GenerateSecret(publicKey, sharedSecret, 0);
        _shared = sharedSecret;
        _public = publicKey.GetEncoded();
    }
    
    private static byte[] GenerateRandomBytes(int byteSize)
    {
        using RandomNumberGenerator randomNumberGenerator = RandomNumberGenerator.Create();
        byte[] randomBytes = new byte[byteSize];
        randomNumberGenerator.GetBytes(randomBytes);
        return randomBytes;
    }
}