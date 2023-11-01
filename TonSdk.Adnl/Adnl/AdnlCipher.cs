using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;

namespace TonSdk.Adnl;

public class CipherBase
{
    protected IBufferedCipher cipher;

    public CipherBase(byte[] key, byte[] iv, bool forEncryption)
    {
        cipher = new BufferedBlockCipher(new SicBlockCipher(new AesEngine()));
        KeyParameter keyParam = new KeyParameter(key);
        ParametersWithIV keyWithIV = new ParametersWithIV(keyParam, iv);
        cipher.Init(forEncryption, keyWithIV);
    }

    public byte[] Final()
    {
        return Array.Empty<byte>();
    }
}

public class Cipher : CipherBase
{
    public Cipher(byte[] key, byte[] iv) : base(key, iv, true) { }

    public byte[] Update(byte[] data)
    {
        return cipher.DoFinal(data);
    }
}

public class Decipher : CipherBase
{
    public Decipher(byte[] key, byte[] iv) : base(key, iv, false) { }

    public byte[] Update(byte[] data)
    {
        return cipher.DoFinal(data);
    }
}

public static class CryptoFactory
{
    public static Cipher CreateCipheriv(byte[] key, byte[] iv)
    {
        return new Cipher(key, iv);
    }

    public static Decipher CreateDecipheriv(byte[] key, byte[] iv)
    {
        return new Decipher(key, iv);
    }
}