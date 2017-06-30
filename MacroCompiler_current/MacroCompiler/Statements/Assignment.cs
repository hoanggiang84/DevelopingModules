using System.Collections.Generic;
using HPMacroComponents;
using HPMathExpression;
using MacroVariableDB;

namespace HPCompiler
{
    internal class Assignment : Statement, IStatement
    {
        public Assignment(IEnumerable<Token> tokens)
        {
            SourceTokenManager = new TokenManager(tokens);
            GetInfo();
        }

        private void GetInfo()
        {
            variableName = SourceTokenManager.GetVariable();
            SourceTokenManager.Match("=");
            var keyword = SourceTokenManager.LookNextToken().Text;
            while (!(keyword == MacroKeywords.END 
                || keyword == MacroKeywords.END_STATEMENT   // <assignment> ';'
                || keyword == MacroKeywords.TO))            // FOR <assignment> TO ...
            {
                expressionTokens.Add(SourceTokenManager.GetNextToken());
                keyword = SourceTokenManager.LookNextToken().Text;
            }
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