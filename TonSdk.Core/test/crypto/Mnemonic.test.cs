using TonSdk.Core.Crypto;

namespace TonSdk.Core.Tests;

public class MnemonicTest
{
    [Test]
    public void Test_ConstructrorMnemonicExceptions()
    {
        Assert.DoesNotThrow(() => new Mnemonic());
        Assert.DoesNotThrow(() => new Mnemonic(null));

        Assert.Throws<Exception>(() => new Mnemonic(Array.Empty<string>()), "Mnemonic: must contain 24 bip39 words.");
        Assert.Throws<Exception>(() => new Mnemonic(new string[] { "abandon", "abandon", "abandon" }), "Mnemonic: must contain 24 bip39 words.");
        Assert.Throws<Exception>(() => new Mnemonic(new string[]
        { "ton", "testton", "testtoncat",
          "ton", "testton", "testtoncat",
          "ton", "testton", "testtoncat",
          "ton", "testton", "testtoncat",
          "ton", "testton", "testtoncat",
          "ton", "testton", "testtoncat",
          "ton", "testton", "testtoncat",
          "ton", "testton", "testtoncat" }), "Mnemonic: invalid mnemonic phrase words.");
    }

    [Test]
    public void Test_ConstructrorMnemonic()
    {
        Assert.That(new Mnemonic().Words.Length, Is.EqualTo(24));
        Assert.That(new Mnemonic().Seed.Length, Is.EqualTo(32));
        Assert.That(new Mnemonic().Keys.PrivateKey.Length, Is.EqualTo(32));
        Assert.That(new Mnemonic().Keys.PublicKey.Length, Is.EqualTo(32));

        // All tests have been checked with @tonweb-mnemonic and @eversdk/ton
        Mnemonic mnemonic = new Mnemonic(new string[]{
        "album", "satoshi", "ginger",
        "erode", "trial", "say",
        "modify", "shield", "shove",
        "hunt", "tissue", "sound",
        "fine", "tonight", "piece",
        "unfair", "mom", "island",
        "toss", "annual", "trick",
        "barrel", "major", "february" });

        Assert.That(Utils.BytesToHex(mnemonic.Seed).ToLower(), Is.EqualTo("2a39b81538a65e4edbd59dfaf9078983c78e4fc024df0eec662f38d4799ff4c8"));
        Assert.That(Utils.BytesToHex(mnemonic.Keys.PublicKey).ToLower(), Is.EqualTo("63e687c02783ce5d4951d73204428a9ab465d32edde4537f5b03200a6a8e32ff"));
        Assert.That(Utils.BytesToHex(mnemonic.Keys.PrivateKey).ToLower(), Is.EqualTo("2a39b81538a65e4edbd59dfaf9078983c78e4fc024df0eec662f38d4799ff4c8"));
    }

    [Test]
    public void Test_GenerateWords()
    {
        Assert.DoesNotThrow(() => Mnemonic.GenerateWords());
        Assert.That(Mnemonic.GenerateWords().Length, Is.EqualTo(24));
        Assert.That(Mnemonic.GenerateWords().All(word => MnemonicWords.Bip0039En.Contains(word)), Is.EqualTo(true));
    }

    [Test]
    public void Test_GenerateKeyPair()
    {
        KeyPair mnemonicKeyPair = null;
        Assert.DoesNotThrow(() => mnemonicKeyPair = Mnemonic.GenerateKeyPair(Utils.HexToBytes("2a39b81538a65e4edbd59dfaf9078983c78e4fc024df0eec662f38d4799ff4c8")));
        Assert.That(Utils.BytesToHex(mnemonicKeyPair.PublicKey).ToLower(), Is.EqualTo("63e687c02783ce5d4951d73204428a9ab465d32edde4537f5b03200a6a8e32ff"));
        Assert.That(Utils.BytesToHex(mnemonicKeyPair.PrivateKey).ToLower(), Is.EqualTo("2a39b81538a65e4edbd59dfaf9078983c78e4fc024df0eec662f38d4799ff4c8"));
    }

    [Test]
    public void Test_GenerateSeed()
    {
        Assert.Throws<Exception>(() => Mnemonic.GenerateSeed(new string[] { "abandon", "abandon", "abandon" }), "Mnemonic: must contain 24 bip39 words.");
        Assert.Throws<Exception>(() => Mnemonic.GenerateSeed(new string[]
        { "ton", "testton", "testtoncat",
          "ton", "testton", "testtoncat",
          "ton", "testton", "testtoncat",
          "ton", "testton", "testtoncat",
          "ton", "testton", "testtoncat",
          "ton", "testton", "testtoncat",
          "ton", "testton", "testtoncat",
          "ton", "testton", "testtoncat" }), "Mnemonic: invalid mnemonic phrase words.");

        byte[]? seed = null;
        Assert.DoesNotThrow(() => seed = Mnemonic.GenerateSeed(new string[]{
        "album", "satoshi", "ginger",
        "erode", "trial", "say",
        "modify", "shield", "shove",
        "hunt", "tissue", "sound",
        "fine", "tonight", "piece",
        "unfair", "mom", "island",
        "toss", "annual", "trick",
        "barrel", "major", "february" }));
        Assert.That(Utils.BytesToHex(seed).ToLower(), Is.EqualTo("2a39b81538a65e4edbd59dfaf9078983c78e4fc024df0eec662f38d4799ff4c8"));
    }
}
