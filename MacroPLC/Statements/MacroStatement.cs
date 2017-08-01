using System;
using HPMacroCommon;
using HPMacroTask;
using HPVariableRepository;
using MacroLexScn;
using System.Collections.Generic;

namespace MacroPLC
{
    public abstract class MacroStatement
    {
        public EventHandler<StatementArg> StepNotify ;

        protected static MacroStatement NULL_STATEMENT = new NullStatement();

        protected TokenManager tokenManager;
        protected VariableRepository varDB; 

        protected List<Token> GetRemainTokens()
        {
            var tokens = new List<Token>();
            while (tokenManager.IgnoreWhiteLookNextToken().Type != TokenType.END)
                tokens.Add(tokenManager.IgnoreWhiteGetNextToken());
            return tokens;
        }

        protected void Match(string expectedString)
        {
            if (tokenManager.IgnoreWhiteLookNextToken().Text == expectedString)
                tokenManager.Match(expectedString);
            else
                throw new Exception(string.Format("Expected '{0}'", expectedString));
        }

        protected static bool IsVariableToken(Token varToken)
        {
            return varToken.Type == TokenType.GLOBAL_VAR 
                || varToken.Type == TokenType.LOCAL_VAR;
        }

        protected List<Token> MatchParantheseExpression()
        {
            int nestedParanCount;
            Match(MacroKeywords.PARANTHESE_OPEN);

            var ParanExpr = getParanthesesExpression(out nestedParanCount);

            Match(MacroKeywords.PARANTHESE_CLOSE);
            nestedParanCount--;

            if (nestedParanCount != 0)
                throw new Exception("Unbalanced parantheses");

            return ParanExpr;
        }

        protected void validateENDToken()
        {
            var lastToken = tokenManager.IgnoreWhiteGetNextToken();
            if (lastToken.Type != TokenType.END)
                throw new Exception(string.Format("Invalid statement"));
        }

        public virtual void Execute()
        {
        }

        public virtual void Step()
        {
        }

        public static MacroStatement CreateStatement(
            TaskType task_type, 
            IEnumerable<Token> tokens, 
            VariableRepository variables)
        {
            switch (task_type)
            {
                case TaskType.ASSIGNMENT:
                    return new Assignment(tokens, variables);
                case TaskType.GCODE:
                    return new GCodeStatement(tokens, variables);
                case TaskType.BUILT_IN_FUNCTION:
                    return new BuiltInFunctionStatement(tokens, variables);
            }

            return NULL_STATEMENT;
        }

        #region Private Functions
        private List<Token> getParanthesesExpression(out int nestedParanCount)
        {
            nestedParanCount = 1;
            var paranExpressionTokens = new List<Token>();
            var keyword = tokenManager.IgnoreWhiteLookNextToken().Text;
            while (!(ParanthesesBalanced(keyword, nestedParanCount)
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
                paranExpressionTokens.Add(tokenManager.IgnoreWhiteGetNextToken());
                keyword = tokenManager.IgnoreWhiteLookNextToken().Text;
            }
            return paranExpressionTokens;
        }

        private bool ParanthesesBalanced(string keyword, int nestedParan)
        {
            return keyword == MacroKeywords.PARANTHESE_CLOSE
                   && nestedParan == 1;
        }

        #endregion
    }

    public class StatementArg : EventArgs
    {
        public readonly int LineNumber;
        public readonly string Statement;
        public StatementArg(string statement, int line_num)
        {
            Statement = statement;
            LineNumber = line_num;
        }
    }

    internal class NullStatement : MacroStatement
    {
    }
}