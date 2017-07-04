using System.Collections.Generic;
namespace MacroPLC
{
    public class SourceManager
    {
        private List<string> sourceLines = new List<string>();
        public SourceManager(string source)
        {
            GetSourceLines(source);
        }

        private void GetSourceLines(string source)
        {
            var reader = new SourceReader(source);
            var lineContent = reader.ReadNextLine();
            while (lineContent != null)
            {
                sourceLines.Add(lineContent);
                lineContent = reader.ReadNextLine();
            }
        }

        public int CurrentLine { get; private set; }
        /// <summary>
        /// Get current line string and increase index by 1
        /// </summary>
        /// <returns></returns>
        private string GetCurrentLine()
        {
            if (CurrentLine >= sourceLines.Count)
            {
                CurrentLine = sourceLines.Count;
                return null;
            }

            return sourceLines[CurrentLine++];
        }

        /// <summary>
        /// Get next line string of source code
        /// </summary>
        /// <param name="lineIndex">First line index is 0</param>
        /// <returns></returns>
        public string GetNextLine(out int lineIndex)
        {
            lineIndex = CurrentLine;
            return GetCurrentLine();
        }

        /// <summary>
        /// Get line string at specific index of source code
        /// </summary>
        /// <param name="index">First line index is 0 </param>
        /// <returns></returns>
        public string GetLineAt(int index)
        {
            CurrentLine = index;
            return GetCurrentLine();
        }
    }
}