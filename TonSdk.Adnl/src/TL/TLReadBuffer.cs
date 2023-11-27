using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;
using TonSdk.Core.Boc;
using TonSdk.Core.Crypto;

namespace TonSdk.Adnl.TL
{
    public class TLReadBuffer
    {
        private readonly BinaryReader _reader;

    public TLReadBuffer(byte[] buffer)
    {
        _reader = new BinaryReader(new MemoryStream(buffer));
    }

    private void EnsureSize(int needBytes)
    {
        if (_reader.BaseStream.Position + needBytes > _reader.BaseStream.Length)
        {
            throw new Exception("Not enough bytes");
        }
    }

    public int ReadInt32()
    {
        EnsureSize(4);
        return _reader.ReadInt32();
    }

    public uint ReadUInt32()
    {
        EnsureSize(4);
        return _reader.ReadUInt32();
    }

    public long ReadInt64()
    {
        EnsureSize(8);
        return _reader.ReadInt64();
    }

    public byte ReadUInt8()
    {
        EnsureSize(1);
        return _reader.ReadByte();
    }

    public byte[] ReadInt256()
    {
        EnsureSize(32);
        return _reader.ReadBytes(32);
    }

    public byte[] ReadBuffer()
    {
        int len = ReadUInt8();
        if (len == 254)
        {
            len = _reader.ReadBytes(3)[0] | _reader.ReadBytes(3)[1] << 8 | _reader.ReadBytes(3)[2] << 16;
        }

        byte[] buffer = _reader.ReadBytes(len);

        while ((_reader.BaseStream.Position % 4) != 0)
        {
            _reader.ReadByte();
        }

        return buffer;
    }

    public string ReadString()
    {
        byte[] buffer = ReadBuffer();
        return Encoding.UTF8.GetString(buffer);
    }

    public bool ReadBool()
    {
        uint value = ReadUInt32();
        return value switch
        {
            0xbc799737 => false,
            0x997275b5 => true,
            _ => throw new Exception("Unknown boolean value"),
        };
    }

    public T[] ReadVector<T>(Func<TLReadBuffer, T> codec)
    {
        int count = (int)ReadUInt32();
        T[] result = new T[count];
        for (int i = 0; i < count; i++)
        {
            result[i] = codec(this);
        }
        return result;
    }

        public byte[] ReadObject()
        {
            int remainingBytes = (int)(_reader.BaseStream.Length - _reader.BaseStream.Position);
            return _reader.ReadBytes(remainingBytes);
        }

        public int Remaining => (int)(_reader.BaseStream.Length - _reader.BaseStream.Position);
    }
}