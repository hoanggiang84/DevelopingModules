using System.Collections.Generic;
namespace MacroPLC
{
    public class SourceManager
    {
        private List<SourceLine> sourceLines = new List<SourceLine>();
        public SourceManager(string source)
        {
            ExtractSourceLines(source);
        }

        private void ExtractSourceLines(string source)
        {
            var reader = new SourceReader(source);
            var line_content = reader.ReadNextLine();
            var line_num = 0;
            while (line_content != null)
            {
                sourceLines.Add(new SourceLine(line_content, line_num++));
                line_content = reader.ReadNextLine();
            }
        }

        public int CurrentLine { get; private set; }

        public void Reset()
        {
            CurrentLine = 0;
        }

        /// <summary>
        /// Get current line and increase index by 1
        /// </summary>
        /// <returns></returns>
        private SourceLine GetCurrentLine()
        {
            if (CurrentLine >= sourceLines.Count)
            {
                CurrentLine = sourceLines.Count;
                return null;
            }

            return sourceLines[CurrentLine++];
        }

        /// <summary>
        /// Get next source code line
        /// </summary>
        /// <param name="lineIndex">First line index is 0</param>
        /// <returns></returns>
        public SourceLine GetNextLine(out int lineIndex)
        {
            lineIndex = CurrentLine;
            return GetCurrentLine();
        }

        /// <summary>
        /// Look current line and increase index by 1
        /// </summary>
        /// <returns></returns>
        public SourceLine LookCurrentLine(out int lineIndex)
        {
            lineIndex = CurrentLine;
            if (CurrentLine >= sourceLines.Count)
            {
                CurrentLine = sourceLines.Count;
                return null;
            }
            return sourceLines[CurrentLine];
        }

        /// <summary>
        /// Get line at specific index of source code
        /// </summary>
        /// <param name="index">First line index is 0 </param>
        /// <returns></returns>
        public SourceLine GetLineAt(int index)
        {
            CurrentLine = index;
            return GetCurrentLine();
        }
    }
}