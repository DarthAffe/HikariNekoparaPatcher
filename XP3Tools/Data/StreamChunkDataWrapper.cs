using System;
using System.IO;
using System.Linq;
using System.Text;
using XP3Tools.Extensions;

namespace XP3Tools.Data
{
    public class StreamChunkDataWrapper : IChunkDataWrapper
    {
        #region Properties & Fields

        private Stream _stream;

        public byte[] Bytes => _stream.ToArray();
        public int Size => (int)_stream.Length;

        #endregion

        #region Constructors

        public StreamChunkDataWrapper(Stream stream)
        {
            this._stream = stream;

            _stream.Seek(0, SeekOrigin.Begin);
        }

        #endregion

        #region Methods

        public void WriteBytes(params byte[] data)
        {
            _stream.Write(data, 0, data.Length);
        }

        public void WriteShort(short data)
        {
            WriteBytes(BitConverter.GetBytes(data));
        }

        public void WriteInt(int data)
        {
            WriteBytes(BitConverter.GetBytes(data));
        }

        public void ReplaceInt(int position, int data)
        {
            byte[] bData = BitConverter.GetBytes(data);
            long origPosition = _stream.Position;
            _stream.Seek(position, SeekOrigin.Begin);
            _stream.Write(bData, 0, bData.Length);
            _stream.Seek(origPosition, SeekOrigin.Begin);
        }

        public void WriteLong(int data)
        {
            WriteBytes(BitConverter.GetBytes((long)data));
        }

        public void WriteString(string data)
        {
            WriteBytes(data.Select(x => (byte)x).ToArray());
        }

        public void WriteUnicodeString(string data)
        {
            WriteBytes(Encoding.Unicode.GetBytes(data));
        }

        public void Dispose()
        {
            _stream?.Dispose();
            _stream = null;
        }

        #endregion
    }
}
