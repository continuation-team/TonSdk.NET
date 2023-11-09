﻿using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Security;
using System.Numerics;
using System.Security.Cryptography;

namespace TonSdk.Adnl;

public class DiffieHellman
{
    private static readonly BigInteger Ed25519P = BigInteger.Parse("57896044618658097711785492504343953926634992332820282019728792003956564819949");
    
    private Ed25519PrivateKeyParameters _privateKey;
    private Ed25519PublicKeyParameters _publicKey;

    public DiffieHellman()
    {
        var keyPairGenerator = new Ed25519KeyPairGenerator();
        keyPairGenerator.Init(new Ed25519KeyGenerationParameters(new SecureRandom()));

        AsymmetricCipherKeyPair keyPair = keyPairGenerator.GenerateKeyPair();
        _privateKey = (Ed25519PrivateKeyParameters)keyPair.Private;
        _publicKey = (Ed25519PublicKeyParameters)keyPair.Public;
    }

    public byte[] PublicKey => _publicKey.GetEncoded();
    public byte[] PrivateKey => _privateKey.GetEncoded();
    
    private byte[] Ed25519PrivateKeyToCurve25519(byte[] privateKey)
    {
        byte[] sha512Hash = SHA512.HashData(privateKey);

        sha512Hash[0] &= 248;
        sha512Hash[31] &= 127;
        sha512Hash[31] |= 64;

        byte[] x25519PrivateKey = new byte[32];
        Array.Copy(sha512Hash, x25519PrivateKey, 32);

        return x25519PrivateKey;
    }

    private static byte[] EdwardsToMontgomeryPublicKey(byte[] publicKey)
    {
        if (publicKey == null || publicKey.Length != 32) throw new ArgumentException("Invalid public key.");
        
        byte[] yBytes = new byte[publicKey.Length];
        Array.Copy(publicKey, yBytes, publicKey.Length);
        
        yBytes[^1] &= 0b01111111; // clear sign bit
        BigInteger y = new BigInteger(yBytes, isUnsigned: true, isBigEndian: false);
        
        BigInteger montgomeryU = 
            (BigInteger.One + y) * BigInteger.ModPow(BigInteger.One - y, Ed25519P - (BigInteger.One + BigInteger.One), Ed25519P);
        montgomeryU %= Ed25519P;
        if (montgomeryU < 0)
            montgomeryU += Ed25519P;
        return montgomeryU.ToByteArray();
    }
    
    public byte[] CalculateSharedSecret(byte[] otherPublicKey)
    {
        X25519PrivateKeyParameters x25519PrivateKey =
            new X25519PrivateKeyParameters(Ed25519PrivateKeyToCurve25519(_privateKey.GetEncoded()), 0);
        X25519PublicKeyParameters x25519OtherPublicKey =
            new X25519PublicKeyParameters(EdwardsToMontgomeryPublicKey(otherPublicKey), 0);

        byte[] sharedSecret = new byte[32];
        
        x25519PrivateKey.GenerateSecret(x25519OtherPublicKey, sharedSecret, 0);

        return sharedSecret;
    }
}

public class AdnlKeys
{
    private byte[] _peer;
    private byte[] _public;
    private byte[] _shared;

    private DiffieHellman _diffieHellman;

    public AdnlKeys(byte[] peerPublicKey)
    {
        _peer = peerPublicKey;
        _diffieHellman = new DiffieHellman();
    }

    public byte[] Public => _diffieHellman.PublicKey;
    
    public byte[] Private => _diffieHellman.PrivateKey;
    public byte[] Shared => _diffieHellman.CalculateSharedSecret(_peer);
    
    internal static byte[] GenerateRandomBytes(int byteSize)
    {
        using RandomNumberGenerator randomNumberGenerator = RandomNumberGenerator.Create();
        byte[] randomBytes = new byte[byteSize];
        randomNumberGenerator.GetBytes(randomBytes);
        return randomBytes;
    }
}