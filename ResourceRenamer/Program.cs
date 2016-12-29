using System;
using System.IO;

namespace ResourceRenamer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // % is a reserved character in VisualStudio - we need to escape it to include the images
            string resourceDir = Path.Combine(Environment.CurrentDirectory, @"..\..\..\HikariNekoparaPatcher\PatchData");
            EscapeNames(resourceDir);
        }

        private static void EscapeNames(string directory)
        {
            foreach (string dir in Directory.GetDirectories(directory))
                EscapeNames(dir);

            foreach (string file in Directory.GetFiles(directory))
                if (file.Contains("%"))
                    File.Move(file, file.Replace("%", "___percent___"));
        }
    }
}
