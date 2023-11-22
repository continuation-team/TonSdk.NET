using System;
using System.Text;
using TonSdk.Adnl.TL;

namespace TonSdk.Adnl.LiteClient
{
    internal class LiteClientMethods
    {
        internal static (byte[], byte[]) EncodeGetMasterchainInfo()
        {
            byte[] queryId = AdnlKeys.GenerateRandomBytes(32);
            TLWriteBuffer buffer = new TLWriteBuffer();
            buffer.WriteUInt32(
                BitConverter.ToUInt32(
                    Crc32.ComputeChecksum(
                        Encoding.UTF8.GetBytes("adnl.message.query query_id:int256 query:bytes = adnl.Message")),0));
            buffer.WriteInt256(queryId);
            buffer.WriteUInt32(12); // 1
            buffer.WriteUInt32(BitConverter.ToUInt32(
                Crc32.ComputeChecksum(
                    Encoding.UTF8.GetBytes("liteServer.query data:bytes = Object")),0));
            buffer.WriteBuffer(Crc32.ComputeChecksum(Encoding.UTF8.GetBytes("liteServer.getMasterchainInfo = liteServer.MasterchainInfo")));
            buffer.WriteUInt8(0);
            buffer.WriteUInt8(0);
            buffer.WriteUInt8(0);
            return (queryId, buffer.Build());
        }

        internal static void DecodeGetMasterchainInfo(byte[] data)
        {
            TLReadBuffer buffer = new TLReadBuffer(data);
            Console.WriteLine(buffer.ReadUInt32());
        }
    }
}