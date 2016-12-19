using System;
using System.IO;
using System.Text;
using Ionic.Zlib;

namespace XP3Tools.Data
{
    public class MemoryFileDataWrapper : IFileDataWrapper
    {
        #region Properties & Fields

        private byte[] _bytes;

        public byte[] Bytes => _bytes;
        public int Location { get; private set; }
        public int Size => _bytes?.Length ?? -1;
        public bool IsCompressed { get; private set; } = false;

        #endregion

        #region Constructors

        private MemoryFileDataWrapper()
        { }

        #endregion

        #region Methods

        public void GoTo(int location)
        {
            Location = location;
        }

        public byte GetByte()
        {
            return _bytes[Location++];
        }

        public byte GetByte(int index)
        {
            return _bytes[index];
        }

        public byte[] GetBytes(int count)
        {
            byte[] data = new byte[count];
            for (int i = 0; i < data.Length; i++)
                data[i] = _bytes[Location++];

            return data;
        }

        public short GetShort()
        {
            byte[] data = GetBytes(sizeof(short));
            return BitConverter.ToInt16(data, 0);
        }

        public int GetInt()
        {
            byte[] data = GetBytes(sizeof(int));
            return BitConverter.ToInt32(data, 0);
        }

        public int GetLong()
        {
            byte[] data = GetBytes(sizeof(long));
            long l = BitConverter.ToInt64(data, 0);
            return l > int.MaxValue ? int.MaxValue : (int)l;
        }

        public string GetString(int length)
        {
            char[] chars = new char[length];
            for (int i = 0; i < chars.Length; i++)
                chars[i] = (char)_bytes[Location++];

            return new string(chars);
        }

        public string GetUnicodeString(int length)
        {
            byte[] chars = new byte[length * 2];
            for (int i = 0; i < chars.Length; i++)
                chars[i] = _bytes[Location++];

            return Encoding.Unicode.GetString(chars);
        }

        public IFileDataWrapper GetData(int offset, int size)
        {
            MemoryFileDataWrapper newData = FromArray(new byte[size]);
            Array.Copy(_bytes, offset, newData._bytes, 0, size);

            return newData;
        }

        public IFileDataWrapper GetCompressedData(int offset, int size, int uncompressedSize)
        {
            MemoryFileDataWrapper newData = FromArray(new byte[uncompressedSize]);
            using (MemoryStream ms = new MemoryStream(_bytes, offset, size))
            using (ZlibStream zlib = new ZlibStream(ms, CompressionMode.Decompress))
                zlib.Read(newData._bytes, 0, uncompressedSize);

            return newData;
        }

        public void PutByte(int index, byte b)
        {
            _bytes[index] = b;
        }

        public void XOR(int offset, int size, byte xorByte)
        {
            for (int i = 0; i < size; i++)
                _bytes[offset + i] = (byte)(_bytes[offset + i] ^ xorByte);
        }

        public void Compress()
        {
            _bytes = ZlibStream.CompressBuffer(_bytes);
            IsCompressed = true;
        }

        public void Dispose()
        {
            _bytes = null;
        }

        #endregion

        #region Factory

        public static MemoryFileDataWrapper FromFile(string file)
        {
            return new MemoryFileDataWrapper { _bytes = File.ReadAllBytes(file) };
        }

        public static MemoryFileDataWrapper FromArray(byte[] data, bool isCompressed = false)
        {
            return new MemoryFileDataWrapper { _bytes = data, IsCompressed = isCompressed };
        }

        #endregion
    }
}
