using System;
using System.Globalization;
using System.Linq;
using HPMacroCommon;

namespace MacroLexScn
{
    public class MacroLexicalScanner
    {
        private const int INVALID_INDEX = -1;

        private string _source;
        public MacroLexicalScanner(string source)
        {
            Source = source;
        }

        public string Source
        {
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentException("Empty source");
                _source = value;
                _current_index = 0;
            }
        }

        private int _current_index;
        public int CurrentIndex
        {
            get
            {
                return _current_index >= _source.Length ? INVALID_INDEX : _current_index;
            }
        }

        public Token ScanNext()
        {
            if (_current_index >= _source.Length)
                return new Token(string.Empty, TokenType.END);

            var c = look_next_char();

            if(char.IsWhiteSpace(c))
                return new Token(get_white_string(), TokenType.WHITE_SPACE);

            if(char.IsDigit(c))
                return new Token(get_number_string(), TokenType.NUMBER);
            
            if(local_variable_char(c))
                return new Token(get_variable(), TokenType.LOCAL_VAR);

            if(global_variable_char(c))
                return new Token(get_variable(),TokenType.GLOBAL_VAR);

            if(valid_identifier_char(c))
                return new Token(get_identifier_string(), TokenType.IDENTIFIER);

            if(valid_symbol(c))
                return new Token(get_symbol_string(), TokenType.SYMBOL);

            return get_undefined_string();
        }

        private string get_variable()
        {
            var c = look_next_char();
            var varStr = string.Empty;
            if (local_variable_char(c) || global_variable_char(c))
                varStr += get_next_char();
            else
                return varStr;

            varStr += get_positive_natural_number();
            return varStr;
        }

        private static bool global_variable_char(char c)
        {
            return c == '@';
        }

        private static bool local_variable_char(char c)
        {
            return c == '#';
        }

        private static bool valid_identifier_char(char c)
        {
            return char.IsLetter(c) && (c < 128);
        }

        private Token get_undefined_string()
        {
            var undefined = string.Empty;
            while (undefined_char(look_next_char()) && _current_index < _source.Length)
            {
                undefined += get_next_char();
            }
            return new Token(undefined, TokenType.UNDEFINED);
        }

        private bool undefined_char(char c)
        {
            return !(char.IsWhiteSpace(c) | char.IsDigit(c) | valid_identifier_char(c) | valid_symbol(c));
        }

        private string get_symbol_string()
        {
            var c = get_next_char();
            var symbol = string.Empty;
            symbol += c;
            switch (c)
            {
                case '<':
                    if (look_next_char() == '=')
                    {
                        get_next_char();
                        return "<=";
                    }
                    if (look_next_char() == '>')
                    {
                        get_next_char();
                        return "<>";
                    }
                    break;

                case '>':
                    if (look_next_char() == '=')
                    {
                        get_next_char();
                        return ">=";
                    }
                    break;

                case '/':
                    if (look_next_char() == '*')
                    {
                        get_next_char();
                        return "/*";
                    }
                    if (look_next_char() == '/')
                    {
                        get_next_char();
                        return "//";
                    }
                    break;

                case '*':
                    if (look_next_char() == '/')
                    {
                        get_next_char();
                        return "*/";
                    }
                    break;
            }

            return symbol;
        }

        private bool valid_symbol(char c)
        {
            return MacroKeywords.ValidSymbols
                .Any(word => word.StartsWith(c.ToString(CultureInfo.InvariantCulture)));
        }

        private string get_identifier_string()
        {
            // Identifier has form: [letters][number]
            var ident = string.Empty;
            while (valid_identifier_char(look_next_char()))
            {
                ident += get_next_char();
                if (!char.IsDigit(look_next_char())) continue;
                ident += get_number_string();
                return ident;
            }
            return ident;
        }

        private string get_number_string()
        {
            var number = string.Empty;
            number += get_positive_natural_number();

            if(look_next_char() == '.')
            {
                number += get_next_char();
                number += get_positive_natural_number();
            }
            return number;
        }

        private string get_positive_natural_number()
        {
            var num = string.Empty;
            while (char.IsNumber(look_next_char()))
            {
                num += get_next_char();
            }
            return num;
        }

        private string get_white_string()
        {
            var whiteStr = string.Empty;
            var c = look_next_char();
            while(char.IsWhiteSpace(c))
            {
                whiteStr += get_next_char();
                c = look_next_char();
            }
            return whiteStr;
        }

        private char get_next_char()
        {
            if (CurrentIndex == INVALID_INDEX)
                return (char) 0;
            return _source[_current_index++];
        }

        private char look_next_char()
        {
            if (CurrentIndex == INVALID_INDEX)
                return (char)0;
            return _source[CurrentIndex];
        }
    }
}