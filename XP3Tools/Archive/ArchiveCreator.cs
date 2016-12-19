using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Ionic.Zlib;
using XP3Tools.Data;
using XP3Tools.Extensions;

namespace XP3Tools.Archive
{
    public class ArchiveCreator
    {
        #region Properties & Fields

        private IChunkDataWrapper _data;
        private IChunkDataWrapper _index = new MemoryChunkDataWrapper();
        private Dictionary<string, int> _offsets = new Dictionary<string, int>();

        private string _workingDirectory;
        private string _infoDirectory;

        private bool _isIndexWritten = false;

        #endregion

        #region Constructors

        public ArchiveCreator(IChunkDataWrapper archiveWrapper, string workingDirectory, string infoDirectory)
        {
            this._workingDirectory = workingDirectory;
            this._infoDirectory = infoDirectory;
            this._data = archiveWrapper;

            WriteHeader();
        }

        #endregion

        #region Methods

        public void AddFile(string file)
        {
            if (_isIndexWritten) throw new Exception("Index already written");

            string fileName = file.Replace(_workingDirectory, string.Empty).Replace('\\', '/');
            if (fileName.StartsWith("/") || fileName.StartsWith("\\"))
                fileName = fileName.Substring(1);

            using (MemoryFileDataWrapper data = MemoryFileDataWrapper.FromFile(file))
            {
                int adler = BitConverter.ToInt32(File.ReadAllBytes(Path.Combine(_infoDirectory, fileName + ".crc")), 0);

                byte[] key = File.ReadAllBytes(Path.Combine(_infoDirectory, fileName + ".key"));
                if (!file.EndsWith(".ini"))
                    Encrypter.ForFile(file).Encrypt(data, key[0]);
                int uncompresedSize = data.Size;

                int cachedOffset;
                bool isDuplicate =
                    _offsets.TryGetValue(uncompresedSize.ToString() + adler.ToString(), out cachedOffset) &&
                    !file.EndsWith(".ini");

                if (File.Exists(Path.Combine(_infoDirectory, fileName + ".zip")))
                    data.Compress();

                // eliF
                _index.WriteString("eliF");
                _index.WriteLong(sizeof(int) + sizeof(short) + fileName.Length * 2 + 2); // chunkSize
                _index.WriteInt(adler);
                _index.WriteShort((short)fileName.Length);
                _index.WriteUnicodeString(fileName);
                _index.WriteBytes(0x00, 0x00);

                // File
                _index.WriteString("File");
                _index.WriteLong((4 + sizeof(long) * 2) + (4 + (sizeof(long) + sizeof(int))) +
                    (4 + sizeof(int) + (sizeof(long) * 4)) + (4 + sizeof(long) + 88)); // chunkSize

                // time
                _index.WriteString("time");
                _index.WriteLong(sizeof(long)); // chunkSize
                _index.WriteBytes(0x80, 0xAD, 0x3D, 0xA9, 0x49, 0x01, 0x00, 0x00); // random value from one file

                // adlr
                _index.WriteString("adlr");
                _index.WriteLong(sizeof(int)); // chunkSize
                _index.WriteInt(adler);

                // segm
                _index.WriteString("segm");
                _index.WriteLong(sizeof(int) + (sizeof(long) * 3)); // chunkSize
                _index.WriteInt(data.IsCompressed ? 1 : 0); // zip
                int offset = isDuplicate ? cachedOffset : _data.Size;
                _index.WriteLong(offset); // offset
                _index.WriteLong(uncompresedSize); // size
                _index.WriteLong(data.Size); // zsize

                if (!isDuplicate && !file.EndsWith(".ini"))
                    _offsets.Add(uncompresedSize.ToString() + adler.ToString(), offset);

                // info
                _index.WriteString("info");
                byte[] info = File.ReadAllBytes(Path.Combine(_infoDirectory, fileName));
                using (IChunkDataWrapper replaceData = new MemoryChunkDataWrapper())
                {
                    replaceData.WriteLong(uncompresedSize);
                    replaceData.WriteLong(data.Size);
                    byte[] replaceDataBytes = replaceData.Bytes;
                    for (int i = 0; i < replaceData.Size; i++)
                        info[4 + i] = replaceDataBytes[i];
                }

                _index.WriteLong(info.Length); // chunkSize
                _index.WriteBytes(info); // I have no idea how this is created ...

                // data
                if (!isDuplicate)
                    _data.WriteBytes(data.Bytes);
            }
        }

        public void WriteIndex()
        {
            byte[] compressedIndex = ZlibStream.CompressBuffer(_index.Bytes);

            _data.ReplaceInt(0x20, _data.Size); // offset
            _data.WriteBytes(0x01); // zip index
            _data.WriteLong(compressedIndex.Length); // zsize
            _data.WriteLong(_index.Size); // size

            _data.WriteBytes(compressedIndex);

            _isIndexWritten = true;
        }

        private void WriteHeader()
        {
            byte[] header = Assembly.GetExecutingAssembly().GetManifestResource("XP3Tools.PredefinedBinaries.header.bin");

            _data.WriteBytes(header);

            // this file is part of the header
            // eliF
            string file = "$$$ This is a protected archive. $$$ 著作者はこのアーカイブが正規の利用方法以外の方法で展開されることを望んでいません。 $$$ This is a protected archive. $$$ 著作者はこのアーカイブが正規の利用方法以外の方法で展開されることを望んでいません。 $$$ This is a protected archive. $$$ 著作者はこのアーカイブが正規の利用方法以外の方法で展開されることを望んでいません。 $$$ Warning! Extracting this archive may infringe on author's rights. 警告 このアーカイブを展開することにより、あなたは著作者の権利を侵害するおそれがあります。.txt";
            string fileName = file.Replace(_workingDirectory, string.Empty).Replace('\\', '/');
            if (fileName.StartsWith("/") || fileName.StartsWith("\\"))
                fileName = fileName.Substring(1);

            _index.WriteString("eliF");
            _index.WriteLong(sizeof(int) + sizeof(short) + fileName.Length * 2 + 2); // chunkSize
            _index.WriteInt(1754764004); // adler
            _index.WriteShort((short)fileName.Length);
            _index.WriteUnicodeString(fileName);
            _index.WriteBytes(0x00, 0x00);

            // File
            _index.WriteString("File");
            _index.WriteLong(156); // chunkSize

            // adlr
            _index.WriteString("adlr");
            _index.WriteLong(sizeof(int)); // chunkSize
            _index.WriteInt(1754764004);

            // segm
            _index.WriteString("segm");
            _index.WriteLong(sizeof(int) + (sizeof(long) * 3)); // chunkSize
            _index.WriteInt(0); // No zip so far
            _index.WriteLong(88); // offset
            _index.WriteLong(157); // size
            _index.WriteLong(157); // zsize

            // info
            byte[] info = Assembly.GetExecutingAssembly().GetManifestResource("XP3Tools.PredefinedBinaries.info.bin");

            _index.WriteString("info");
            _index.WriteLong(info.Length); // chunkSize
            _index.WriteBytes(info); // I have no idea how this is created ...
        }

        #endregion
    }
}
