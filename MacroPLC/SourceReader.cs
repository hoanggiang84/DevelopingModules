using System;
using UtilitiesVS2008WinCE;

namespace MacroPLC
{
    public class SourceReader
    {
        private string source;
        public SourceReader(string source)
        {
            CurrentIndex = 0;
            Source = source;
        }

        public string Source
        {
            get { return source; } 
            set
            {
                if (value.IsNullOrWhite())
                    throw new InvalidSourceCodeException("Source is null or only white space");
                source = value;
            }
        }

        public int CurrentIndex { get; private set; }

        public string ReadNextLine()
        {
            if (CurrentIndex >= source.Length)
                return null;

            var lineContent = string.Empty;
            while (CurrentIndex < source.Length)
            {
                var currentChar = source[CurrentIndex++];
                if(currentChar == '\n')
                    break;
                lineContent += currentChar;
            }

            return lineContent.IsNullOrWhite() ? string.Empty : lineContent;
        }

        public void ResetIndex()
        {
            CurrentIndex = 0;
        }
    }

    public class InvalidSourceCodeException : Exception
    {
        public InvalidSourceCodeException(string msg) :
            base(msg)
        {
        }
    }
}