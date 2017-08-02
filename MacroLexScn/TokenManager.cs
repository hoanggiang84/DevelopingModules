using System;
using System.Collections.Generic;
using HPMacroCommon;

namespace MacroLexScn
{
    public class TokenManager
    {
        public static readonly Token END_TOKEN = new Token(MacroKeywords.END, TokenType.END);

        public TokenManager(string expr)
        {
            var lex_scn = new MacroLexicalScanner(expr);
            var current_token = lex_scn.ScanNext();
            while (current_token.Type != TokenType.END)
            {
                tokens.Add(current_token);
                current_token = lex_scn.ScanNext();
            }
            _current_index = 0;
        }

        public TokenManager(IEnumerable<Token> tokens)
        {
            this.tokens.AddRange(tokens);
            _current_index = 0;
        }

        private List<Token> tokens = new List<Token>();
        private int _current_index;

        public Token IgnoreWhiteGetNextToken()
        {
            ignore_white_space();
            if (_current_index < tokens.Count)
                return tokens[_current_index++];
            return get_last_token();
        }

        public Token IgnoreWhiteLookNextToken()
        {
            ignore_white_space();
            if (_current_index < tokens.Count)
                return tokens[_current_index];
            return get_last_token();
        }

        private void ignore_white_space()
        {
            if(_current_index >= tokens.Count)
                return;

            var current_token = tokens[_current_index];
            while (_current_index < tokens.Count && current_token.Type == TokenType.WHITE_SPACE)
            {
                Match(current_token.Text);
                if (_current_index >= tokens.Count)
                    current_token = END_TOKEN;
                else
                    current_token = tokens[_current_index];
            }
        }

        private Token get_last_token()
        {
            if (tokens.Count <= 0)
                throw new Exception("Empty tokens");

            return END_TOKEN; // Also to indicate end of source
        }

        /// <summary>
        /// Match current token string with input. Increase current index if succeeded.
        /// </summary>
        public Token Match(string str)
        {
            Token next_token;
            try
            {
                next_token = tokens[_current_index++];
            }
            catch
            {
                throw new Exception(string.Format("Expected: '{0}'", str));
            }

            if (next_token.Text != str)
                throw new Exception(string.Format("Expected: '{0}'. But was: '{1}'", str, next_token.Text));
            return next_token;
        }

        public string GetIdentifier()
        {
            var next_token = IgnoreWhiteGetNextToken();
            validate(next_token, TokenType.IDENTIFIER);
            if (MacroKeywords.IsKeyword(next_token.Text))
                throw new Exception(string.Format("Error: Invalid identifier name '{0}'", next_token.Text));
            return next_token.Text;
        }

        public string GetVariable()
        {
            var next_token = IgnoreWhiteGetNextToken();
            if(!(next_token.Type == TokenType.GLOBAL_VAR
                || next_token.Type == TokenType.LOCAL_VAR
                || MacroKeywords.IsKeyword(next_token.Text)))
                throw new Exception(string.Format("Invalid variable name '{0}'", next_token.Text));
            return next_token.Text;
        }

        public void Reset()
        {
            _current_index = 0;
        }

        private void validate(Token token, TokenType expect_type)
        {
            if (token.Type != expect_type)
                throw new Exception(string.Format(
                    "Invalid String '{0}'. Expected: {1}", 
                    token.Text, expect_type));
        }

        public Token LookNextNextToken()
        {
            if (_current_index + 1 < tokens.Count)
                return tokens[_current_index + 1];
            return get_last_token();
        }
    }
}