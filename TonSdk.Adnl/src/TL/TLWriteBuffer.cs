using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using TonSdk.Core.Crypto;

namespace TonSdk.Adnl.TL
{
    public class TLWriteBuffer
    {
        private MemoryStream _stream;
        private BinaryWriter _writer;

        public TLWriteBuffer()
        {
            _stream = new MemoryStream(128);
            _writer = new BinaryWriter(_stream);
        }

        private void EnsureSize(int needBytes)
        {
            if (_stream.Length - _stream.Position < needBytes)
            {
                int newLength = (int)_stream.Length * 2;
                var newStream = new MemoryStream(newLength);
                _stream.Position = 0;
                _stream.CopyTo(newStream);
                _stream = newStream;
                _writer = new BinaryWriter(_stream);
            }
        }

        public void WriteInt32(int val)
        {
            EnsureSize(4);
            _writer.Write(val);
        }

        public void WriteUInt32(uint val)
        {
            EnsureSize(4);
            _writer.Write(val);
        }

        public void WriteInt64(long val)
        {
            EnsureSize(8);
            _writer.Write(val);
        }

        public void WriteUInt8(byte val)
        {
            EnsureSize(1);
            _writer.Write(val);
        }

        public void WriteInt256(BigInteger val)
        {
            EnsureSize(32);
            byte[] bytes = val.ToByteArray();
            if (bytes.Length != 32)
            {
                throw new Exception("Invalid int256 length");
            }
            _writer.Write(bytes);
        }

        public void WriteBytes(byte[] data, int size)
        {
            if (data.Length != size) throw new Exception($"Input array size not equals to {size}.");
            EnsureSize(size);
            _writer.Write(data);
        }
        
        public void WriteBuffer(byte[] buf)
        {
            EnsureSize(buf.Length + 4);
            int len = 0;
            if (buf.Length <= 253)
            {
                WriteUInt8((byte)buf.Length);
                len += 1;
            }
            else
            {
                WriteUInt8(254);
                EnsureSize(3 + buf.Length);
                byte[] lengthBytes = BitConverter.GetBytes(buf.Length);
                if (!BitConverter.IsLittleEndian)
                {
                    Array.Reverse(lengthBytes);
                }
                _writer.Write(lengthBytes, 0, 3);
                len += 4;
            }

            foreach (var byteVal in buf)
            {
                WriteUInt8(byteVal);
                len += 1;
            }

            while (len % 4 != 0)
            {
                WriteUInt8(0);
                len += 1;
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

        public void WriteVector<T>(Action<T, TLWriteBuffer> codec, T[] data)
        {
            WriteUInt32((uint)data.Length);
            foreach (T d in data)
            {
                codec(d, this);
            }
        }

        public byte[] Build()
        {
            return _stream.ToArray();
        }
    }
}