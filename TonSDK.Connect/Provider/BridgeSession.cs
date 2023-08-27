using TonSdk.Core.Crypto;

namespace TonSdk.Connect
{

    public class BridgeSession
    {
        public CryptedSessionInfo? CryptedSessionInfo { get; set; }
        public string? WalletPublicKey { get; set; }
        public string? BridgeUrl { get; set; }

        public BridgeSession(SessionInfo? sessionInfo = null)
        {
            CryptedSessionInfo = sessionInfo?.SessionPrivateKey != null ? new CryptedSessionInfo(sessionInfo?.SessionPrivateKey) : null;
            WalletPublicKey = sessionInfo?.WalletPublicKey ?? null;
            BridgeUrl = sessionInfo?.BridgeUrl ?? null;
        }

        public SessionInfo GetSessionInfo()
        {
            return new SessionInfo()
            {
                SessionPrivateKey = Utils.BytesToHex(CryptedSessionInfo.KeyPair.PrivateKey),
                WalletPublicKey = WalletPublicKey,
                BridgeUrl = BridgeUrl
            };
        }
    }
}