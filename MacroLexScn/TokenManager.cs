using System;
using System.Collections.Generic;
using HPMacroCommon;

namespace MacroLexScn
{
    public class TokenManager
    {
        public TokenManager(IEnumerable<Token> tokens)
        {
            this.tokens.AddRange(tokens);
            CurrentIndex = 0;
        }

        private List<Token> tokens = new List<Token>();
        private int CurrentIndex;

        public Token GetNextToken()
        {
            if (CurrentIndex < tokens.Count)
                return tokens[CurrentIndex++];
            return GetLastToken();
        }

        public Token LookNextToken()
        {
            if (CurrentIndex < tokens.Count)
            {
                return tokens[CurrentIndex];

            }
            return GetLastToken();
        }

        private Token GetLastToken()
        {
            if (tokens.Count <= 0)
                throw new Exception("Empty tokens");

            return new Token(string.Empty, TokenType.END); // Also to indicate end of source
        }

        public Token Match(string str)
        {
            var nextToken = GetNextToken();
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