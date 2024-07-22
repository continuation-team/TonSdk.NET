using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net.Http;
using System.Numerics;
using System.Threading.Tasks;
using TonSdk.Core.Boc;
using TonSdk.Core.Crypto;

namespace TonSdk.Client
{
    public class JettonContent
    {
        [JsonProperty("uri")] public string Uri;
        [JsonProperty("name")] public string Name;
        [JsonProperty("description")] public string Description;
        [JsonProperty("image")] public string Image;
        [JsonProperty("image_data")] public string ImageData;
        [JsonProperty("symbol")] public string Symbol;
        [JsonProperty("decimals")] public uint Decimals = 9;

        public JettonContent(Dictionary<string, string> valueDict)
        {
            valueDict.TryGetValue("uri", out Uri);
            valueDict.TryGetValue("name", out Name);
            valueDict.TryGetValue("description", out Description);
            valueDict.TryGetValue("image", out Image);
            valueDict.TryGetValue("image_data", out ImageData);
            valueDict.TryGetValue("symbol", out Symbol);
            valueDict.TryGetValue("decimals", out string dec);
            Decimals = dec != null ? uint.Parse(dec) : 9;
        }

        public JettonContent(JettonUtils.OutJettonOffContent outJettonOffContent)
        {
            Uri = outJettonOffContent.Uri ?? null;
            Name = outJettonOffContent.Name ?? null;
            Description = outJettonOffContent.Description ?? null;
            Image = outJettonOffContent.Image ?? null;
            ImageData = outJettonOffContent.ImageData ?? null;
            Symbol = outJettonOffContent.Symbol ?? null;
            Decimals = outJettonOffContent.Decimals == null ? 9 : uint.Parse(outJettonOffContent.Decimals);
        }
    }

    public class JettonUtils
    {
        public struct OutJettonOffContent
        {
            [JsonProperty("uri")] public string Uri;
            [JsonProperty("name")] public string Name;
            [JsonProperty("description")] public string Description;
            [JsonProperty("image")] public string Image;
            [JsonProperty("image_data")] public string ImageData;
            [JsonProperty("symbol")] public string Symbol;
            [JsonProperty("decimals")] public string Decimals;
        }

        public static async Task<JettonContent> ParseMetadata(Cell content)
        {
            CellSlice ds = content.Parse();
            if (ds.Bits.Length < 8) throw new Exception("Invalid metadata");

            var contentLayout = (ContentLayout)(uint)ds.LoadUInt(8);

            return contentLayout switch
            {
                ContentLayout.ONCHAIN => await ParseOnChain(ds),
                ContentLayout.OFFCHAIN => await ParseOffChain(ds),
                _ => throw new Exception("Invalid metadata prefix"),
            };
        }

        private static async Task<JettonContent> ParseOnChain(CellSlice content)
        {
            Dictionary<string, byte[]> metadataDict = new Dictionary<string, byte[]>()
        {
            { "uri", Utils.HexToBytes("70e5d7b6a29b392f85076fe15ca2f2053c56c2338728c4e33c9e8ddb1ee827cc") },
            { "name", Utils.HexToBytes("82a3537ff0dbce7eec35d69edc3a189ee6f17d82f353a553f9aa96cb0be3ce89") },
            { "description", Utils.HexToBytes("c9046f7a37ad0ea7cee73355984fa5428982f8b37c8f7bcec91f7ac71a7cd104") },
            { "image", Utils.HexToBytes("6105d6cc76af400325e94d588ce511be5bfdbb73b437dc51eca43917d7a43e3d") },
            { "image_data", Utils.HexToBytes("d9a88ccec79eef59c84b671136a20ece4cd00caaad5bc47e2c208829154ee9e4") },
            { "symbol", Utils.HexToBytes("b76a7ca153c24671658335bbd08946350ffc621fa1c516e7123095d4ffd5c581") },
            { "decimals", Utils.HexToBytes("ee80fd2f1e03480e2282363596ee752d7bb27f50776b95086a0279189675923e") }
        };

            var dict = content.LoadDict(new HashmapOptions<byte[], Cell>
            {
                KeySize = 256,
                Deserializers = new HashmapDeserializers<byte[], Cell>
                {
                    Key = b => b.ToBytes(),
                    Value = c => c
                },
                Serializers = new HashmapSerializers<byte[], Cell>
                {
                    Key = b => new Bits(b),
                    Value = c => c
                }
            });

            Dictionary<string, string> dataDict = new Dictionary<string, string>();

            foreach (var kv in metadataDict)
            {
                var valueCell = dict.Get(kv.Value);
                if (valueCell != null) dataDict.Add(kv.Key, valueCell!.Parse().LoadRef().Parse().SkipBits(8).LoadString());
            }

            JettonContent jettonContent = new JettonContent(dataDict);
            if (!string.IsNullOrEmpty(jettonContent.Uri)) jettonContent = await ParseOffChainUri(jettonContent);
            return jettonContent;
        }

        private static async Task<JettonContent> ParseOffChainUri(JettonContent jettonContent)
        {
            string url = jettonContent.Uri;
            if (url.StartsWith("ipfs://")) url = "https://ipfs.io/ipfs/" + url.Substring(7);
            HttpClient httpClient = new HttpClient();
            HttpResponseMessage response = await httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode) throw new Exception($"Received error: {await response.Content.ReadAsStringAsync()}");
            string result = await response.Content.ReadAsStringAsync();

            OutJettonOffContent offJettonContent = JsonConvert.DeserializeObject<OutJettonOffContent>(result);
            jettonContent.Description ??= offJettonContent.Description;
            jettonContent.Name ??= offJettonContent.Name;
            jettonContent.ImageData ??= offJettonContent.ImageData;
            jettonContent.Image ??= offJettonContent.Image;
            jettonContent.Symbol ??= offJettonContent.Symbol;
            return jettonContent ?? throw new Exception("Parse metadata error.");
        }

        private static async Task<JettonContent> ParseOffChain(CellSlice content)
        {
            string jsonUrl = content.LoadString();
            if (string.IsNullOrEmpty(jsonUrl))
            {
                while (content.Refs.Length != 0)
                {
                    var nextRef = content.LoadRef();
                    content = nextRef.Parse();
                    jsonUrl = content.LoadString();
                }
            }
            
            if (jsonUrl.StartsWith("ipfs://")) 
                jsonUrl = "https://ipfs.io/ipfs/" + jsonUrl.Substring(7);
            HttpClient httpClient = new HttpClient();
            HttpResponseMessage response = await httpClient.GetAsync(jsonUrl);

            if (!response.IsSuccessStatusCode) throw new Exception($"Received error: {await response.Content.ReadAsStringAsync()}");
            string result = await response.Content.ReadAsStringAsync();

            OutJettonOffContent offJettonContent = JsonConvert.DeserializeObject<OutJettonOffContent>(result);
            JettonContent jettonContent = new JettonContent(offJettonContent);
            return jettonContent ?? throw new Exception("Parse metadata error.");
        }

        enum ContentLayout
        {
            ONCHAIN = 0x00,
            OFFCHAIN = 0x01,
        }
    }
}
