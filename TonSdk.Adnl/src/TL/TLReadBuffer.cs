using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using TonSdk.Core.Boc;
using TonSdk.Core.Crypto;

namespace TonSdk.Adnl.TL
{
    public class TLReadBuffer
    {
        private readonly BitsSlice _buf;

        public TLReadBuffer(byte[] buf)
        {
            _buf = new Bits(buf).Parse();
        }

        public int ReadInt32Le()
        {
            byte[] array = BitConverter.GetBytes((int)_buf.LoadInt(32));
            Array.Reverse(array);
            return BitConverter.ToInt32(array);
        }

        public uint ReadUInt32Le() => (uint)_buf.LoadUInt32LE();

        public long ReadInt64Le()
        {
            byte[] array = BitConverter.GetBytes((long)_buf.LoadInt(64));
            Array.Reverse(array);
            return BitConverter.ToInt64(array);
        }

        public byte ReadUInt8() => (byte)_buf.LoadUInt(8);

        public BigInteger ReadInt256Le()
        {
            byte[] array = _buf.LoadInt(256).ToByteArray();
            Array.Reverse(array);
            return new BigInteger(array);
        }

        public string ReadString() => _buf.LoadString();

        public bool ReadBool()
        {
            uint val = ReadUInt32Le();
            if (val == 0xbc799737) return false;
            if (val == 0x997275b5) return true;
            throw new Exception("Unknown boolean value");
        }

        public List<T> ReadVector<T>(Func<TLReadBuffer, T> codec)
        {
            int count = (int)ReadUInt32Le();
            List<T> res = new List<T>();
            for (int i = 0; i < count; i++)
            {
                res.Add(codec(this));
            }
            return res;
        }

        public byte[] Remainder => _buf.RestoreRemainder().ToBytes();

        // public byte[] ReadObject()
        // {
        //     int len = _buf.RemainderBits - _offset;
        //     byte[] buff = new byte[len];
        //     Array.Copy(_buf, _offset, buff, 0, len);
        //     _offset += len;
        //     return buff;
        // }
    }
}