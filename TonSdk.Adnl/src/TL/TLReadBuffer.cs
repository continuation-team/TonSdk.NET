using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace TonSdk.Adnl.TL
{
    public class TLReadBuffer
    {
        private int _offset = 0;
        private readonly byte[] _buf;

        public TLReadBuffer(byte[] buf)
        {
            _buf = buf;
        }

        private void EnsureSize(int needBytes)
        {
            if (_offset + needBytes > _buf.Length) throw new Exception("Not enough bytes");
        }

        public int ReadInt32()
        {
            EnsureSize(4);
            int val = BitConverter.ToInt32(_buf, _offset);
            _offset += 4;
            return val;
        }

        public uint ReadUInt32()
        {
            EnsureSize(4);
            uint val = BitConverter.ToUInt32(_buf, _offset);
            _offset += 4;
            return val;
        }

        public string ReadInt64()
        {
            EnsureSize(8);
            byte[] buff = new byte[8];
            Array.Copy(_buf, _offset, buff, 0, 8);
            _offset += 8;
            bool inv = (buff[7] & 128) != 0;
            if (inv)
            {
                buff[7] &= 127;
            }

            BigInteger baseNum = new BigInteger(buff);
            if (inv)
            {
                baseNum -= BigInteger.Parse("8000000000000000", System.Globalization.NumberStyles.HexNumber);
            }
            return baseNum.ToString();
        }

        public byte ReadUInt8()
        {
            EnsureSize(1);
            byte val = _buf[_offset];
            _offset++;
            return val;
        }

        public byte[] ReadInt256()
        {
            EnsureSize(32);
            byte[] buff = new byte[32];
            Array.Copy(_buf, _offset, buff, 0, 32);
            _offset += 32;
            return buff;
        }

        public byte[] ReadBuffer()
        {
            int size = 1;
            int len = ReadUInt8();

            if (len == 254)
            {
                len = BitConverter.ToInt32(new byte[] { _buf[_offset], _buf[_offset + 1], _buf[_offset + 2], 0 }, 0);
                _offset += 3;
                size += 3;
            }

            size += len;

            byte[] buff = new byte[len];
            Array.Copy(_buf, _offset, buff, 0, len);
            _offset += len;

            while (size % 4 != 0)
            {
                ReadUInt8();
                size++;
            }

            return buff;
        }

        public string ReadString()
        {
            return Encoding.UTF8.GetString(ReadBuffer());
        }

        public bool ReadBool()
        {
            uint val = ReadUInt32();
            if (val == 0xbc799737) return false;
            if (val == 0x997275b5) return true;
            throw new Exception("Unknown boolean value");
        }

        public List<T> ReadVector<T>(Func<TLReadBuffer, T> codec)
        {
            int count = (int)ReadUInt32();
            List<T> res = new List<T>();
            for (int i = 0; i < count; i++)
            {
                res.Add(codec(this));
            }
            return res;
        }

        public byte[] ReadObject()
        {
            int len = _buf.Length - _offset;
            byte[] buff = new byte[len];
            Array.Copy(_buf, _offset, buff, 0, len);
            _offset += len;
            return buff;
        }
    }
}