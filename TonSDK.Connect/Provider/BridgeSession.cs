namespace TonSdk.Connect;

public struct BridgeSession
{
    public CryptedSessionInfo? CryptedSessionInfo { get; private set; }
    public string? WalletPublicKey { get; private set; }
    public string? BridgeUrl { get; private set; }

    public BridgeSession(SessionInfo sessionInfo)
    {
        CryptedSessionInfo = sessionInfo.SessionPrivateKey != null ? new(sessionInfo.SessionPrivateKey) : null;
        WalletPublicKey = sessionInfo.WalletPublicKey ?? null;
        BridgeUrl = sessionInfo.BridgeUrl ?? null;
    }
}
