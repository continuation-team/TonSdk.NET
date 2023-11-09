using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using System;

public abstract class CipherBase
{
    protected BufferedBlockCipher cipher;

    protected CipherBase(byte[] key, byte[] iv)
    {
        KeyParameter keyParam = new KeyParameter(key);
        ICipherParameters parameters = new ParametersWithIV(keyParam, iv);

        // CTR mode in Bouncy Castle
        cipher = new BufferedBlockCipher(new SicBlockCipher(new AesEngine()));
        cipher.Init(true, parameters);
    }

    public byte[] Final()
    {
        return new byte[0];
    }
}

public class Cipher : CipherBase
{
    public Cipher(byte[] key, byte[] iv) : base(key, iv)
    {
    }

    public byte[] Update(byte[] data)
    {
        byte[] result = new byte[cipher.GetOutputSize(data.Length)];
        int length = cipher.ProcessBytes(data, 0, data.Length, result, 0);
        cipher.DoFinal(result, length); // Finalize the encryption operation
        return result;
    }
}

public class Decipher : Cipher
{
    public Decipher(byte[] key, byte[] iv) : base(key, iv)
    {
        cipher.Init(false, new ParametersWithIV(new KeyParameter(key), iv)); // Initialize for decryption
    }

    public new byte[] Update(byte[] data)
    {
        byte[] result = new byte[cipher.GetOutputSize(data.Length)];
        int length = cipher.ProcessBytes(data, 0, data.Length, result, 0);
        cipher.DoFinal(result, length); // Finalize the decryption operation
        return result;
    }
}

public static class CipherFactory
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