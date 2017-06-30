using HPMacroComponents;

namespace MacroPLC
{
    public class MacroLexicalScanner
    {
        private const int INVALID_INDEX = -1;

        private string source;
        public MacroLexicalScanner(string source)
        {
            Source = source;
        }

        public string Source
        {
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new InvalidSourceCodeException("Source is null or empty");
                source = value;
                currentIndex = 0;
            }
        }

        private int currentIndex;
        public int CurrentIndex
        {
            get
            {
                if (currentIndex >= source.Length)
                    return INVALID_INDEX;
                return currentIndex;
            }
        }

        public Token ScanNext()
        {
            if (currentIndex >= source.Length)
                return null;

            var c = lookNextChar();

            if(char.IsWhiteSpace(c))
                return new Token(getWhiteToken(), TokenType.WHITE_SPACE);

            if(char.IsDigit(c))
                return new Token(getNumberToken(), TokenType.NUMBER);
            
            return null;
        }

        private string getNumberToken()
        {
            var number = string.Empty;
            number += getPositiveNaturalNumber();

            if(lookNextChar() == '.')
            {
                number += getNextChar();
                number += getPositiveNaturalNumber();
            }
            return number;
        }

        private string getPositiveNaturalNumber()
        {
            var num = string.Empty;
            while (char.IsNumber(lookNextChar()))
            {
                num += getNextChar();
            }
            return num;
        }

        private string getWhiteToken()
        {
            var whiteStr = string.Empty;
            var c = lookNextChar();
            while(char.IsWhiteSpace(c))
            {
                whiteStr += getNextChar();
                c = lookNextChar();
            }
            return whiteStr;
        }

        private char getNextChar()
        {
            if (CurrentIndex == INVALID_INDEX)
                return (char) 0;
            return source[currentIndex++];
        }

        private char lookNextChar()
        {
            if (CurrentIndex == INVALID_INDEX)
                return (char)0;
            return source[CurrentIndex];
        }
    }
}