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
            CurrentIndex = 0;
        }

        public TokenManager(IEnumerable<Token> tokens)
        {
            this.tokens.AddRange(tokens);
            CurrentIndex = 0;
        }

        private List<Token> tokens = new List<Token>();
        private int CurrentIndex;

        public Token IgnoreWhiteGetNextToken()
        {
            IgnoreWhiteSpace();
            if (CurrentIndex < tokens.Count)
                return tokens[CurrentIndex++];
            return GetLastToken();
        }

        public Token IgnoreWhiteLookNextToken()
        {
            IgnoreWhiteSpace();
            if (CurrentIndex < tokens.Count)
                return tokens[CurrentIndex];
            return GetLastToken();
        }

        private void IgnoreWhiteSpace()
        {
            if(CurrentIndex >= tokens.Count)
                return;

            var current_token = tokens[CurrentIndex];
            while (CurrentIndex < tokens.Count && current_token.Type == TokenType.WHITE_SPACE)
            {
                Match(current_token.Text);
                if (CurrentIndex >= tokens.Count)
                    current_token = END_TOKEN;
                else
                    current_token = tokens[CurrentIndex];
            }
        }

        private Token GetLastToken()
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
            var next_token = tokens[CurrentIndex++];
            if (next_token.Text != str)
                throw new Exception(string.Format("Expected: '{0}'. But was: '{1}'", str, next_token.Text));
            return next_token;
        }

        public string GetIdentifier()
        {
            var next_token = IgnoreWhiteGetNextToken();
            Validate(next_token, TokenType.IDENTIFIER);
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
                throw new Exception(string.Format("Error: Invalid variable name '{0}'", next_token.Text));
            return next_token.Text;
        }

        public void Reset()
        {
            CurrentIndex = 0;
        }

        private void Validate(Token token, TokenType expect_type)
        {
            if (token.Type != expect_type)
                throw new Exception(string.Format(
                    "Error: Invalid String '{0}'. Expected: {1}", 
                    token.Text, expect_type));
        }

        public Token LookNextNextToken()
        {
            if (CurrentIndex + 1 < tokens.Count)
                return tokens[CurrentIndex + 1];
            return GetLastToken();
        }
    }
}