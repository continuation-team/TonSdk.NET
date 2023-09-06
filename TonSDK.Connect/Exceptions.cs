
using System;

namespace TonSdk.Connect
{
    public class TonConnectError : Exception
    {
        protected static string Prefix = "[TON_CONNECT_SDK_ERROR]";
        protected static string Info = null;

        public TonConnectError(string message = null) : base($"{Prefix}" + (Info != null ? $": {Info}" : "") + (message != null ? $" {message}" : "")) { }
    }

    public class WalletAlreadyConnectedError : TonConnectError
    {
        protected static new string Info = "Wallet connection called but wallet already connected. To avoid the error, disconnect the wallet before doing a new connection.";
    }

    public class WalletNotConnectedError : TonConnectError
    {
        protected static new string Info = "Send transaction or other protocol methods called while wallet is not connected.";
    }

    public class WalletNotSupportFeatureError : TonConnectError
    {
        protected static new string Info = "Wallet doesn't support requested feature method.";
    }

    public class FetchWalletsError : TonConnectError
    {
        protected static new string Info = "An error occurred while fetching the wallets list.";
    }

    public class UnknownError : TonConnectError
    {
        protected static new string Info = "Unknown error.";
    }

    public class BadRequestError : TonConnectError
    {
        protected static new string Info = "Request to the wallet contains errors.";
    }

    public class UnknownAppError : TonConnectError
    {
        protected static new string Info = "App tries to send rpc request to the injected wallet while not connected.";
    }

    public class UserRejectsError : TonConnectError
    {
        protected static new string Info = "User rejects the action in the wallet.";
    }

    public class ManifestNotFoundError : TonConnectError
    {
        protected static new string Info = "Manifest not found. Make sure you added `tonconnect-manifest.json` to the root of your app or passed correct manifest_url. See more https://github.com/ton-connect/docs/blob/main/requests-responses.md#app-manifest";
    }

    public class ManifestContentError : TonConnectError
    {
        protected static new string Info = "Passed `tonconnect-manifest.json` contains errors. Check format of your manifest. See more https://github.com/ton-connect/docs/blob/main/requests-responses.md#app-manifest";
    }

    public class WalletNotInjectedError : TonConnectError
    {
        protected static new string Info = "There is an attempt to connect to the injected wallet while it is not exists in the webpage.";
    }
}
