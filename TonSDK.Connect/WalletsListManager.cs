using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace TonSdk.Connect
{
    public struct WalletConfig
    {
        public string Name { get; set; }
        public string Image { get; set; }
        public string AboutUrl { get; set; }
        public string BridgeUrl { get; set; }
        public string JsBridgeKey { get; set; }
        public string UniversalUrl { get; set; }
        public string AppName { get; set; }
    }

    internal class WalletsListManager
    {
        private string walletsListSource = "https://raw.githubusercontent.com/ton-blockchain/wallets-list/main/wallets-v2.json";
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

        internal WalletsListManager(string walletsListSource = null, int cacheTtl = 0)
        {
            if (walletsListSource != null && walletsListSource != "")
                this.walletsListSource = walletsListSource;

            this.cacheTtl = cacheTtl;
            this.walletsListCache = null;
            this.walletsListCacheCreationTimestamp = 0;
        }

        public List<WalletConfig> GetWallets(bool includeInjected = false)
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

                for(int i = 0; i < walletsList.Count; i++)
                {
                    if (walletsList[i] == null)
                    {
                        Console.WriteLine("Not supported wallet: is not a dictionary -> " + walletsList[i]);
                        continue;
                    }

                    if (!walletsList[i].ContainsKey("name") || !walletsList[i].ContainsKey("image") || !walletsList[i].ContainsKey("about_url") || !walletsList[i].ContainsKey("bridge"))
                    {
                        Console.WriteLine("Not supported wallet. Config -> " + walletsList[i]);
                        continue;
                    }

                    List<Dictionary<string, object>> bridges = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(walletsList[i]["bridge"].ToString());
                    if (bridges == null || bridges.Count == 0)
                    {
                        Console.WriteLine("Not supported wallet: bridges is not a list or len is equal 0, config -> " + walletsList[i]);
                        continue;
                    }

                    WalletConfig walletConfig = new WalletConfig()
                    {
                        Name = walletsList[i]["name"].ToString(),
                        Image = walletsList[i]["image"].ToString(),
                        AboutUrl = walletsList[i]["about_url"].ToString(),
                        AppName = walletsList[i]["app_name"].ToString()
                    };

                    foreach (Dictionary<string, object> bridge in bridges)
                    {
                        if (bridge.TryGetValue("type", out object value) && value.ToString() == "sse")
                        {
                            if (!bridge.ContainsKey("url"))
                            {
                                Console.WriteLine("Not supported wallet: bridge url not found, config -> " + walletsList[i]);
                                continue;
                            }

                            walletConfig.BridgeUrl = bridge["url"].ToString();
                            if (walletsList[i].TryGetValue("universal_url", out object urlUni)) walletConfig.UniversalUrl = urlUni.ToString();
                            if(walletConfig.JsBridgeKey != null) walletConfig.JsBridgeKey = null;
                            walletsListCache.Add(walletConfig);
                        }
                        else if(value.ToString() == "js")
                        {
                            if(includeInjected)
                            {
                                if(!bridge.ContainsKey("key"))
                                {
                                    Console.WriteLine("Not supported wallet: bridge key not found, config -> " + walletsList[i]);
                                    continue;
                                }
                                walletConfig.JsBridgeKey = bridge["key"].ToString();
                                if(walletConfig.BridgeUrl != null) walletConfig.BridgeUrl = null;
                                walletsListCache.Add(walletConfig);
                            }
                        }
                    }

                    if (walletConfig.BridgeUrl == null && walletConfig.JsBridgeKey == null) continue;
                }

                walletsListCacheCreationTimestamp = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            }

            return walletsListCache;
        }
    }
}

