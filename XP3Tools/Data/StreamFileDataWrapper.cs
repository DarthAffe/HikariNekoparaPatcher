using System;
using System.IO;
using System.Text;
using Ionic.Zlib;
using XP3Tools.Extensions;

namespace XP3Tools.Data
{
    public class StreamFileDataWrapper : IFileDataWrapper
    {
        #region Properties & Fields

        private Stream _stream;

        public byte[] Bytes => _stream.ToArray();
        public int Location => (int)_stream.Position;
        public int Size => (int)(_stream?.Length ?? -1);
        public bool IsCompressed { get; private set; } = false;

        #endregion

        #region Constructors

        private StreamFileDataWrapper(Stream stream)
        {
            this._stream = stream;

            _stream.Seek(0, SeekOrigin.Begin);
        }

        #endregion

        #region Methods

        public void GoTo(int location)
        {
            _stream.Seek(location, SeekOrigin.Begin);
        }

        public byte GetByte()
        {
            return GetBytes(1)[0];
        }

        public byte GetByte(int index)
        {
            return GetBytes(index, 1)[0];
        }

        public byte[] GetBytes(int count)
        {
            byte[] data = new byte[count];
            _stream.Read(data, 0, count);
            return data;
        }

        public byte[] GetBytes(int index, int count)
        {
            byte[] data = new byte[count];

            long position = _stream.Position;
            _stream.Seek(index, SeekOrigin.Begin);
            _stream.Read(data, 0, count);
            _stream.Seek(position, SeekOrigin.Begin);

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
            byte[] data = GetBytes(chars.Length);
            for (int i = 0; i < chars.Length; i++)
                chars[i] = (char)data[i];

            return new string(chars);
        }

        public string GetUnicodeString(int length)
        {
            byte[] data = GetBytes(length * 2);
            return Encoding.Unicode.GetString(data);
        }

        public IFileDataWrapper GetData(int offset, int size)
        {
            byte[] data = GetBytes(offset, size);
            return MemoryFileDataWrapper.FromArray(data);
        }

        public IFileDataWrapper GetCompressedData(int offset, int size, int uncompressedSize)
        {
            byte[] data = GetBytes(offset, size);
            return MemoryFileDataWrapper.FromArray(ZlibStream.UncompressBuffer(data));
        }

        public void PutByte(int index, byte b)
        {
            long position = _stream.Position;
            _stream.Seek(index, SeekOrigin.Begin);
            _stream.WriteByte(b);
            _stream.Seek(position, SeekOrigin.Begin);
        }

        public void XOR(int offset, int size, byte xorByte)
        {
            byte[] data = GetBytes(offset, size);

            long position = _stream.Position;
            _stream.Seek(offset, SeekOrigin.Begin);
            for (int i = 0; i < size; i++)
                _stream.WriteByte((byte)(data[i] ^ xorByte));
            _stream.Seek(position, SeekOrigin.Begin);
        }

        public void Compress()
        {
            _stream = new ZlibStream(_stream, CompressionMode.Compress);
            IsCompressed = true;
        }

        public void Dispose()
        {
            _stream?.Dispose();
            _stream = null;
        }

        #endregion

        #region Factory

        public static StreamFileDataWrapper FromFile(string file)
        {
            return new StreamFileDataWrapper(File.Open(file, FileMode.Open));
        }

        public static StreamFileDataWrapper FromArray(byte[] data, bool isCompressed = false)
        {
            return new StreamFileDataWrapper(new MemoryStream(data)) { IsCompressed = isCompressed };
        }

        public static StreamFileDataWrapper FromStream(Stream stream)
        {
            return new StreamFileDataWrapper(stream);
        }

        #endregion
    }
}
