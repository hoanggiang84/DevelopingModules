using System.Collections.Generic;
using HPMacroCommon;
using HPMathExpression;
using MacroLexScn;
using MacroVariableDB;

namespace MacroPLC
{
    internal class Assignment
    {
        private TokenManager tokenManager;
        public Assignment(IEnumerable<Token> tokens)
        {
            tokenManager = new TokenManager(tokens);
            GetInfo();
        }

        private void GetInfo()
        {
            variableName = tokenManager.GetNextToken().Text;
            if (tokenManager.LookNextToken().Text == MacroKeywords.EQUAL)
                tokenManager.Match(MacroKeywords.EQUAL);

            while (tokenManager.LookNextToken().Type != TokenType.END)
                expressionTokens.Add(tokenManager.GetNextToken());
        }

        private string variableName;
        private List<Token> expressionTokens = new List<Token>();

        public void Execute()
        {
            var value = MathExpression.Create(expressionTokens).Evaluate();
            VariableDB.SetVariable(variableName, value);
        }

        public void Step()
        {
            Execute();
        }

        public string VariableName { get { return variableName; } }
    }
}