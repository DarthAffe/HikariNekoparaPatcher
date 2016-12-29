using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Win32;

namespace HikariNekoparaPatcher.Services
{
    public class InstallPathService
    {
        #region Constants

        private const string GAME_EXE = "nekopara_vol1.exe";
        private static readonly string[] CHECK_FILES = { GAME_EXE, "data.xp3" };

        #endregion

        #region Methods

        public string GetPathIfDefault()
        {
            string[] subFolders =
            {
                @"Steam\steamapps\common\NEKOPARA Vol. 1",
                @"Steam\steamapps\common\NEKOPARA Vol1",
                @"Steam\steamapps\common\NEKOPARA_Vol. 1",
                @"Steam\steamapps\common\NEKOPARA_Vol1",
                @"NEKO WORKs\nekopara vol. 1",
                @"NEKO WORKs\nekopara vol1",
                @"NEKO WORKs\nekopara_vol. 1",
                @"NEKO WORKs\nekopara_vol1"
            };

            List<string> pathsToTry = new List<string>();
            foreach (string subFolder in subFolders)
            {
                pathsToTry.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), subFolder));
                pathsToTry.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), subFolder));
                pathsToTry.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), subFolder));
            }

            foreach (string pathToTry in pathsToTry)
                if (CheckPath(pathToTry))
                    return pathToTry;

            return null;
        }

        public string BrowseGamePath()
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                CheckFileExists = true,
                CheckPathExists = true,
                Filter = $"Nekopara-Exe|{GAME_EXE}",
                Multiselect = false,
                Title = "Nekopara Installationsverzeichnis suchen"
            };

            if (ofd.ShowDialog() == true && CheckPath(ofd.FileName))
                return Path.GetDirectoryName(ofd.FileName);

            return null;
        }

        public bool CheckPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return false;

            if (File.Exists(path))
                path = Path.GetDirectoryName(path);

            if (!Directory.Exists(path)) return false;

            return CHECK_FILES.All(x => File.Exists(Path.Combine(path, x)));
        }

        public string GetExePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return null;

            path = Path.Combine(path, GAME_EXE);
            return File.Exists(path) ? path : null;
        }

        #endregion
    }
}
