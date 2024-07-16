using System;
using System.Collections;
using System.Numerics;
using System.Text;
using TonSdk.Core.Crypto;

namespace TonSdk.Core.Boc {
    public abstract class BitsSliceImpl<T, U> where T : BitsSliceImpl<T, U> {
        protected void CheckBitsUnderflow(int bitEnd) {
            if (bitEnd > _bits_en) {
                throw new ArgumentException("Bits underflow");
            }
        }

        protected bool CheckBitsUnderflowQ(int bitEnd) {
            return bitEnd > _bits_en;
        }

        protected void CheckSize(int size) {
            if (size < 0) {
                throw new ArgumentException("Invalid size. Must be >= 0", nameof(size));
            }
        }

        protected Bits _bits;
        protected int _bits_st = 0;
        protected int _bits_en;

        public int RemainderBits {
            get { return _bits_en - _bits_st; }
        }

        public Bits Bits {
            get { return new Bits(_bits.Data.slice(_bits_st, _bits_en)); }
        }

        public BitsSliceImpl(BitArray bits) {
            _bits = new Bits(bits);
            _bits_en = _bits.Length;
        }

        public BitsSliceImpl(Bits bits) {
            _bits = bits;
            _bits_en = _bits.Length;
        }

        protected BitsSliceImpl(Bits bits, int bits_st, int bits_en) {
            _bits = bits;
            _bits_st = bits_st;
            _bits_en = bits_en;
        }

        public T SkipBits(int size) {
            CheckSize(size);
            var bitEnd = _bits_st + size;
            CheckBitsUnderflow(bitEnd);
            _bits_st = bitEnd;
            return (T)this;
        }

        public Bits ReadBits(int size) {
            CheckSize(size);
            var bitEnd = _bits_st + size;
            CheckBitsUnderflow(bitEnd);
            return new Bits(_bits.Data.slice(_bits_st, bitEnd));
        }

        public Bits LoadBits(int size) {
            CheckSize(size);
            var bitEnd = _bits_st + size;
            CheckBitsUnderflow(bitEnd);
            var bits = _bits.Data.slice(_bits_st, bitEnd);
            _bits_st = bitEnd;
            return new Bits(bits);
        }

        public T SkipBit() {
            return SkipBits(1);
        }

        public bool ReadBit() {
            var bitEnd = _bits_st + 1;
            CheckBitsUnderflow(bitEnd);
            return _bits.Data[_bits_st];
        }

        public bool ReadBit(int idx) {
            var bitEnd = _bits_st + idx + 1;
            CheckBitsUnderflow(bitEnd);
            return _bits.Data[_bits_st + idx];
        }

        public bool LoadBit() {
            var bitEnd = _bits_st + 1;
            CheckBitsUnderflow(bitEnd);
            var bit = _bits.Data[_bits_st];
            _bits_st = bitEnd;
            return bit;
        }

        public BigInteger ReadUInt(int size) {
            CheckSize(size);
            var bitEnd = _bits_st + size;
            CheckBitsUnderflow(bitEnd);
            return _unsafeReadBigInteger(size);
        }

        public BigInteger LoadUInt(int size) {
            CheckSize(size);
            var bitEnd = _bits_st + size;
            CheckBitsUnderflow(bitEnd);
            var result = _unsafeReadBigInteger(size);
            _bits_st = bitEnd;
            return result;
        }

        public BigInteger ReadInt(int size) {
            CheckSize(size);
            var bitEnd = _bits_st + size;
            CheckBitsUnderflow(bitEnd);
            return _unsafeReadBigInteger(size, true);
        }

        public BigInteger LoadInt(int size) {
            CheckSize(size);
            var bitEnd = _bits_st + size;
            CheckBitsUnderflow(bitEnd);
            var result = _unsafeReadBigInteger(size, true);
            _bits_st = bitEnd;
            return result;
        }

        public BigInteger ReadUInt32LE() {
            var bitEnd = _bits_st + 32;
            CheckBitsUnderflow(bitEnd);
            return _unsafeReadBigInteger(32, false, true);
        }

        public BigInteger LoadUInt32LE() {
            var bitEnd = _bits_st + 32;
            CheckBitsUnderflow(bitEnd);
            var result = _unsafeReadBigInteger(32, false, true);
            _bits_st = bitEnd;
            return result;
        }

        public BigInteger ReadUInt64LE() {
            var bitEnd = _bits_st + 64;
            CheckBitsUnderflow(bitEnd);
            return _unsafeReadBigInteger(64, false, true);
        }

        public BigInteger LoadUInt64LE() {
            var bitEnd = _bits_st + 64;
            CheckBitsUnderflow(bitEnd);
            var result = _unsafeReadBigInteger(64, false, true);
            _bits_st = bitEnd;
            return result;
        }

        public Coins ReadCoins(int decimals = 9) {
            return new Coins((decimal)ReadVarUInt(16), new CoinsOptions(true, decimals));
        }

        public Coins LoadCoins(int decimals = 9) {
            return new Coins((decimal)LoadVarUInt(16), new CoinsOptions(true, decimals));
        }

        public BigInteger ReadVarUInt(int length) {
            return LoadVarInt(length, false, false);
        }

        public BigInteger LoadVarUInt(int length) {
            return LoadVarInt(length, false, true);
        }

        public BigInteger ReadVarInt(int length) {
            return LoadVarInt(length, true, false);
        }

        public BigInteger LoadVarInt(int length) {
            return LoadVarInt(length, true, true);
        }

        protected BigInteger LoadVarInt(int length, bool sgn, bool inplace) {
            int size = (int)Math.Ceiling(Math.Log(length, 2));
            int sizeBytes = (int)ReadUInt(size);
            int sizeBits = sizeBytes * 8;

            CheckBitsUnderflow(_bits_st + size + sizeBits);

            if (inplace) {
                SkipBits(size);
                return sizeBits == 0
                    ? BigInteger.Zero
                    : sgn
                        ? LoadInt(sizeBits)
                        : LoadUInt(sizeBits);
            }
            else {
                var varIntSlice = ReadBits(size + sizeBits).Parse();
                varIntSlice.SkipBits(size);
                return sizeBits == 0
                    ? BigInteger.Zero
                    : sgn
                        ? varIntSlice.LoadInt(sizeBits)
                        : varIntSlice.LoadUInt(sizeBits);
            }
        }

        public Address? ReadAddress() {
            return LoadAddress(false);
        }

        public Address? LoadAddress() {
            return LoadAddress(true);
        }

        protected Address? LoadAddress(bool inplace) {
            byte prefix = (byte)ReadUInt(2);
            switch (prefix) {
                case 0b10: // addr_std
                    var prefixAndAnycast = (byte)ReadUInt(3);
                    if (prefixAndAnycast == 0b101) {
                        throw new AddressTypeNotSupportedError("Anycast addresses are not supported");
                    }

                    CheckBitsUnderflow(_bits_st + 267);
                    if (inplace) {
                        SkipBits(3);
                        return new Address((int)LoadInt(8), LoadBytes(32));
                    }
                    else {
                        var addrSlice = ReadBits(267).Parse();
                        addrSlice.SkipBits(3);
                        return new Address((int)addrSlice.LoadInt(8), addrSlice.LoadBytes(32));
                    }
                case 0b01: // addr_extern
                {
                    int len = (int)LoadInt(9);
                    var extAddr = LoadBits(len);
                    var address = new ExternalAddress(len, extAddr);
                    return null;
                }
                case 0b11: // addr_var
                    throw new AddressTypeNotSupportedError("Var addresses are not supported");
                default: // addr_none
                    if (inplace) SkipBits(2);
                    return null;
            }
        }

        public byte[] ReadBytes(int size) {
            return ReadBits(size * 8).ToBytes();
        }

        public byte[] LoadBytes(int size) {
            return LoadBits(size * 8).ToBytes();
        }

        public string ReadString() {
            return ReadString(RemainderBits / 8);
        }

        public string ReadString(int size) {
            return Encoding.UTF8.GetString(ReadBytes(size));
        }

        public string LoadString() {
            return LoadString(RemainderBits / 8);
        }

        public string LoadString(int size) {
            return Encoding.UTF8.GetString(LoadBytes(size));
        }

        public abstract U Restore();

        private BigInteger _unsafeReadBigInteger(int size, bool sgn = false, bool le = false) {
            BigInteger result = 0;

            if (le) {
                var bits = new BitArray(LoadBits(size).ToBytes());
                for (int i = 0; i < size; i++) {
                    if (bits[i]) {
                        result |= BigInteger.One << i;
                    }
                }
            }
            else {
                for (int i = 0; i < size; i++) {
                    if (_bits.Data[_bits_st + i]) {
                        result |= BigInteger.One << (size - 1 - i);
                    }
                }
            }

            // Check if the most significant bit is set (which means the number is negative)
            if (sgn & (result & (BigInteger.One << (size - 1))) != 0) {
                // If the number is negative, apply two's complement
                result -= BigInteger.One << size;
            }

            return result;
        }
    }


    public class BitsSlice : BitsSliceImpl<BitsSlice, Bits> {
        public BitsSlice(BitArray bits) : base(bits) { }

        public BitsSlice(Bits bits) : base(bits) { }

        public Bits RestoreRemainder() {
            return Bits;
        }

        public override Bits Restore() {
            return _bits;
        }
    }
}

public class AddressTypeNotSupportedError : Exception
{
    public AddressTypeNotSupportedError() { }

    public AddressTypeNotSupportedError(string message)
        : base(message) { }
}
