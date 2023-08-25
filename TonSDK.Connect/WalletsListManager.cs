using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace TonSdk.Connect;

public struct WalletConfig
{
    public string Name { get; set; }
    public string Image { get; set; }
    public string AboutUrl { get; set; }
    public string BridgeUrl { get; set; }
    public string UniversalUrl { get; set; }
}

public class WalletsListManager
{
    private string walletsListSource = "https://raw.githubusercontent.com/ton-blockchain/wallets-list/main/wallets.json";
    private int cacheTtl;

    private List<WalletConfig> walletsListCache;
    private int walletsListCacheCreationTimestamp;
    private readonly List<Dictionary<string, object>> FALLBACK_WALLETS_LIST = new List<Dictionary<string, object>>()
    {
        new Dictionary<string, object>()
        {
            { "name", "Tonkeeper" },
            { "image", "https://tonkeeper.com/assets/tonconnect-icon.png" },
            { "tondns", "tonkeeper.ton" },
            { "about_url", "https://tonkeeper.com" },
            { "universal_url", "https://app.tonkeeper.com/ton-connect" },
            { "bridge", new List<Dictionary<string, object>>()
                {
                    new Dictionary<string, object>()
                    {
                        { "type", "sse" },
                        { "url", "https://bridge.tonapi.io/bridge" }
                    },
                    new Dictionary<string, object>()
                    {
                        { "type", "js" },
                        { "key", "tonkeeper" }
                    }
                }
            }
        },
        new Dictionary<string, object>()
        {
            { "name", "Tonhub" },
            { "image", "https://tonhub.com/tonconnect_logo.png" },
            { "about_url", "https://tonhub.com" },
            { "universal_url", "https://tonhub.com/ton-connect" },
            { "bridge", new List<Dictionary<string, object>>()
                {
                    new Dictionary<string, object>()
                    {
                        { "type", "js" },
                        { "key", "tonhub" }
                    },
                    new Dictionary<string, object>()
                    {
                        { "type", "sse" },
                        { "url", "https://connect.tonhubapi.com/tonconnect" }
                    }
                }
            }
        }
    };

    public WalletsListManager(string walletsListSource = null, int cacheTtl = 0)
    {
        if (walletsListSource != null && walletsListSource != "")
            this.walletsListSource = walletsListSource;

        this.cacheTtl = cacheTtl;
        this.walletsListCache = null;
        this.walletsListCacheCreationTimestamp = 0;
    }

    public List<WalletConfig> GetWallets()
    {
        if (cacheTtl > 0 && walletsListCacheCreationTimestamp > 0 && DateTimeOffset.UtcNow.ToUnixTimeSeconds() > walletsListCacheCreationTimestamp + cacheTtl)
            walletsListCache = null;

        if (walletsListCache == null)
        {
            List<Dictionary<string, object>> walletsList = null;
            try
            {
                using var httpClient = new HttpClient();
                var response = httpClient.GetStringAsync(walletsListSource).Result;
                walletsList = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(response);

                if (walletsList == null || walletsList.Count == 0)
                    throw new FetchWalletsError();
            }
            catch (Exception e)
            {
                Console.WriteLine("WalletsListManager get_wallets: " + e.GetType() + ": " + e.Message);
                walletsList = new List<Dictionary<string, object>>(FALLBACK_WALLETS_LIST);
            }

            walletsListCache = new List<WalletConfig>();
            foreach (Dictionary<string, object> wallet in walletsList)
            {
                WalletConfig? supportedWallet = GetSupportedWalletConfig(wallet);
                if (supportedWallet != null) walletsListCache.Add((WalletConfig)supportedWallet);
            }

            walletsListCacheCreationTimestamp = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }

        return walletsListCache;
    }

    private WalletConfig? GetSupportedWalletConfig(Dictionary<string, object> wallet)
    {
        if (wallet == null)
        {
            Console.WriteLine("Not supported wallet: is not a dictionary -> " + wallet);
            return null;
        }

        if (!wallet.ContainsKey("name") || !wallet.ContainsKey("image") || !wallet.ContainsKey("about_url") || !wallet.ContainsKey("bridge"))
        {
            Console.WriteLine("Not supported wallet. Config -> " + wallet);
            return null;
        }

        List<Dictionary<string, object>> bridges = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(wallet["bridge"].ToString());
        if (bridges == null || bridges.Count == 0)
        {
            Console.WriteLine("Not supported wallet: bridges is not a list or len is equal 0, config -> " + wallet);
            return null;
        }

        WalletConfig walletConfig = new()
        {
            Name = wallet["name"].ToString(),
            Image = wallet["image"].ToString(),
            AboutUrl = wallet["about_url"].ToString(),
        };

        foreach (Dictionary<string, object> bridge in bridges)
        {
            if (bridge.TryGetValue("type", out object value) && value.ToString() == "sse")
            {
                if (!bridge.ContainsKey("url"))
                {
                    Console.WriteLine("Not supported wallet: bridge url not found, config -> " + wallet);
                    return null;
                }

                walletConfig.BridgeUrl = bridge["url"].ToString();
                if (wallet.TryGetValue("universal_url", out object url)) walletConfig.UniversalUrl = url.ToString();
                break;
            }
        }

        if (walletConfig.BridgeUrl == null || walletConfig.BridgeUrl.Length == 0) return null;
        return walletConfig;
    }
}


