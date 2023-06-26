using TonSdk.Core.Crypto;

namespace TonSdk.Core.Tests;

public class MnemonicBIP39Test
{
    [Test]
    public void Test_ConstructorMnemonicBIP39Exceptions()
    {
        Assert.DoesNotThrow(() => new MnemonicBIP39());
        Assert.DoesNotThrow(() => new MnemonicBIP39(null));
        Assert.DoesNotThrow(() => new MnemonicBIP39(null, new MnemonicOptions("", 2048, 32)));

        Assert.Throws<Exception>(() => new MnemonicBIP39(new string[] { "abandon", "abandon", "abandon" }), "Mnemonic: must contain 24 bip39 words.");
        Assert.Throws<Exception>(() => new MnemonicBIP39(new string[]
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
    public void Test_ConstructorMnemonicBIP39()
    {
        Assert.That(new MnemonicBIP39().Words.Length, Is.EqualTo(24));
        Assert.That(new MnemonicBIP39().Seed.Length, Is.EqualTo(32));
        Assert.That(new MnemonicBIP39().Keys.PrivateKey.Length, Is.EqualTo(32));
        Assert.That(new MnemonicBIP39().Keys.PublicKey.Length, Is.EqualTo(32));

        // All tests have been checked with https://bip39.net
        MnemonicBIP39 mnemonicBIP39 = new MnemonicBIP39(new string[]{
        "album", "satoshi", "ginger",
        "erode", "trial", "say",
        "modify", "shield", "shove",
        "hunt", "tissue", "sound",
        "fine", "tonight", "piece",
        "unfair", "mom", "island",
        "toss", "annual", "trick",
        "barrel", "major", "february" });

        Assert.That(Utils.BytesToHex(mnemonicBIP39.Seed).ToLower(), Is.EqualTo("aee2919303fcdc0d66f8321c13dc749dfd16147069b7d06a3a3d0149eb165ec6"));
        Assert.That(Utils.BytesToHex(mnemonicBIP39.Keys.PublicKey).ToLower(), Is.EqualTo("c2c8e409c14d2a23ea6563fcd9dd5f17c4b995f0c785e2c2f32b9d074eafad2f"));
        Assert.That(Utils.BytesToHex(mnemonicBIP39.Keys.PrivateKey).ToLower(), Is.EqualTo("aee2919303fcdc0d66f8321c13dc749dfd16147069b7d06a3a3d0149eb165ec6"));
    }

    [Test]
    public void Test_GenerateWords()
    {
        Assert.DoesNotThrow(() => MnemonicBIP39.GenerateWords());
        Assert.That(MnemonicBIP39.GenerateWords().Length, Is.EqualTo(24));
        Assert.That(MnemonicBIP39.GenerateWords().All(word => MnemonicWords.Bip0039En.Contains(word)), Is.EqualTo(true));
    }

    [Test]
    public void Test_GenerateKeyPair()
    {
        KeyPair mnemonicKeyPair = null;
        Assert.DoesNotThrow(() => mnemonicKeyPair = MnemonicBIP39.GenerateKeyPair(Utils.HexToBytes("aee2919303fcdc0d66f8321c13dc749dfd16147069b7d06a3a3d0149eb165ec6")));
        Assert.That(Utils.BytesToHex(mnemonicKeyPair.PublicKey).ToLower(), Is.EqualTo("c2c8e409c14d2a23ea6563fcd9dd5f17c4b995f0c785e2c2f32b9d074eafad2f"));
        Assert.That(Utils.BytesToHex(mnemonicKeyPair.PrivateKey).ToLower(), Is.EqualTo("aee2919303fcdc0d66f8321c13dc749dfd16147069b7d06a3a3d0149eb165ec6"));
    }

    [Test]
    public void Test_GenerateSeed()
    {
        Assert.Throws<Exception>(() => MnemonicBIP39.GenerateSeed(new string[] { "abandon", "abandon", "abandon" }), "Mnemonic: must contain 24 bip39 words.");
        Assert.Throws<Exception>(() => MnemonicBIP39.GenerateSeed(new string[]
        { "ton", "testton", "testtoncat",
          "ton", "testton", "testtoncat",
          "ton", "testton", "testtoncat",
          "ton", "testton", "testtoncat",
          "ton", "testton", "testtoncat",
          "ton", "testton", "testtoncat",
          "ton", "testton", "testtoncat",
          "ton", "testton", "testtoncat" }), "Mnemonic: invalid mnemonic phrase words.");

        byte[]? seed = null;
        Assert.DoesNotThrow(() => seed = MnemonicBIP39.GenerateSeed(new string[]{
        "album", "satoshi", "ginger",
        "erode", "trial", "say",
        "modify", "shield", "shove",
        "hunt", "tissue", "sound",
        "fine", "tonight", "piece",
        "unfair", "mom", "island",
        "toss", "annual", "trick",
        "barrel", "major", "february" }));
        Assert.That(Utils.BytesToHex(seed).ToLower(), Is.EqualTo("aee2919303fcdc0d66f8321c13dc749dfd16147069b7d06a3a3d0149eb165ec6"));
    }
}
