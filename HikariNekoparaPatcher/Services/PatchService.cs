using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using XP3Tools.Archive;
using XP3Tools.Data;
using XP3Tools.Extensions;

namespace HikariNekoparaPatcher.Services
{
    public class PatchService
    {
        #region Constants

        private const string FOLDER_TMP_DATA = "HikariTmp";

        #endregion

        #region Properties & Fields

        #endregion

        #region Constructors

        #endregion

        #region Methods

        public void ApplyPatch(string gameDirectory)
        {
            string workingDirectory = Path.Combine(gameDirectory, FOLDER_TMP_DATA);
            if (!Directory.Exists(workingDirectory))
                Directory.CreateDirectory(workingDirectory);

            IEnumerable<IGrouping<string, Tuple<string, string>>> resources = GetType().Assembly.GetManifestResourceNames()
                .Where(x => x.StartsWith("HikariNekoparaPatcher.PatchData."))
                .Select(x => x.Replace("HikariNekoparaPatcher.PatchData.", string.Empty))
                .Select(x => new Tuple<string, string>(x.Substring(0, x.IndexOf('.', x.IndexOf('.') + 1)), x.Substring(x.IndexOf('.', x.IndexOf('.') + 1) + 1)))
                .GroupBy(x => x.Item1);
            foreach (IGrouping<string, Tuple<string, string>> files in resources)
            {
                string file = files.Key;
                ExtractOriginalData(gameDirectory, workingDirectory, file);
                InsertPatchData(workingDirectory, file, files.Select(x => x.Item2));
                RepackData(gameDirectory, workingDirectory, file);
                Cleanup(workingDirectory);
            }
        }

        private void ExtractOriginalData(string gameDirectory, string workingDirectory, string file)
        {
            string sourceFile = Path.Combine(gameDirectory, file);
            string targetDirectory = Path.Combine(workingDirectory, Path.GetFileName(file) + ".extracted");
            string infoDirectory = Path.Combine(workingDirectory, Path.GetFileName(file) + ".info");

            using (IFileDataWrapper data = StreamFileDataWrapper.FromFile(sourceFile))
            {
                ArchiveExtractor archiveExtractor = new ArchiveExtractor(targetDirectory, infoDirectory, data);
                archiveExtractor.Unpack();
            }
        }

        private void InsertPatchData(string workingDirectory, string file, IEnumerable<string> files)
        {
            string targetDirectory = Path.Combine(workingDirectory, Path.GetFileName(file) + ".extracted");
            foreach (string path in files)
            {
                int lastSuccessfullIndex = -1;
                for (int i = 0; i < path.Length; i++)
                    if (path[i] == '.')
                    {
                        string directory = path.Substring(0, i).Replace('.', '\\');
                        if (Directory.Exists(Path.Combine(targetDirectory, directory)))
                            lastSuccessfullIndex = i;
                        else break;
                    }
                if (lastSuccessfullIndex > -1)
                {
                    string targetFile = Path.Combine(targetDirectory, path.Substring(0, lastSuccessfullIndex).Replace('.', '\\'), path.Substring(lastSuccessfullIndex + 1));
                    File.WriteAllBytes(targetFile, GetType().Assembly.GetManifestResource($"HikariNekoparaPatcher.PatchData.{file}.{path}"));
                }
            }
        }

        private void RepackData(string gameDirectory, string workingDirectory, string file)
        {
            string sourceDirectory = Path.Combine(workingDirectory, Path.GetFileName(file) + ".extracted");
            string infoDirectory = Path.Combine(workingDirectory, Path.GetFileName(file) + ".info");
            string targetFile = Path.Combine(gameDirectory, file);

            if (File.Exists(targetFile))
                File.Delete(targetFile);

            using (FileStream fs = File.Create(targetFile))
            {
                IChunkDataWrapper data = new StreamChunkDataWrapper(fs);
                ArchiveCreator creator = new ArchiveCreator(data, sourceDirectory, infoDirectory);
                AddFiles(creator, sourceDirectory);
                creator.WriteIndex();
            }
        }

        private static void AddFiles(ArchiveCreator creator, string srcDirecory)
        {
            foreach (string file in Directory.GetFiles(srcDirecory))
                creator.AddFile(file);

            foreach (string directory in Directory.GetDirectories(srcDirecory))
                AddFiles(creator, directory);
        }

        private void Cleanup(string workingDirectory)
        {
            Directory.Delete(workingDirectory, true);
        }

        #endregion
    }
}
