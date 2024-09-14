using System.Numerics;
using System.Security.Cryptography;
using NUnit.Framework;
using TonSdk.Adnl.TL;

namespace TonSdk.Adnl.Tests;

public class TLWriteBufferTests
{
    [Test]
    public void Test_Int256FromRandomBytesCanBeEncoded()
    {
        // There is 1/256 chance that last byte will be 0 (biginteger is encoded as little-endian)
        // Check x100 samples
        for (var i = 0; i < 256 * 100; ++i)
        {
            var randomBytes = GenerateRandomBytes(32);

            var queryId = new BigInteger(randomBytes);

            EnsureCanBeWrittenAndReadBack(queryId);
        }
    }

    [Test]
    public void Test_Int256WithZeroLastByteCanBeWritten()
    {
        var bytes = new byte[32];
        for (var i = 0; i < 31; ++i)
        {
            bytes[i] = (byte)i;
        }

        bytes[31] = 0;

        var queryId = new BigInteger(bytes);

        EnsureCanBeWrittenAndReadBack(queryId);
    }

    private static void EnsureCanBeWrittenAndReadBack(BigInteger value)
    {
        var writeBuffer = new TLWriteBuffer();
        writeBuffer.WriteInt256(value);

        var writtenBytes = writeBuffer.Build();
        var restoredValue = new BigInteger(writtenBytes);

        Assert.That(restoredValue, Is.EqualTo(value));
    }

    // AdnlKeys generation logic
    private static byte[] GenerateRandomBytes(int byteSize)
    {
        using RandomNumberGenerator randomNumberGenerator = RandomNumberGenerator.Create();
        byte[] randomBytes = new byte[byteSize];
        randomNumberGenerator.GetBytes(randomBytes);
        return randomBytes;
    }
}