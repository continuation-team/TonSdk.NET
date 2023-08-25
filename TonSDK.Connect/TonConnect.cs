using System.Collections.Generic;
using System;
using System.Linq;
using Org.BouncyCastle.Asn1.Ocsp;
using Org.BouncyCastle.Utilities;
using System.Xml.Linq;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;

namespace TonSdk.Connect;

public delegate void StatusChangeCallback(Wallet wallet);
public delegate void StatusChangeErrorsHandler(TonConnectError error);

public class TonConnectOptions
{
    /// <summary>
    /// Url to the [manifest]{@link https://github.com/ton-connect/docs/blob/main/requests-responses.md#app-manifest} with the Dapp metadata that will be displayed in the user's wallet.
    /// If not passed, manifest from `${window.location.origin}/tonconnect-manifest.json` will be taken.
    /// </summary>
    public string ManifestUrl {get; set; }

    /// <summary>
    /// Redefine wallets list source URL.Must be a link to a json file with[following structure]{@link https://github.com/ton-connect/wallets-list}
    /// default https://raw.githubusercontent.com/ton-connect/wallets-list/main/wallets.json
    /// </summary>
    public string WalletsListSource { get; set; } = "";

    /// <summary>
    /// Wallets list cache time to live
    /// default Infinity
    /// </summary>
    public int WalletsListCacheTTLMs { get; set; } = 0;

    /// <summary>
    /// Allows to disable auto pause/unpause SSE connection on 'document.visibilitychange' event. It is not recommended to change default behaviour.
    /// </summary>
    public bool DisableAutoPauseConnection { get; set; }
}

public class TonConnect
{
    private BridgeProvider? _provider;
    private string _manifestUrl;
    private Wallet? _wallet;
    private WalletsListManager? _walletsList = new();

    private List<StatusChangeCallback> _statusChangeCallbacksSubscriptions;
    private List<StatusChangeErrorsHandler> _statusChangeErrorSubscriptions;

    /// <summary>
    /// Shows if the wallet is connected right now.
    /// </summary>
    public bool IsConnected { get => _wallet != null;}

    /// <summary>
    /// Current connected account or None if no account is connected.
    /// </summary>
    public Account Account { get => _wallet?.Account ?? null; }

    /// <summary>
    /// Current connected wallet or None if no account is connected.
    /// </summary>
    public Wallet Wallet { get => (Wallet)_wallet; }

    public TonConnect(TonConnectOptions options)
    {
        _walletsList = new WalletsListManager(options.WalletsListSource, options.WalletsListCacheTTLMs);

        _provider = null;
        _manifestUrl = options.ManifestUrl;

        _wallet = null;

        _statusChangeCallbacksSubscriptions = new();
        _statusChangeErrorSubscriptions = new();
    }

    /// <summary>
    /// Return available wallets list.
    /// </summary>
    /// <param name="current">Set false, if you want to get dafault wallet list, true - current TonConnect instance</param>
    /// <returns>WalletConfig array</returns>
    public WalletConfig[] GetWallets(bool current = true) => current ? _walletsList.GetWallets().ToArray() : new WalletsListManager().GetWallets().ToArray();

    /// <summary>
    /// Allows to subscribe to connection status changes and handle connection errors.
    /// </summary>
    /// <param name="callback">Callback will be called after connections status changes with actual wallet or null.</param>
    /// <param name="errorsHandler">ErrorsHandler (optional) will be called with some instance of TonConnectError when connect error is received.</param>
    /// <returns>Unsubscribe callback.</returns>
    public Action OnStatusChange(StatusChangeCallback callback, StatusChangeErrorsHandler? errorsHandler = null)
    {
        _statusChangeCallbacksSubscriptions.Add(callback);
        if(errorsHandler != null) _statusChangeErrorSubscriptions.Add(errorsHandler);

        void unsubscribe()
        {
            if (_statusChangeCallbacksSubscriptions.Contains(callback)) _statusChangeCallbacksSubscriptions.Remove(callback);
            if (errorsHandler != null && _statusChangeErrorSubscriptions.Contains(errorsHandler)) _statusChangeErrorSubscriptions.Remove(errorsHandler);
        }

        return unsubscribe;
    }

    /// <summary>
    /// Generates universal link for an external wallet and subscribes to the wallet's bridge, or sends connect request to the injected wallet.
    /// </summary>
    /// <param name="walletConfig">Wallet wallet's bridge url and universal link for an external wallet or jsBridge key for the injected wallet.</param>
    /// <param name="connectAdditionalRequest">Request (optional) additional request to pass to the wallet while connect (currently only ton_proof is available).</param>
    /// <returns>Universal link if external wallet was passed or void for the injected wallet.</returns>
    /// <exception cref="TonConnectError">Wallet already connected</exception>
    public async Task<string> Connect(WalletConfig walletConfig, ConnectAdditionalRequest? connectAdditionalRequest = null)
    {
        if (IsConnected) throw new TonConnectError("Wallet already connected");
        _provider?.CloseConnection();
        _provider = CreateProvider(walletConfig);
        return await _provider.ConnectAsync(CreateConnectRequest(connectAdditionalRequest));
    }

    private BridgeProvider CreateProvider(WalletConfig walletConfig)
    {
        BridgeProvider provider = new BridgeProvider(walletConfig);
        provider.Listen(new WalletEventListener(WalletEventsListener));
        return provider;
    }

    private void WalletEventsListener(dynamic eventData)
    {
        Console.WriteLine(eventData);
        switch ((string)eventData.@event)
        {
            case "connect":
                {
                    OnWalletConnected(eventData.payload);
                    break;
                }
            case "connect_error":
                {
                    break;
                }
            case "disconnect":
                {
                    OnWalletDisconnected();
                    break;
                }
        }
    }

    private void OnWalletConnected(dynamic payload)
    {
        Wallet wallet = ConnectEventParser.ParseResponse(payload);
        foreach(StatusChangeCallback listener in _statusChangeCallbacksSubscriptions)
        {
            listener(wallet);
        }
    }


//    private onWalletConnected(connectEvent: ConnectEventSuccess['payload']) : void {
//        const tonAccountItem: TonAddressItemReply | undefined = connectEvent.items.find(
//            item => item.name === 'ton_addr'
//        ) as TonAddressItemReply | undefined;

//        const tonProofItem: TonProofItemReply | undefined = connectEvent.items.find(
//            item => item.name === 'ton_proof'
//        ) as TonProofItemReply | undefined;

//        if (!tonAccountItem) {
//            throw new TonConnectError('ton_addr connection item was not found');
//    }

//    const wallet: Wallet = {
//                device: connectEvent.device,
//                provider: this.provider!.type,
//                account: {
//                    address: tonAccountItem.address,
//    chain: tonAccountItem.network,
//                    walletStateInit: tonAccountItem.walletStateInit,
//                    publicKey: tonAccountItem.publicKey
//                }
//            };

//    if (tonProofItem)
//    {
//        wallet.connectItems = {
//        tonProof: tonProofItem
//                };
//    }

//this.wallet = wallet;
//    }


    private void OnWalletDisconnected() => _wallet = null;


    private ConnectRequest CreateConnectRequest(ConnectAdditionalRequest? connectAdditionalRequest = null)
    {
        ConnectRequest connectRequest = new();
        connectRequest.manifestUrl = _manifestUrl;
        List<IConnectItem> connectItems = new()
        {
            new ConnectAddressItem() { name = "ton_addr" }
        };
        if (connectAdditionalRequest != null && connectAdditionalRequest?.TonProof != null && connectAdditionalRequest?.TonProof != "") 
        {
            connectItems.Add(new ConnectProofItem() { name = "ton_proof", payload = connectAdditionalRequest?.TonProof });
        }
        connectRequest.items = connectItems.ToArray();
        return connectRequest;
    }
}
