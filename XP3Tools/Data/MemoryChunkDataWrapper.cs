using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XP3Tools.Data
{
    public class MemoryChunkDataWrapper : IChunkDataWrapper
    {
        #region Properties & Fields

        private List<byte> _bytes;

        public byte[] Bytes => _bytes.ToArray();
        public int Size => _bytes.Count;

        #endregion

        #region Constructors

        public MemoryChunkDataWrapper()
            : this(new List<byte>())
        { }

        public MemoryChunkDataWrapper(List<byte> data)
        {
            this._bytes = data;
        }

        #endregion

        #region Methods

        public void WriteBytes(params byte[] data)
        {
            _bytes.AddRange(data);
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
            for (int i = 0; i < bData.Length; i++)
                _bytes[position + i] = bData[i];
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
            _bytes = null;
        }

        #endregion
    }
}
