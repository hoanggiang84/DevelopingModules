using System.Linq;
using HPGCodeValidation;
using HPMacroCommon;
using HPMacroFunctions;
using HPMacroTask;
using HPMathExpression;
using HPVariableRepository;
using MacroLexScn;
using System.Collections.Generic;
using System;

namespace MacroPLC
{
    public class IfStatement:MacroStatement
    {
        private List<List<Token>> tokens;
        private List<Token> condition;
        private List<MacroStatement> trueStatements;
        private List<MacroStatement> falseStatements; 

        public IfStatement(List<List<Token>> tokens, VariableRepository varDB)
        {
            this.tokens = tokens;
            this.varDB = varDB;
            MathExpression.VarDB = varDB;
            GetInfo();
        }

        private void GetInfo()
        {
            Match(MacroKeywords.IF);
            condition = MatchParantheseExpression();
            var ifClause = tokens[1];

            if (HPFUNC.IsMacroFuntion(ifClause.First().Text))
            {
                
            }

            var endIf = tokens[2];
            tokenManager = new TokenManager(endIf);
            Match(MacroKeywords.ENDIF);
            Match(MacroKeywords.END_STATEMENT);
        }

      public override void Execute()
        {
            currentIndex = 0;
            while (currentIndex < tokens.Count)
            {
                Step();
            }
        }

        private int currentIndex;
        public override void Step()
        {
            if(StepNotify != null)
                StepNotify.Invoke(this, new StatementArg(string.Empty, currentIndex));
            currentIndex++;
        }
    }
}