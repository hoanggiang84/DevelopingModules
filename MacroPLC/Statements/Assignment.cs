using System.Collections.Generic;
using HPMacroCommon;
using HPMathExpression;
using HPVariableRepository;
using MacroLexScn;
using MacroVariableDB;

namespace MacroPLC
{
    internal class Assignment
    {
        private TokenManager tokenManager;
        private VariableRepository varDB;
        public Assignment(IEnumerable<Token> tokens, VariableRepository varDB)
        {
            tokenManager = new TokenManager(tokens);
            GetInfo();
            this.varDB = varDB;
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