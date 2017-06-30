using System;
using System.Collections.Generic;
using HPMacroComponents;
using HPMathExpression;
using HPTypes;
using LoadIdentifierInterface;

namespace HPCompiler
{
    internal abstract class Statement
    {
        protected TokenManager SourceTokenManager;

        protected IEvaluate<HPType> MatchParantheseExpression()
        {
            SourceTokenManager.Match("(");

            var nestedParanCount = 1;
            var expTokens = new List<Token>();
            var keyword = SourceTokenManager.LookNextToken().Text;
            while (!((keyword == ")" && nestedParanCount == 1)
                || keyword == MacroKeywords.END))
            {
                switch (keyword)
                {
                    case "(":
                        nestedParanCount++;
                        break;
                    case ")":
                        nestedParanCount--;
                        break;
                }
                expTokens.Add(SourceTokenManager.GetNextToken());
                keyword = SourceTokenManager.LookNextToken().Text;
            }

            var ParanExpr = MathExpression.Create(expTokens);
            SourceTokenManager.Match(")");
            nestedParanCount--;
            if (nestedParanCount != 0)
                throw new Exception("Unbalanced Paranthesis");
            return ParanExpr;
        }
    }
}