using System;

namespace TonSdk.Adnl.LiteClient
{
    public class Crc32
    {
        private static readonly uint[] Table;

        static Crc32()
        {
            uint polynomial = 0xedb88320;
            Table = new uint[256];

            for (uint i = 0; i < Table.Length; ++i)
            {
                var temp = i;
                for (var j = 8; j > 0; --j)
                {
                    if ((temp & 1) == 1)
                    {
                        temp = (temp >> 1) ^ polynomial;
                    }
                    else
                    {
                        temp >>= 1;
                    }
                }
                Table[i] = temp;
            }
        }

        public static uint ComputeChecksumUint(byte[] bytes)
        {
            uint crc = 0xffffffff;
            foreach (byte t in bytes)
            {
                byte index = (byte)((crc & 0xff) ^ t);
                crc = (crc >> 8) ^ Table[index];
            }
            return ~crc;
        }

        public static byte[] ComputeChecksum(byte[] bytes)
        {
            byte[] result = BitConverter.GetBytes(ComputeChecksumUint(bytes));
            if(!BitConverter.IsLittleEndian) Array.Reverse(result);
            return result;
        }
        
        internal static ushort CalculateCrc16Xmodem(byte[] data)
        {
            const ushort polynom = 0x1021;
            ushort crc = 0x0000;

            foreach (byte b in data)
            {
                crc ^= (ushort)(b << 8);
                for (int i = 0; i < 8; i++)
                {
                    if ((crc & 0x8000) != 0)
                    {
                        crc = (ushort)((crc << 1) ^ polynom);
                    }
                    else
                    {
                        crc <<= 1;
                    }
                }
            }

            return crc;
        }
    }
}