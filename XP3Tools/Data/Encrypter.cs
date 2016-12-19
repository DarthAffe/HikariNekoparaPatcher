using System.Collections.Generic;
using System.IO;

namespace XP3Tools.Data
{
    public class Encrypter
    {
        #region Properties & Fields

        private readonly EncrypterData _encrypterData;

        #endregion

        #region Constructors

        private Encrypter(EncrypterData encrypterData)
        {
            this._encrypterData = encrypterData;
        }

        #endregion

        #region Methods

        public byte Decrypt(IFileDataWrapper data)
        {
            int baseByteOffset = _encrypterData.BaseByteOffset < 0 ? data.Size + _encrypterData.BaseByteOffset : _encrypterData.BaseByteOffset;
            byte xorByte = data.GetByte(baseByteOffset);
            if (_encrypterData.BaseByteXOR.HasValue)
                xorByte = (byte)(xorByte ^ _encrypterData.BaseByteXOR.Value);

            if (_encrypterData.FixFirstByte)
            {
                byte firstByte;
                if (_encrypterData.FirstByteXOR.HasValue)
                    firstByte = (byte)(xorByte ^ _encrypterData.FirstByteXOR.Value);
                else
                    firstByte = xorByte;
                data.PutByte(0, firstByte);
            }

            data.XOR(0, data.Size, xorByte);

            return xorByte;
        }

        public void Encrypt(IFileDataWrapper data, byte xorByte = 0x56)
        {
            data.XOR(0, data.Size, xorByte);

            if (_encrypterData.FixFirstByte)
            {
                byte firstByte;
                if (_encrypterData.FirstByteXOR.HasValue)
                    firstByte = (byte)(xorByte ^ _encrypterData.FirstByteXOR.Value);
                else
                    firstByte = xorByte;
                data.PutByte(0, firstByte);
            }

            if (_encrypterData.BaseByteXOR.HasValue)
                xorByte = (byte)(xorByte ^ _encrypterData.BaseByteXOR.Value);

            int baseByteOffset = _encrypterData.BaseByteOffset < 0 ? data.Size + _encrypterData.BaseByteOffset : _encrypterData.BaseByteOffset;
            data.PutByte(baseByteOffset, xorByte);
        }

        #endregion

        #region Factory

        private static readonly Dictionary<string, EncrypterData> EXTENSION_DATA = new Dictionary<string, EncrypterData>
        {
            { ".png", new EncrypterData(1, (byte)'P', 0x89) },
            { ".ogg", new EncrypterData(1, (byte)'g', (byte)'O') },
            { ".bmp", new EncrypterData(1, (byte)'M', (byte)'B') },
            { ".sli", new EncrypterData(-1, (byte)'\n',(byte)'#') },
            { ".scn", new EncrypterData(7, null, null, false) },
            { ".cur", new EncrypterData(1, null, null) },
            { ".otf", new EncrypterData(4, null, (byte)'O') },
            { ".ttf", new EncrypterData(2, null, null) },
            { ".txt", new EncrypterData(-1, null, 0xFF) },
            { ".wmv", new EncrypterData(1, 0x26, 0x30) },
            { "", new EncrypterData(1, 0xFE, 0xFF) },
        };

        public static Encrypter ForFile(string file)
        {
            return ForExtension(Path.GetExtension(file));
        }

        public static Encrypter ForExtension(string extension)
        {
            EncrypterData encrypterData;
            if (!EXTENSION_DATA.TryGetValue(extension, out encrypterData))
                encrypterData = EXTENSION_DATA[""];
            return new Encrypter(encrypterData);
        }

        #endregion

        #region EncrypterData

        private class EncrypterData
        {
            internal int BaseByteOffset { get; }
            internal byte? BaseByteXOR { get; }
            internal byte? FirstByteXOR { get; }
            internal bool FixFirstByte { get; }

            internal EncrypterData(int baseByteOffset, byte? baseByteXor, byte? firstByteXor, bool fixFirstByte = true)
            {
                this.BaseByteOffset = baseByteOffset;
                this.BaseByteXOR = baseByteXor;
                this.FirstByteXOR = firstByteXor;
                this.FixFirstByte = fixFirstByte;
            }
        }

        #endregion
    }
}
