using System;
using System.Collections.Generic;
using HPMacroCommon;
using HPMathExpression;
using HPVariableRepository;
using MacroLexScn;
using HPMacroFunctions;

namespace MacroPLC
{
    public class BuiltInFunctionStatement
    {
        private TokenManager tokenManager;
        private VariableRepository varDB;

        public BuiltInFunctionStatement(IEnumerable<Token> tokens, VariableRepository varDB)
        {
            tokenManager = new TokenManager(tokens);
            this.varDB = varDB;
            MathExpression.VarDB = varDB;
            GetInfo();
        }

        private Token FunctionToken;
        private List<List<Token>> argumentList = new List<List<Token>>();
        private void GetInfo()
        {
            FunctionToken = tokenManager.IgnoreWhiteGetNextToken();
            if (!( HPFUNC.IsMacroReturnValueFunction(FunctionToken.Text)
                || HPFUNC.IsVoidMacroFunction(FunctionToken.Text)))
                throw new Exception(string.Format("Unrecognized function '{0}'", FunctionToken.Text));

            var nextToken = tokenManager.IgnoreWhiteLookNextToken();
            if (nextToken.Text == MacroKeywords.PARANTHESE_OPEN)
                tokenManager.Match(MacroKeywords.PARANTHESE_OPEN);
            else
                throw new Exception(string.Format("Expected '{0}'", MacroKeywords.PARANTHESE_OPEN));

            nextToken = tokenManager.IgnoreWhiteLookNextToken();
            var currentExpressionToken = new List<Token>();
            while (nextToken.Text != MacroKeywords.END
                && nextToken.Text != MacroKeywords.PARANTHESE_CLOSE)
            {
                nextToken = tokenManager.IgnoreWhiteGetNextToken();
                if(nextToken.Text == MacroKeywords.COMMA)
                {
                    if (currentExpressionToken.Count <= 0)
                    {
                        throw new Exception(string.Format(
                            "Expected argument in function '{0}'", FunctionToken.Text));
                    }
                    argumentList.Add(currentExpressionToken);
                    currentExpressionToken.Clear();
                }
                else
                {
                    currentExpressionToken.Add(nextToken);
                }

                nextToken = tokenManager.IgnoreWhiteLookNextToken();

            }

            if (nextToken.Text == MacroKeywords.PARANTHESE_CLOSE)
            {
                tokenManager.Match(MacroKeywords.PARANTHESE_CLOSE);
                argumentList.Add(currentExpressionToken);
                var lastToken = tokenManager.IgnoreWhiteGetNextToken();
                if (lastToken.Type != TokenType.END)
                    throw new Exception(string.Format("Invalid function statement '{0}'", FunctionToken.Text));
            }
            else
            {
                throw new Exception(string.Format("Expected '{0}'", MacroKeywords.PARANTHESE_CLOSE));
            }
        }

        public void Step()
        {
            if(HPFUNC.IsVoidMacroFunction(FunctionToken.Text))
                HPFUNC.GetVoidFunction(FunctionToken.Text).Invoke(null);
        }

        public void Execute()
        {
            Step();
        }
    }
}