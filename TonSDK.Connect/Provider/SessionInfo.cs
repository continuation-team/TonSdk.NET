using System.Security.Cryptography;
using TonSdk.Core.Crypto;

namespace TonSdk.Connect;

public struct CryptedSessionInfo
{
    public KeyPair KeyPair { get; private set; }
    public string SesionId { get; private set; }


    public CryptedSessionInfo(string? seed = null)
    {
        byte[] seedBytes = seed == null ? GenerateRandomBytes(32) : Utils.HexToBytes(seed);
        KeyPair = Utils.GenerateKeyPair(seedBytes);
        SesionId = Utils.BytesToHex(KeyPair.PublicKey);
    }

    private static byte[] GenerateRandomBytes(int byteSize)
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