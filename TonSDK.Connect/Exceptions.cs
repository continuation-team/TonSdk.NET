
using System;

namespace TonSdk.Connect
{
    public class TonConnectError : Exception
    {
        private const string Prefix = "[TON_CONNECT_SDK_ERROR]";

        public TonConnectError(string message = null, string info = null) 
            : base($"{Prefix}" + (info != null ? $": {info}" : "") + (message != null ? $" {message}" : "")) { }
    }

    public class WalletAlreadyConnectedError : TonConnectError
    {
        public WalletAlreadyConnectedError(string message = null) 
            : base(message, "Wallet connection called but wallet already connected. To avoid the error, disconnect the wallet before doing a new connection.") { }
    }

    public class WalletNotConnectedError : TonConnectError
    {
        public WalletNotConnectedError(string message = null) 
            : base(message, "Send transaction or other protocol methods called while wallet is not connected.") { }
    }

    public class WalletNotSupportFeatureError : TonConnectError
    {
        public WalletNotSupportFeatureError(string message = null) 
            : base(message, "Wallet doesn't support requested feature method.") { }
    }

    public class FetchWalletsError : TonConnectError
    {
        public FetchWalletsError(string message = null) 
            : base(message, "An error occurred while fetching the wallets list.") { }
    }

    public class UnknownError : TonConnectError
    {
        public UnknownError(string message = null) 
            : base(message, "Unknown error.") { }
    }

    public class BadRequestError : TonConnectError
    {
        public BadRequestError(string message = null) 
            : base(message, "Request to the wallet contains errors.") { }
    }

    public class UnknownAppError : TonConnectError
    {
        public UnknownAppError(string message = null) 
            : base(message, "App tries to send rpc request to the injected wallet while not connected.") { }
    }

    public class UserRejectsError : TonConnectError
    {
        public UserRejectsError(string message = null) 
            : base(message, "User rejects the action in the wallet.") { }
    }

    public class ManifestNotFoundError : TonConnectError
    {
        public ManifestNotFoundError(string message = null) 
            : base(message, "Manifest not found." +
            " Make sure you added `tonconnect-manifest.json` to the root of your app or passed correct manifest_url." +
            " See more https://github.com/ton-connect/docs/blob/main/requests-responses.md#app-manifest.") { }
    }

    public class ManifestContentError : TonConnectError
    {
        public ManifestContentError(string message = null) 
            : base(message, "Passed `tonconnect-manifest.json` contains errors. Check format of your manifest. See more https://github.com/ton-connect/docs/blob/main/requests-responses.md#app-manifest.") { }
    }

    public class WalletNotInjectedError : TonConnectError
    {
        public WalletNotInjectedError(string message = null) 
            : base(message, "There is an attempt to connect to the injected wallet while it is not exists in the webpage.") { }
    }
}
