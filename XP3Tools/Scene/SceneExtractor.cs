using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace XP3Tools.Scene
{
    public class SceneExtractor
    {
        #region Properties & Fields

        private static readonly Regex REGEX_SCRIPT_LINES = new Regex(@"<(?<number>\d*?)> *(?<text>.*)", RegexOptions.Compiled);

        private string _baseDirectory;
        private string _scriptDirectory;
        private string _sceneDirectory;

        #endregion

        #region Constructors

        public SceneExtractor(string baseDirectory)
        {
            this._baseDirectory = baseDirectory;

            _scriptDirectory = _baseDirectory + ".scripts";
            _sceneDirectory = Path.Combine(_baseDirectory, "scn");
        }

        #endregion

        #region Methods

        public void Pack()
        {
            foreach (string templateFile in Directory.GetFiles(_scriptDirectory, "*.txt"))
            {
                string sceneFile = Path.Combine(_sceneDirectory, Path.GetFileNameWithoutExtension(templateFile));

                StringExtractor stringExtractor = new StringExtractor();
                stringExtractor.Import(sceneFile);

                string[] templateLines = File.ReadAllLines(templateFile);
                foreach (string line in templateLines.Select(x => x.Trim()).Where(x => !x.StartsWith("//")))
                {
                    Match m = REGEX_SCRIPT_LINES.Match(line);
                    if (m.Success)
                        stringExtractor.Strings[int.Parse(m.Groups["number"].Value)] = m.Groups["text"].Value;
                }

                stringExtractor.Export(Path.Combine(_scriptDirectory, Path.GetFileName(sceneFile)));
            }
        }

        public void Unpack()
        {
            if (Directory.Exists(_scriptDirectory))
                Directory.Delete(_scriptDirectory, true);
            Directory.CreateDirectory(_scriptDirectory);

            IEnumerable<string> sceneFiles = Directory.GetFiles(_sceneDirectory, "*.scn");
            foreach (string sceneFile in sceneFiles)
            {
                string fileTemplate = Path.Combine(_scriptDirectory, Path.GetFileName(sceneFile) + ".txt");

                int lineCounter = 0;

                StringExtractor stringExtractor = new StringExtractor();
                byte[] data = File.ReadAllBytes(sceneFile);
                stringExtractor.Import(data);

                List<string> templateLines = new List<string>();
                foreach (string line in stringExtractor.Strings)
                    templateLines.Add($"<{(lineCounter++):0000}> {line}");
                File.AppendAllLines(fileTemplate, templateLines);
            }
        }

        #endregion
    }
}
