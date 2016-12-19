using System;

namespace XP3Tools.Data
{
    public interface IChunkDataWrapper : IDisposable
    {
        byte[] Bytes { get; }
        int Size { get; }

        void WriteBytes(params byte[] data);
        void WriteShort(short data);
        void WriteInt(int data);
        void ReplaceInt(int position, int data);
        void WriteLong(int data);
        void WriteString(string data);
        void WriteUnicodeString(string data);
    }
}
