using System;
using System.Collections.Generic;
using System.Linq;
using HPMacroCommon;
using HPMathExpression;
using HPTypes;
using HPVariableRepository;
using MacroLexScn;
using HPMacroFunctions;

namespace MacroPLC
{
    public class BuiltInFunctionStatement:MacroStatement
    {
        private Token FunctionToken;
        private List<List<Token>> argumentList = new List<List<Token>>();

        public BuiltInFunctionStatement(IEnumerable<Token> tokens, VariableRepository varDB)
        {
            tokenManager = new TokenManager(tokens);
            this.varDB = varDB;
            MathExpression.VarDB = varDB;
            GetInfo();
        }

        private void GetInfo()
        {
            getFunctionToken();

            Match(MacroKeywords.PARANTHESE_OPEN);

            getArgumentExpressions();

            Match(MacroKeywords.PARANTHESE_CLOSE);

            validateENDToken();
        }

        private void getArgumentExpressions()
        {
            var nextToken = tokenManager.IgnoreWhiteLookNextToken();
            var currentExpressionToken = new List<Token>();
            while (nextToken.Text != MacroKeywords.END
                   && nextToken.Text != MacroKeywords.PARANTHESE_CLOSE)
            {
                nextToken = tokenManager.IgnoreWhiteGetNextToken();
                if (nextToken.Text == MacroKeywords.COMMA)
                {
                    if (currentExpressionToken.Count <= 0)
                        throw new Exception(string.Format(
                            "Invalid symbol '{0}' in function '{1}'", 
                            MacroKeywords.COMMA, FunctionToken.Text));
                    
                    argumentList.Add(currentExpressionToken);
                    currentExpressionToken.Clear();
                }
                else
                {
                    currentExpressionToken.Add(nextToken);
                }

                nextToken = tokenManager.IgnoreWhiteLookNextToken();
            }
            argumentList.Add(currentExpressionToken);
        }

        private void getFunctionToken()
        {
            FunctionToken = tokenManager.IgnoreWhiteGetNextToken();
            if (!(HPFUNC.IsMacroFuntion(FunctionToken.Text)))
                throw new Exception(string.Format("Unrecognized function '{0}'", FunctionToken.Text));
        }

        public override void Step()
        {
            var args = (from expr in argumentList 
                        where expr.Count > 0 
                        select MathExpression.Create(expr).Evaluate()).ToList();

            if(HPFUNC.IsVoidMacroFunction(FunctionToken.Text))
                HPFUNC.GetVoidFunction(FunctionToken.Text).Invoke(args);
            else if(HPFUNC.IsReturnValueFunction(FunctionToken.Text))
                HPFUNC.GetReturnValueFunction(FunctionToken.Text).Invoke(args);
        }

        public override void Execute()
        {
            Step();
        }
    }
}