using System;
using System.IO;
using XP3Tools.Data;

namespace XP3Tools.Archive
{
    // Based on the nekopara quickbms script
    public class ArchiveExtractor
    {
        #region Properties & Fields

        private string _baseDirectory;
        private string _infoDirectory;
        private IFileDataWrapper _data;

        #endregion

        #region Constructors

        public ArchiveExtractor(string baseDirectory, string infoDirectory, IFileDataWrapper data)
        {
            this._baseDirectory = baseDirectory;
            this._infoDirectory = infoDirectory;
            this._data = data;

            string id = _data.GetString(3);
            if (id != "XP3") throw new InvalidDataException("Invalid archive format");
            _data.GoTo(0);
        }

        #endregion

        #region Methods

        public void Unpack()
        {
            if (Directory.Exists(_baseDirectory))
                Directory.Delete(_baseDirectory, true);

            _data.GoTo(0x20);
            int offset = _data.GetInt();
            _data.GoTo(offset);

            byte zip = _data.GetByte();
            int zSize = _data.GetLong();
            int size = _data.GetLong();
            offset = _data.Location;
            using (IFileDataWrapper archive = zip != 0 ? _data.GetCompressedData(offset, zSize, size) : _data.GetData(offset, zSize))
                Extract(archive, archive.Size, string.Empty);
        }

        private void Extract(IFileDataWrapper archive, int chunkSize, string name)
        {
            int chunkOffset = archive.Location;
            int chunkLimit = chunkOffset + chunkSize;
            while (chunkOffset != chunkLimit)
            {
                string chunkType = archive.GetString(4);
                chunkSize = archive.GetLong();
                int location = archive.Location;
                switch (chunkType)
                {
                    case "File":
                        Extract(archive, chunkSize, name);
                        break;

                    case "neko":
                    case "eliF":
                        int adlr = archive.GetInt();
                        short namesz = archive.GetShort();

                        if (namesz <= 0x100)
                            name = archive.GetUnicodeString(namesz);
                        else
                            archive.GetUnicodeString(namesz);

                        break;

                    case "adlr":
                        if (!string.IsNullOrEmpty(name))
                            WriteToFile(Path.Combine(_infoDirectory, name + ".crc"), archive.GetBytes(sizeof(int)));
                        else
                            archive.GetInt();
                        break;

                    case "time":
                        int timestamp = archive.GetLong();
                        break;

                    case "segm":
                        int zip = archive.GetInt();
                        int offset = archive.GetLong();
                        int size = archive.GetLong();
                        int zSize = archive.GetLong();

                        using (IFileDataWrapper segment = zip != 0 ? _data.GetCompressedData(offset, zSize, size) : _data.GetData(offset, zSize))
                            if (!string.IsNullOrEmpty(name))
                            {
                                string targetFile = Path.Combine(_baseDirectory, name);
                                byte key = 0;
                                if (!name.EndsWith(".ini"))
                                    key = Encrypter.ForFile(name).Decrypt(segment);
                                WriteToFile(targetFile, segment.Bytes);

                                string keyFile = Path.Combine(_infoDirectory, name + ".key");
                                WriteToFile(keyFile, new[] { key });

                                if (zip != 0)
                                    using (File.Create(Path.Combine(_infoDirectory, name + ".zip"))) ;
                            }
                        break;
                    case "info":
                        if (!string.IsNullOrEmpty(name))
                        {
                            string infoFile = Path.Combine(_infoDirectory, name);
                            Directory.CreateDirectory(Path.GetDirectoryName(infoFile));
                            WriteToFile(infoFile, archive.GetBytes(chunkSize));
                        }
                        break;
                }
                chunkOffset = location + chunkSize;
                archive.GoTo(chunkOffset);
            }
        }

        private void WriteToFile(string file, byte[] bytes)
        {
            if (!Directory.Exists(Path.GetDirectoryName(file)))
                Directory.CreateDirectory(Path.GetDirectoryName(file));
            File.WriteAllBytes(file, bytes);
        }

        #endregion
    }
}
