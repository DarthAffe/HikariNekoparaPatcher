using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace XP3Tools.Scene
{
    // Based on marcussacana - KrKrZSceneManager
    public class StringExtractor
    {
        #region Properties & Fields

        private int _offsetLength;
        private int _stringTable;
        private int _offsetTable;
        private int _stringCount;
        private byte[] _source;
        private byte[] _sufix;

        private int _tblHrdLen = 0;

        public string[] Strings { get; private set; } = new string[0];
        public bool ResizeOffsets { get; set; } = false;

        public bool Initialized { get; private set; }

        #endregion

        #region Methods

        public void Import(string file)
        {
            Import(File.ReadAllBytes(file));
        }

        public void Import(byte[] packget)
        {
            _source = packget;

            //Initialize variables
            _offsetTable = GetOffset(_source, 16, 4);
            _stringTable = GetOffset(_source, 20, 4);
            _offsetLength = GetOffsetSize(_source);
            _tblHrdLen = GetPrefixSize(_source);
            _stringCount = GetStrCount(_source);

            //Get strings
            Strings = new string[_stringCount];
            int offset = 0;
            for (int i = 0; i < _stringCount; i++)
            {
                offset = _offsetTable + _tblHrdLen + (i * _offsetLength);
                offset = GetOffset(_source, offset, _offsetLength) + _stringTable;
                Strings[i] = _source[offset] == 0x00 ? string.Empty : GetString(_source, offset);
            }

            //Get end-position of the last string
            while (_source[offset] != 0x00)
                offset++;

            //Get all data after the string table
            int sufixPos = offset + 1;
            if (sufixPos < _source.Length)
            {
                int length = _source.Length - sufixPos;
                _sufix = new byte[length];
                Array.Copy(_source, sufixPos, _sufix, 0, length);
            }

            Initialized = true;
        }

        public void Export(string file)
        {
            File.WriteAllBytes(file, Export());
        }

        public byte[] Export()
        {
            if (!Initialized)
                throw new Exception("You need import a scene before export.");

            //Copy script-backup;
            byte[] script = new byte[_offsetTable + _tblHrdLen];
            Array.Copy(_source, 0, script, 0, script.Length);

            //Get offset size or update if needed
            int offsetSize = _offsetLength;
            if (ResizeOffsets)
            {
                script[script.Length - 1] = ConvertSize(4);
                offsetSize = 4;
                GenOffset(offsetSize, _offsetTable + _tblHrdLen + (_stringCount * offsetSize)).CopyTo(script, 0x14);
            }

            //Generate string and offset table
            byte[] offsets = new byte[_stringCount * offsetSize];
            MemoryStream buffer = new MemoryStream();
            for (int pos = 0; pos < Strings.Length; pos++)
            {
                GenOffset(offsetSize, (int)buffer.Length).CopyTo(offsets, pos * offsetSize); //Write offset

                //Append string
                byte[] str = Encoding.UTF8.GetBytes(Strings[pos] + "\x0");
                buffer.Write(str, 0, str.Length);
            }
            byte[] strings = buffer.ToArray();
            buffer.Close();

            //Merge all data
            IEnumerable<byte> outScript = script;
            outScript = outScript.Concat(offsets);
            outScript = outScript.Concat(strings);
            outScript = outScript.Concat(_sufix);
            script = outScript.ToArray();

            //Update offsets
            int startPos = GetOffset(_source, 0x20, 4),
            resOffPos = GetOffset(_source, 0x18, 4),
            resSizePos = GetOffset(_source, 0x1C, 4);
            int diff = script.Length - _source.Length;

            if (startPos > _stringTable) //If is after string table...
                GenOffset(4, startPos + diff).CopyTo(script, 0x20); //Update the difference
            if (resOffPos > _stringTable)
                GenOffset(4, resOffPos + diff).CopyTo(script, 0x18);
            if (resSizePos > _stringTable)
                GenOffset(4, resSizePos + diff).CopyTo(script, 0x1C);
            
            return script;
        }

        private byte[] Shortdword(byte[] off)
        {
            int length = off.Length - 1;
            while (off[length] == 0x0 && length > 0)
                length--;

            byte[] rst = new byte[length + 1];
            for (int i = 0; i < rst.Length; i++)
                rst[i] = off[i];

            return rst;
        }

        private byte[] GenOffset(int size, int value)
        {
            byte[] off = BitConverter.GetBytes(value);
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(off);

            off = Shortdword(off);

            if (off.Length > size)
                throw new Exception("Edited Strings are too big.");

            if (off.Length < size)
            {
                byte[] rst = new byte[size];
                off.CopyTo(rst, 0);
                off = rst;
            }

            return off;
        }

        private int GetOffsetSize(byte[] file)
        {
            int pos = GetOffset(file, 0x10, 4);
            int firstSize = ConvertSize(file[pos++]);
            return ConvertSize(file[firstSize + pos]);
        }

        private int GetStrCount(byte[] file)
        {
            int pos = GetOffset(file, 0x10, 4);
            int size = ConvertSize(file[pos++]);
            return GetOffset(file, pos, size);
        }

        private int GetPrefixSize(byte[] file)
        {
            return ConvertSize(file[GetOffset(file, 0x10, 4)]) + 2;
        }

        private byte ConvertSize(int s)
        {
            switch (s)
            {
                case 1:
                    return 0xD;
                case 2:
                    return 0xE;
                case 3:
                    return 0xF;
                case 4:
                    return 0x10;
                default:
                    throw new Exception("Unknow Offset Size");
            }
        }

        private int ConvertSize(byte b)
        {
            switch (b)
            {
                case 0xD:
                    return 1;
                case 0xE:
                    return 2;
                case 0xF:
                    return 3;
                case 0x10:
                    return 4;
                default:
                    throw new Exception("Unknow Offset Size");
            }
        }

        private string GetString(byte[] scene, int pos)
        {
            MemoryStream arr = new MemoryStream();
            for (int i = pos; scene[i] != 0x00 && i + 1 < scene.Length; i++)
                arr.Write(new[] { scene[i] }, 0, 1);

            byte[] rst = new byte[arr.Length];
            arr.Position = 0;
            arr.Read(rst, 0, (int)arr.Length);
            arr.Close();

            return Encoding.UTF8.GetString(rst);
        }

        private int GetOffset(byte[] file, int index, int offsetSize)
        {
            byte[] value = new byte[4];
            Array.Copy(file, index, value, 0, offsetSize);

            if (!BitConverter.IsLittleEndian) //Force Little Endian DWORD
                Array.Reverse(value, 0, 4);

            return BitConverter.ToInt32(value, 0);
        }

        #endregion
    }
}
