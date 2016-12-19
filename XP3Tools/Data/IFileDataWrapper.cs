using System;

namespace XP3Tools.Data
{
    public interface IFileDataWrapper : IDisposable
    {
        byte[] Bytes { get; }
        int Location { get; }
        int Size { get; }
        bool IsCompressed { get; }

        void GoTo(int location);

        byte GetByte();
        byte GetByte(int index);
        byte[] GetBytes(int count);
        short GetShort();
        int GetInt();
        int GetLong();
        string GetString(int length);
        string GetUnicodeString(int length);
        IFileDataWrapper GetData(int offset, int size);
        IFileDataWrapper GetCompressedData(int offset, int size, int uncompressedSize);

        void PutByte(int index, byte b);
        void XOR(int offset, int size, byte xorByte);
        void Compress();
    }
}