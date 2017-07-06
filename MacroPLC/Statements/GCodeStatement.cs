using System;
using System.Collections.Generic;
using HPMacroCommon;
using HPMathExpression;
using HPTypes;
using HPVariableRepository;
using LoadIdentifierInterface;
using MacroLexScn;
using MacroVariableDB;

namespace MacroPLC
{
    public class GCodeStatement
    {
        private TokenManager tokenManager;

        public GCodeStatement(IEnumerable<Token> tokens, VariableRepository varDB)
        {
            MathExpression.VarDB = varDB;
            tokenManager = new TokenManager(tokens);
            GetInfo();
        }

        private readonly Dictionary<char, IEvaluate<HPType>> ParamsEvals =
            new Dictionary<char, IEvaluate<HPType>>();

        private string CommandCode;
   
        private void GetInfo()
        {
            CommandCode = tokenManager.GetNextToken().Text;
            var nextToken = tokenManager.LookNextToken();
            while (nextToken.Type != TokenType.END)
            {
                var nextWord = nextToken.Text;
                if (ParamsEvals.ContainsKey(nextWord[0]))
                    throw new Exception(string.Format("Duplicate Char '{0}'", nextWord[0]));

                tokenManager.GetNextToken();
                if (nextWord.Length > 1)
                {
                    ParamsEvals.Add(nextWord[0], MathExpression.Create(nextWord.Substring(1)));
                }
                else
                {
                    IEvaluate<HPType> paramEval = null;
                    if (tokenManager.LookNextToken().Text == MacroKeywords.PARANTHESE_OPEN)
                        paramEval = MatchParantheseExpression();
                    else
                    {
                        paramEval = MathExpression.Create(tokenManager.LookNextToken().Text);
                        tokenManager.GetNextToken();
                    }
                    ParamsEvals.Add(nextWord[0], paramEval);
                }
                nextToken = tokenManager.LookNextToken();
            }
        }

        public void Execute()
        {
            Step();
        }

        public void Step()
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

        protected IEvaluate<HPType> MatchParantheseExpression()
        {
            tokenManager.Match(MacroKeywords.PARANTHESE_OPEN);

            var nestedParanCount = 1;
            var expTokens = new List<Token>();
            var keyword = tokenManager.LookNextToken().Text;
            while (!((keyword == MacroKeywords.PARANTHESE_CLOSE && nestedParanCount == 1)
                || keyword == MacroKeywords.END))
            {
                switch (keyword)
                {
                    case MacroKeywords.PARANTHESE_OPEN:
                        nestedParanCount++;
                        break;
                    case MacroKeywords.PARANTHESE_CLOSE:
                        nestedParanCount--;
                        break;
                }
                expTokens.Add(tokenManager.GetNextToken());
                keyword = tokenManager.LookNextToken().Text;
            }

            var ParanExpr = MathExpression.Create(expTokens);
            tokenManager.Match(MacroKeywords.PARANTHESE_CLOSE);
            nestedParanCount--;
            if (nestedParanCount != 0)
                throw new Exception("Unbalanced Paranthesis");
            return ParanExpr;
        }
    }
}