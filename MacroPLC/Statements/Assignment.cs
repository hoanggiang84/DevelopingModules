using System;
using System.Collections.Generic;
using HPMacroCommon;
using HPMathExpression;
using HPTypes;
using HPVariableRepository;
using MacroLexScn;

namespace MacroPLC
{
    public class Assignment
    {
        private TokenManager tokenManager;
        private VariableRepository varDB;
        public Assignment(IEnumerable<Token> tokens, VariableRepository varDB)
        {
            this.varDB = varDB;
            MathExpression.VarDB = varDB;
            tokenManager = new TokenManager(tokens);
            GetInfo();
        }

        private void GetInfo()
        {
            getVariableName();

            Match(MacroKeywords.EQUAL);

            getRightSideExpression();
        }

        private List<Token> expressionTokens = new List<Token>();
        private void getRightSideExpression()
        {
            while (tokenManager.LookNextToken().Type != TokenType.END)
                expressionTokens.Add(tokenManager.GetNextToken());
        }

        private void Match(string expectedString)
        {
            if (tokenManager.LookNextToken().Text == expectedString)
                tokenManager.Match(expectedString);
            else
                throw new Exception(string.Format("Expected '{0}'", expectedString));
        }

        private string variableName;
        private void getVariableName()
        {
            var varToken = tokenManager.GetNextToken();
            if(varToken.Type != TokenType.GLOBAL_VAR && varToken.Type != TokenType.LOCAL_VAR)
                throw new Exception("Expected variable at the start of statement");

            variableName = varToken.Text;
            if (variableName != "#" && variableName != "@") return; // No indexer

            // Assignment uses indexer #[i] or @[i]
            var nextToken = tokenManager.LookNextToken();
            if (nextToken.Text != MacroKeywords.INDEX_OPEN)
                throw new Exception(string.Format("Expected '{0}'", MacroKeywords.INDEX_OPEN));
            
            tokenManager.Match(MacroKeywords.INDEX_OPEN);
            nextToken = tokenManager.LookNextToken();
            indexTokens.Clear();
            while (nextToken.Text != MacroKeywords.INDEX_CLOSE)
            {
                if (nextToken.Type == TokenType.END)
                    throw new Exception(string.Format("Expected '{0}'", MacroKeywords.INDEX_CLOSE));
                indexTokens.Add(tokenManager.GetNextToken());
                nextToken = tokenManager.LookNextToken();
            }
            tokenManager.Match(MacroKeywords.INDEX_CLOSE);
        }

        private List<Token> indexTokens = new List<Token>();

        public void Execute()
        {
            Step();
        }

        public void Step()
        {
            var varName = variableName;
            // Get index value
            if (indexTokens.Count > 0)
            {
                var index = MathExpression.Create(indexTokens).Evaluate();
                if (index.Type != VariableType.INT)
                    throw new Exception("Index must be an integer");

                var indexVal = int.Parse(index.Literal);
                if (indexVal < 0)
                    throw new Exception("Index must be non-negative");
                varName += index.Literal;
            }
            
            var value = MathExpression.Create(expressionTokens).Evaluate();
            varDB.SetVariable(varName, value);
        }
    }
}