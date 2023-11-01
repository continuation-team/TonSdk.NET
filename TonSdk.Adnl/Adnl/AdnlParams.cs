using System.Security.Cryptography;

namespace TonSdk.Adnl;

public class AdnlAesParams
{
    private byte[] _bytes = AdnlKeys.GenerateRandomBytes(160);
    
    public byte[] Bytes => _bytes;
    
    public byte[] RxKey => _bytes.Skip(0).Take(32).ToArray();
    
    public byte[] TxKey => _bytes.Skip(32).Take(64).ToArray();
    
    public byte[] RxNonce => _bytes.Skip(64).Take(80).ToArray();
    
    public byte[] TxNonce => _bytes.Skip(80).Take(96).ToArray();
    
    public byte[] Padding => _bytes.Skip(96).Take(160).ToArray();
    
    public byte[] Hash => SHA256.HashData(_bytes);
}