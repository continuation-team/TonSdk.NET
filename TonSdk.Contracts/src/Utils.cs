using System;
using System.Collections.Generic;
using System.Text;
using Org.BouncyCastle.Math;
using TonSdk.Contracts.Jetton;
using TonSdk.Core.Boc;
using TonSdk.Core.Crypto;

namespace TonSdk.Contracts {
    public static class SmcUtils 
    {
        public const uint ONCHAIN_CONTENT_PREFIX = 0x00;
        public const uint OFFCHAIN_CONTENT_PREFIX = 0x01;
        public const uint SNAKE_PREFIX = 0x00;
        public const int CELL_MAX_SIZE_BYTES = 126;
        
        public static ulong GenerateQueryId(int timeout, int? randomId = null) {
            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var random = randomId ?? new Random().Next(0, (int)Math.Pow(2, 30));

            return (ulong)((now + timeout) << 32) | (uint)random;
        }

        public static Cell CreateOffChainUriCell(string uri)
        {
            return new CellBuilder()
                .StoreUInt(OFFCHAIN_CONTENT_PREFIX, 8)
                .StoreBytes(Encoding.UTF8.GetBytes(uri))
                .Build();
        }
        
        private static List<byte[]> BufferToChunks(byte[] buff, int chunkSize)
        {
            List<byte[]> chunks = new List<byte[]>();
            int currentIndex = 0;

            while (buff.Length - currentIndex > 0)
            {
                int currentChunkSize = Math.Min(chunkSize, buff.Length - currentIndex);
                byte[] chunk = new byte[currentChunkSize];
                Array.Copy(buff, currentIndex, chunk, 0, currentChunkSize);
                chunks.Add(chunk);
                currentIndex += currentChunkSize;
            }

            return chunks;
        }

        private static Cell MakeSnakeCell(byte[] data)
        {
            var chunks = BufferToChunks(data, CELL_MAX_SIZE_BYTES);
            var currentCell = new CellBuilder();

            for (int i = chunks.Count - 1; i >= 0; i--)
            {
                if (i == 0)
                {
                    currentCell.StoreInt(SNAKE_PREFIX, 8);
                }

                currentCell.StoreBytes(chunks[i]);

                if (i > 0)
                {
                    Cell finishedCell = currentCell.Build();
                    currentCell = new CellBuilder();
                    currentCell.StoreRef(finishedCell);
                }
            }

            return new CellBuilder().StoreRef(currentCell.Build()).Build();
        }
        
        public static Cell CreateOnChainUriCell(JettonOnChainContent contentStorage)
        {
            var hmOptions = new HashmapOptions<byte[], Cell>()
            {
                KeySize = 256,
                Serializers = new HashmapSerializers<byte[], Cell>
                {
                    Key = k => new BitsBuilder(256).StoreBytes(k).Build(),
                    Value = v => v
                },
                Deserializers = null
            };
            
            var hm = new HashmapE<byte[], Cell>(hmOptions);

            if (contentStorage.Name == null || contentStorage.Symbol == null || contentStorage.Description == null)
                throw new Exception("Jetton must contain required fields - name, symbol and description.");
            
            hm.Set(Utils.HexToBytes("82a3537ff0dbce7eec35d69edc3a189ee6f17d82f353a553f9aa96cb0be3ce89"),
                MakeSnakeCell(Encoding.UTF8.GetBytes(contentStorage.Name!)));
            hm.Set(Utils.HexToBytes("c9046f7a37ad0ea7cee73355984fa5428982f8b37c8f7bcec91f7ac71a7cd104"),
                MakeSnakeCell(Encoding.UTF8.GetBytes(contentStorage.Description!)));
            hm.Set(Utils.HexToBytes("b76a7ca153c24671658335bbd08946350ffc621fa1c516e7123095d4ffd5c581"),
                MakeSnakeCell(Encoding.UTF8.GetBytes(contentStorage.Symbol!)));
            
            if(contentStorage.Decimals != null)
                hm.Set(Utils.HexToBytes("ee80fd2f1e03480e2282363596ee752d7bb27f50776b95086a0279189675923e"),
                    MakeSnakeCell(Encoding.UTF8.GetBytes(contentStorage.Decimals.ToString())));
            if(contentStorage.Image != null)
                hm.Set(Utils.HexToBytes("6105d6cc76af400325e94d588ce511be5bfdbb73b437dc51eca43917d7a43e3d"),
                    MakeSnakeCell(Encoding.UTF8.GetBytes(contentStorage.Image)));
            if(contentStorage.Uri != null)
                hm.Set(Utils.HexToBytes("70e5d7b6a29b392f85076fe15ca2f2053c56c2338728c4e33c9e8ddb1ee827cc"),
                    MakeSnakeCell(Encoding.UTF8.GetBytes(contentStorage.Uri)));
            if(contentStorage.ImageData != null)
                hm.Set(Utils.HexToBytes("d9a88ccec79eef59c84b671136a20ece4cd00caaad5bc47e2c208829154ee9e4"),
                    MakeSnakeCell(Encoding.UTF8.GetBytes(contentStorage.ImageData)));
            if(contentStorage.RenderType != null)
                hm.Set(Utils.HexToBytes("d33ae06043036d0d1c3be27201ac15ee4c73da8cdb7c8f3462ce308026095ac0"),
                    MakeSnakeCell(Encoding.UTF8.GetBytes(contentStorage.RenderType)));
            if(contentStorage.AmountStyle != null)
                hm.Set(Utils.HexToBytes("8b10e058ce46c44bc1ba139bc9761721e49170e2c0a176129250a70af053b700"),
                    MakeSnakeCell(Encoding.UTF8.GetBytes(contentStorage.AmountStyle)));
            
            return new CellBuilder()
                .StoreUInt(ONCHAIN_CONTENT_PREFIX, 8)
                .StoreDict(hm)
                .Build();
        }
    }
}
