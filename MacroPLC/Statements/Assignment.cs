using System;
using System.Collections.Generic;
using System.Globalization;
using HPMacroCommon;
using HPMathExpression;
using HPTypes;
using HPVariableRepository;
using MacroLexScn;

namespace MacroPLC
{
    public class Assignment:MacroStatement
    {
        private List<Token> indexTokens = new List<Token>();
        private List<Token> expressionTokens = new List<Token>();

        public Assignment(IEnumerable<Token> tokens, VariableRepository variables)
        {
            varDB = variables;
            tokenManager = new TokenManager(tokens);
            GetInfo();
        }

        private void GetInfo()
        {
            getVariableName();

            Match(MacroKeywords.EQUAL);

            getRightSideExpression();
        }

        private void getRightSideExpression()
        {
            expressionTokens.AddRange(GetRemainTokens());
        }

        private string variableName;
        private void getVariableName()
        {
            var varToken = tokenManager.IgnoreWhiteGetNextToken();
            if(!IsVariableToken(varToken))
                throw new Exception("Expected variable at the start of assignment statement");

            variableName = varToken.Text;
            if (!isVariableChar(variableName)) 
                return; // No indexer

            // Assignment uses indexer #[i] or @[i]
            // Index open
            var nextToken = tokenManager.IgnoreWhiteLookNextToken();
            Match(MacroKeywords.INDEX_OPEN);
            
            // Index math expression
            nextToken = tokenManager.IgnoreWhiteLookNextToken();
            indexTokens.Clear();
            while (nextToken.Text != MacroKeywords.INDEX_CLOSE)
            {
                if (nextToken.Type == TokenType.END)
                    throw new Exception(string.Format("Expected '{0}'", MacroKeywords.INDEX_CLOSE));

                indexTokens.Add(tokenManager.IgnoreWhiteGetNextToken());
                nextToken = tokenManager.IgnoreWhiteLookNextToken();
            }

            // Index close
            Match(MacroKeywords.INDEX_CLOSE);
        }

        private static bool isVariableChar(string var_name)
        {
            return var_name == MacroKeywords.GLOBAL_VARIABLE_CHAR.ToString(CultureInfo.InvariantCulture)
                   || var_name == MacroKeywords.LOCAL_VARIABLE_CHAR.ToString(CultureInfo.InvariantCulture);
        }

        public override void Execute()
        {
            Step();
        }

        public override void Step()
        {
            var varName = variableName;

            // Get index value
            if (indexTokens.Count > 0)
            {
                var index = MathExpression.Create(indexTokens, varDB).Evaluate();
                validateIndexValue(index);
                varName += index.Literal;
            }

            var value = MathExpression.Create(expressionTokens, varDB).Evaluate();
            varDB.SetVariable(varName, value);
        }

        private static void validateIndexValue(HPType index)
        {
            if (index.Type != VariableType.INT)
                throw new Exception(string.Format("Index must be an integer '{0}'", index.Literal));

            var indexVal = int.Parse(index.Literal);
            if (indexVal < 0)
                throw new Exception(string.Format("Index must be non-negative'{0}'", index.Literal));
        }
    }
}