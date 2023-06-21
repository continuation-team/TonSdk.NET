public static class Utils
{
    public static ushort Crc16(byte[] data)
    {
        const ushort POLY = 0x1021;
        ushort crc = 0;

        for (int i = 0; i < data.Length; i++)
        {
            crc ^= (ushort)(data[i] << 8);

            for (int j = 0; j < 8; j++)
            {
                if ((crc & 0x8000) == 0x8000)
                {
                    crc = (ushort)((crc << 1) ^ POLY);
                }
                else
                {
                    crc <<= 1;
                }
            }
        }

        return (ushort)(crc & 0xffff);
    }

    public static byte[] Crc16BytesBigEndian(byte[] data)
    {
        ushort crc = Crc16(data);
        byte[] bytes = new byte[2];

        bytes[0] = (byte)(crc >> 8);
        bytes[1] = (byte)crc;

        return bytes;
    }

    public static uint Crc32c(byte[] data)
    {
        const uint POLY = 0x82f63b78;
        uint crc = 0xffffffff;

        for (int i = 0; i < data.Length; i++)
        {
            crc ^= data[i];

            for (int j = 0; j < 8; j++)
            {
                if ((crc & 1) == 1)
                {
                    crc = (crc >> 1) ^ POLY;
                }
                else
                {
                    crc >>= 1;
                }
            }
        }

        crc ^= 0xffffffff;

        return crc;
    }

    public static byte[] Crc32cBytesLittleEndian(byte[] data)
    {
        uint crc = Crc32c(data);
        byte[] bytes = new byte[4];

        bytes[0] = (byte)crc;
        bytes[1] = (byte)(crc >> 8);
        bytes[2] = (byte)(crc >> 16);
        bytes[3] = (byte)(crc >> 24);

        return bytes;
    }

    public static short ToLittleEndianInt16(this byte[] bytes, int startIndex = 0)
    {
        if (bytes.Length - startIndex < 2)
        {
            throw new ArgumentException("Byte array too small to convert to Int16");
        }

        return (short)(bytes[startIndex] | (bytes[startIndex + 1] << 8));
    }

}