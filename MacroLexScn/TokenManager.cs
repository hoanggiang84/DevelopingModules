using System;
using System.Collections.Generic;
using HPMacroCommon;

namespace MacroLexScn
{
    public class TokenManager
    {
        public static readonly Token END_TOKEN = new Token(string.Empty,TokenType.END);

        public TokenManager(IEnumerable<Token> tokens)
        {
            this.tokens.AddRange(tokens);
            CurrentIndex = 0;
        }

        private List<Token> tokens = new List<Token>();
        private int CurrentIndex;

        public Token GetNextToken()
        {
            IgnoreWhiteSpace();
            if (CurrentIndex < tokens.Count)
                return tokens[CurrentIndex++];
            return GetLastToken();
        }

        public Token LookNextToken()
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

            var curToken = tokens[CurrentIndex];
            while (CurrentIndex < tokens.Count && curToken.Type == TokenType.WHITE_SPACE)
            {
                Match(curToken.Text);
                if (CurrentIndex >= tokens.Count)
                    curToken = END_TOKEN;
                else
                    curToken = tokens[CurrentIndex];
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
            var nextToken = tokens[CurrentIndex++];
            if (nextToken.Text != str)
                throw new Exception(string.Format("Expected: '{0}'. But was: '{1}'", str, nextToken.Text));
            return nextToken;
        }

        public string GetIdentifier()
        {
            var nextToken = GetNextToken();
            Validate(nextToken, TokenType.IDENTIFIER);
            if (MacroKeywords.IsKeyword(nextToken.Text))
                throw new Exception(string.Format("Error: Invalid identifier name '{0}'", nextToken.Text));
            return nextToken.Text;
        }

        public string GetVariable()
        {
            var nextToken = GetNextToken();
            if(!(nextToken.Type == TokenType.GLOBAL_VAR
                || nextToken.Type == TokenType.LOCAL_VAR
                || MacroKeywords.IsKeyword(nextToken.Text)))
                throw new Exception(string.Format("Error: Invalid variable name '{0}'", nextToken.Text));
            return nextToken.Text;
        }

        public void Reset()
        {
            CurrentIndex = 0;
        }

        private void Validate(Token token, TokenType expectType)
        {
            if (token.Type != expectType)
                throw new Exception(string.Format("Error: Invalid String '{0}'. Expected: {1}", token.Text, expectType));
        }

        public Token LookNextNextToken()
        {
            if (CurrentIndex + 1 < tokens.Count)
                return tokens[CurrentIndex + 1];
            return GetLastToken();
        }
    }
}