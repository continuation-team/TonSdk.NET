using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;

namespace TonSdk.Adnl.TL
{
    public class TLWriteBuffer
    {
        private int _used = 0;
        private byte[] _buf = new byte[128];

        private void EnsureSize(int needBytes)
        {
            if ((_buf.Length - _used) <= needBytes)
            {
                var newBuf = new byte[_buf.Length * 2];
                Array.Copy(_buf, newBuf, _buf.Length);
                _buf = newBuf;
            }
        }

        public void WriteInt32(int val)
        {
            EnsureSize(4);
            BitConverter.GetBytes(val).CopyTo(_buf, _used);
            _used += 4;
        }

        public void WriteUInt32(uint val)
        {
            EnsureSize(4);
            BitConverter.GetBytes(val).CopyTo(_buf, _used);
            _used += 4;
        }

        public void WriteInt64(long val)
        {
            EnsureSize(8);
            BitConverter.GetBytes(val).CopyTo(_buf, _used);
            _used += 8;
        }

        public void WriteUInt8(byte val)
        {
            EnsureSize(1);
            _buf[_used] = val;
            _used++;
        }

        public void WriteBytes(byte[] val)
        {
            EnsureSize(val.Length);
            foreach (var byteVal in val)
            {
                WriteUInt8(byteVal);
            }
        }
        
        public void WriteInt256(BigInteger val)
        {
            EnsureSize(32);
            byte[] array = val.ToByteArray();
            if (!BitConverter.IsLittleEndian) Array.Reverse(array);
            foreach (var byteVal in array)
            {
                WriteUInt8(byteVal);
            }
        }

        public void WriteBuffer(byte[] buf)
        {
            EnsureSize(buf.Length + 4);
            int len = 0;
            if (buf.Length <= 253)
            {
                WriteUInt8((byte)buf.Length);
                len++;
            }
            else
            {
                WriteUInt8(254);
                EnsureSize(3);
                BitConverter.GetBytes(buf.Length).CopyTo(_buf, _used);
                _used += 3;
                len += 4;
            }

            foreach (var byteVal in buf)
            {
                WriteUInt8(byteVal);
                len++;
            }

            while (len % 4 != 0)
            {
                WriteUInt8(0);
                len++;
            }
        }

        public void WriteString(string src)
        {
            WriteBuffer(Encoding.UTF8.GetBytes(src));
        }

        public void WriteBool(bool src)
        {
            WriteUInt32(src ? 0x997275b5 : 0xbc799737);
        }

        public void WriteVector<T>(Action<T, TLWriteBuffer> codec, IEnumerable<T> data)
        {
            WriteUInt32((uint)data.Count());
            foreach (var d in data)
            {
                codec(d, this);
            }
        }

        public byte[] Build()
        {
            return _buf.Take(_used).ToArray();
        }

        public byte[] GetBytes() => _buf;
    }
}