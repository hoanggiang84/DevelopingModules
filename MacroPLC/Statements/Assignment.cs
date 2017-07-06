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
            variableName = tokenManager.GetNextToken().Text;
            var nextToken = tokenManager.LookNextToken();
            if(variableName == "#" || variableName == "@")
            {
                if(nextToken.Text == MacroKeywords.INDEX_OPEN)
                {
                    tokenManager.Match(MacroKeywords.INDEX_OPEN);
                    nextToken = tokenManager.LookNextToken();
                    indexTokens.Clear();
                    while (nextToken.Text != MacroKeywords.INDEX_CLOSE)
                    {
                        if(nextToken.Type == TokenType.END)
                            throw new Exception(string.Format("Expected '{0}'",MacroKeywords.INDEX_CLOSE));
                        indexTokens.Add(tokenManager.GetNextToken());
                        nextToken = tokenManager.LookNextToken();
                    }
                    tokenManager.Match(MacroKeywords.INDEX_CLOSE);

                    var index = MathExpression.Create(indexTokens).Evaluate();
                    if(index.Type != VariableType.INT)
                        throw new Exception("Index must be an integer");

                    var indexVal = int.Parse(index.Literal);
                    if(indexVal < 0)
                        throw new Exception("Index must be non-negative");

                    variableName += index.Literal;
                }
                else
                {
                    throw new Exception(string.Format("Expected '{0}'", MacroKeywords.INDEX_OPEN));
                }
            }

            if (tokenManager.LookNextToken().Text == MacroKeywords.EQUAL)
                tokenManager.Match(MacroKeywords.EQUAL);

            while (tokenManager.LookNextToken().Type != TokenType.END)
                expressionTokens.Add(tokenManager.GetNextToken());
        }

        private string variableName;
        private List<Token> expressionTokens = new List<Token>();
        private List<Token> indexTokens = new List<Token>();

        public void Execute()
        {
            Step();
        }

        public void Step()
        {
            var value = MathExpression.Create(expressionTokens).Evaluate();
            varDB.SetVariable(variableName, value);
        }

        public string VariableName { get { return variableName; } }
    }
}