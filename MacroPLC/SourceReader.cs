using UtilitiesVS2008WinCE;

namespace MacroPLC
{
    public class SourceReader
    {
        private string source;
        public SourceReader(string source)
        {
            this.source = source;
        }

        public string ReadNextLine()
        {
            if (source == string.Empty)
                return string.Empty;
            return null;
        }
    }
}