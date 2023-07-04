using System.Drawing;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using TonSdk.Core.Boc;
using TonSdk.Core.Crypto;

namespace TonSdk.Client;

public struct JettonContent
{
    public string? Uri;
    public string? Name;
    public string? Description;
    public string? Image;
    public string? ImageData;
    public string? Symbol;
    public uint? Decimals;
}

public class JettonUtils
{
    // TODO: Metadata keys
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
        Dictionary<string, byte[]> metadataDict = new()
        {
            { "uri", Core.Crypto.Utils.HexToBytes("70e5d7b6a29b392f85076fe15ca2f2053c56c2338728c4e33c9e8ddb1ee827cc") },
            { "name", Core.Crypto.Utils.HexToBytes("82a3537ff0dbce7eec35d69edc3a189ee6f17d82f353a553f9aa96cb0be3ce89") },
            { "description", Core.Crypto.Utils.HexToBytes("c9046f7a37ad0ea7cee73355984fa5428982f8b37c8f7bcec91f7ac71a7cd104") },
            { "image", Core.Crypto.Utils.HexToBytes("6105d6cc76af400325e94d588ce511be5bfdbb73b437dc51eca43917d7a43e3d") },
            { "image_data", Core.Crypto.Utils.HexToBytes("d9a88ccec79eef59c84b671136a20ece4cd00caaad5bc47e2c208829154ee9e4") },
            { "symbol", Core.Crypto.Utils.HexToBytes("b76a7ca153c24671658335bbd08946350ffc621fa1c516e7123095d4ffd5c581") },
            { "decimals", Core.Crypto.Utils.HexToBytes("ee80fd2f1e03480e2282363596ee752d7bb27f50776b95086a0279189675923e") }
        };

        byte[] kd(Bits b) => b.ToBytes(false);

        Cell vd(Cell c) => c;

        var parsed = HashmapE<byte[], Cell>.Parse(content, new HashmapOptions<byte[], Cell>
        {
            Deserializers = new HashmapDeserializers<byte[], Cell>() { Key = kd, Value = vd }
        });

        Console.WriteLine(parsed.Count);
        

        JettonContent jettonContent = new()
        {

        };

        return jettonContent;
    }

    private static async Task<JettonContent> ParseOffChain(CellSlice content)
    {
        return new JettonContent();
    }

    enum ContentLayout
    {
        ONCHAIN = 0x00,
        OFFCHAIN = 0x01,
    }
}
