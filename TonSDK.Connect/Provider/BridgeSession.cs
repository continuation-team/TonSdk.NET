namespace TonSdk.Connect;

public class BridgeSession
{
    public CryptedSessionInfo? CryptedSessionInfo { get; set; }
    public string? WalletPublicKey { get; set; }
    public string? BridgeUrl { get; set; }

    public BridgeSession(SessionInfo? sessionInfo = null)
    {
        if (sessionInfo == null) return;
        CryptedSessionInfo = sessionInfo?.SessionPrivateKey != null ? new(sessionInfo?.SessionPrivateKey) : null;
        WalletPublicKey = sessionInfo?.WalletPublicKey ?? null;
        BridgeUrl = sessionInfo?.BridgeUrl ?? null;
    }
}
