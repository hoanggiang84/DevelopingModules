using System.Globalization;
using System.Linq;
using HPMacroCommon;
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
                return currentIndex >= source.Length ? INVALID_INDEX : currentIndex;
            }
        }

        public Token ScanNext()
        {
            if (currentIndex >= source.Length)
                return null;

            var c = lookNextChar();

            if(char.IsWhiteSpace(c))
                return new Token(getWhiteString(), TokenType.WHITE_SPACE);

            if(char.IsDigit(c))
                return new Token(getNumberString(), TokenType.NUMBER);
            
            if(IsValidIdentifierChar(c))
                return new Token(getIdentifierString(), TokenType.IDENTIFIER);

            if(IsValidSymbol(c))
                return new Token(getSymbolString(), TokenType.SYMBOL);

            return getUndefinedString();
        }

        private static bool IsValidIdentifierChar(char c)
        {
            return char.IsLetter(c) && (c < 128);
        }

        private Token getUndefinedString()
        {
            var undefined = string.Empty;
            while (IsUndefinedChar(lookNextChar()) && currentIndex < source.Length)
            {
                undefined += getNextChar();
            }
            return new Token(undefined, TokenType.UNDEFINED);
        }

        private bool IsUndefinedChar(char c)
        {
            return !(char.IsWhiteSpace(c) | char.IsDigit(c) | IsValidIdentifierChar(c) | IsValidSymbol(c));
        }

        private string getSymbolString()
        {
            var c = getNextChar();
            var symbol = string.Empty;
            symbol += c;
            switch (c)
            {
                case '<':
                    if (lookNextChar() == '=')
                    {
                        getNextChar();
                        return "<=";
                    }
                    if (lookNextChar() == '>')
                    {
                        getNextChar();
                        return "<>";
                    }
                    break;

                case '>':
                    if (lookNextChar() == '=')
                    {
                        getNextChar();
                        return ">=";
                    }
                    break;

                case '/':
                    if (lookNextChar() == '*')
                    {
                        getNextChar();
                        return "/*";
                    }
                    if (lookNextChar() == '/')
                    {
                        getNextChar();
                        return "//";
                    }
                    break;

                case '*':
                    if (lookNextChar() == '/')
                    {
                        getNextChar();
                        return "*/";
                    }
                    break;
            }

            return symbol;
        }

        private bool IsValidSymbol(char c)
        {
            return MacroKeywords.ValidSymbols
                .Any(word => word.StartsWith(c.ToString(CultureInfo.InvariantCulture)));
        }

        private string getIdentifierString()
        {
            var ident = string.Empty;
            while (char.IsLetter(lookNextChar()))
                ident += getNextChar();
            return ident;
        }

        private string getNumberString()
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

        private string getWhiteString()
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