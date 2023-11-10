using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using System;

public class Cipher
{
    private readonly IBufferedCipher _cipher;
    
    public Cipher(byte[] key, byte[] iv)
    {
        if (key.Length != 32)
            throw new ArgumentException("Invalid key length. Key must be 256 bits.");

        if (iv.Length != 16)
            throw new ArgumentException("Invalid IV length. IV must be 128 bits.");

        _cipher = new BufferedBlockCipher(new SicBlockCipher(new AesEngine()));
        _cipher.Init(true, new ParametersWithIV(new KeyParameter(key), iv));
    }

    public byte[] Update(byte[] data)
    {
        byte[] output = new byte[_cipher.GetOutputSize(data.Length)];
        int length = _cipher.ProcessBytes(data, 0, data.Length, output, 0);
        _cipher.DoFinal(output, length);
        return output;
    }
}

public class Decipher
{ 
    private readonly IBufferedCipher _cipher;
    public Decipher(byte[] key, byte[] iv)
    {
        if (key.Length != 16 && key.Length != 24 && key.Length != 32)
            throw new ArgumentException("Invalid key length. Key must be 128, 192, or 256 bits.");

        if (iv.Length != 16)
            throw new ArgumentException("Invalid IV length. IV must be 128 bits.");

        _cipher = new BufferedBlockCipher(new SicBlockCipher(new AesEngine()));
        _cipher.Init(false, new ParametersWithIV(new KeyParameter(key), iv));
    }

    public byte[] Update(byte[] data)
    {
        byte[] output = new byte[_cipher.GetOutputSize(data.Length)];
        int length = _cipher.ProcessBytes(data, 0, data.Length, output, 0);
        _cipher.DoFinal(output, length);
        return output;
    }
}

public static class CipherFactory
{
    public static Cipher CreateCipheriv(byte[] key, byte[] iv) => new Cipher(key, iv);

    public static Decipher CreateDecipheriv(byte[] key, byte[] iv) => new Decipher(key, iv);
}
