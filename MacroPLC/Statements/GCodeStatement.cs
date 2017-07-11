using System;
using System.Collections.Generic;
using HPGCodeValidation;
using HPMacroCommon;
using HPMathExpression;
using HPTypes;
using HPVariableRepository;
using LoadIdentifierInterface;
using MacroLexScn;

namespace MacroPLC
{
    public class GCodeStatement : MacroStatement
    {
        private readonly Dictionary<char, IEvaluate<HPType>> ParamsEvals =
            new Dictionary<char, IEvaluate<HPType>>();

        private string CommandCode;

        public GCodeStatement(IEnumerable<Token> tokens, VariableRepository varDB)
        {
            this.varDB = varDB;
            MathExpression.VarDB = varDB;
            tokenManager = new TokenManager(tokens);
            GetInfo();
        }
   
        private void GetInfo()
        {
            CommandCode = tokenManager.IgnoreWhiteGetNextToken().Text;
            var nextToken = tokenManager.IgnoreWhiteLookNextToken();
            while (nextToken.Type != TokenType.END)
            {
                var nextWord = nextToken.Text;
                checkDuplicateParameters(nextWord);
                getParametersAndExpressions();
                nextToken = tokenManager.IgnoreWhiteLookNextToken();
            }
        }

        private void getParametersAndExpressions()
        {
            var nextToken = tokenManager.IgnoreWhiteGetNextToken();
            var nextWord = nextToken.Text;
            if (nextWord.Length > 1)
            {
                ParamsEvals.Add(nextWord[0], MathExpression.Create(nextWord.Substring(1)));
            }
            else if (tokenManager.IgnoreWhiteLookNextToken().Text == MacroKeywords.PARANTHESE_OPEN)
            {
                var paramEval = MatchParantheseExpression();
                ParamsEvals.Add(nextWord[0], paramEval);
            }
        }

        private void checkDuplicateParameters(string nextWord)
        {
            if (ParamsEvals.ContainsKey(nextWord[0]))
                throw new Exception(string.Format("Duplicate parameters '{0}'", nextWord[0]));
        }

        public override void Execute()
        {
            Step();
        }

        public override void Step()
        {
            var gCodeStatement = CommandCode;
            foreach (var paramsEval in ParamsEvals)
            {
                var paramChar = paramsEval.Key;
                var paramValue = paramsEval.Value.Evaluate();
                gCodeStatement += string.Format(" {0}{1}", paramChar, paramValue.Literal);
            }
            ValidateGCode(gCodeStatement);
            varDB.OnGCodeGenerated(gCodeStatement);
        }

        private static void ValidateGCode(string gCodeStatement)
        {
            new GCodeValidate(gCodeStatement).Validate();
        }

    
    }
}