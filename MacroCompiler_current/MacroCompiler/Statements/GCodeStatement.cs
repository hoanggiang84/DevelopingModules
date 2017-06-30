using System;
using System.Collections.Generic;
using HPMacroComponents;
using HPMathExpression;
using HPTypes;
using LoadIdentifierInterface;
using MacroVariableDB;

namespace HPCompiler
{
    internal class GCodeStatement:Statement,IStatement
    {
        public GCodeStatement(IEnumerable<Token> tokens)
        {
            SourceTokenManager = new TokenManager(tokens);
            GetInfo();
        }

        private readonly Dictionary<char, IEvaluate<HPType>> ParamsEvals =
            new Dictionary<char, IEvaluate<HPType>>();

        private string CommandCode;
   
        private void GetInfo()
        {
            CommandCode = SourceTokenManager.GetNextToken().Text;
            var nextWord = SourceTokenManager.LookNextToken().Text;
            while (!(nextWord == MacroKeywords.END_STATEMENT
                || nextWord == MacroKeywords.END))
            {

                if (ParamsEvals.ContainsKey(nextWord[0]))
                    throw new Exception(string.Format("Duplicate Char '{0}'", nextWord[0]));

                SourceTokenManager.GetNextToken();
                if (nextWord.Length > 1)
                {
                    ParamsEvals.Add(nextWord[0], MathExpression.Create(nextWord.Substring(1)));
                }
                else
                {
                    IEvaluate<HPType> paramEval = null;
                    if (SourceTokenManager.LookNextToken().Text == "(")
                        paramEval = MatchParantheseExpression();
                    else
                    {
                        paramEval = MathExpression.Create(SourceTokenManager.LookNextToken().Text);
                        SourceTokenManager.GetNextToken();
                    }
                    ParamsEvals.Add(nextWord[0], paramEval);
                }
                nextWord = SourceTokenManager.LookNextToken().Text;
            }
        }

        public void Execute()
        {
            var gCodeStatement = CommandCode;
            foreach (var paramsEval in ParamsEvals)
            {
                var paramChar = paramsEval.Key;
                var paramValue = paramsEval.Value.Evaluate();
                gCodeStatement += string.Format(" {0}{1}", paramChar, paramValue.Literal);
            }
            VariableDB.OnGCodeGenerated(gCodeStatement);
        }

        public void Step()
        {
            Execute();
        }
    }
}