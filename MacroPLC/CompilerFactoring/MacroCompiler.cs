using System;
using System.Collections.Generic;
using HPMacroCommon;
using HPMacroTask;
using MacroLexScn;

namespace MacroPLC
{
    public partial class MacroCompiler
    {
        #region Private refactoring functions

        private static int LCount;
        private static string CreateNewLabel()
        {
            return string.Format("L{0}", LCount++);
        }

        private static string CreateProgramLabel(string label)
        {
            return "P" + label;
        }


        private SourceLine GetNextLine(out int lineNum)
        {
            return sourceManager.GetNextLine(out lineNum);
        }

        private static List<Token> ExtractStatementTokens(IEnumerable<Token> tokens)
        {
            // Skip last end statement token ';'
            var tokenList = new List<Token>();
            var token_mgr = new TokenManager(tokens);
            var next_token = token_mgr.IgnoreWhiteLookNextToken();
            while (next_token.Type != TokenType.END
                && next_token.Text != MacroKeywords.END_STATEMENT)
            {
                tokenList.Add(token_mgr.IgnoreWhiteGetNextToken());
                next_token = token_mgr.IgnoreWhiteLookNextToken();
            }

            token_mgr.Match(MacroKeywords.END_STATEMENT);

            if (token_mgr.IgnoreWhiteLookNextToken().Type != TokenType.END)
                throw new Exception(string.Format("Unexpected string '{0}'",
                    token_mgr.IgnoreWhiteGetNextToken().Text));

            return tokenList;
        }

        private void CreateOneLineStatement(TaskType task_type, IEnumerable<Token> tokens, int line_num)
        {
            var funcTask = new Task(task_type, ExtractStatementTokens(tokens));
            funcTask.SetLineNumber(line_num);
            compiledTasks.Add(funcTask);
        }

        private static bool isVariableToken(Token tok)
        {
            return tok.Type == TokenType.GLOBAL_VAR
                || tok.Type == TokenType.LOCAL_VAR;
        }
        #endregion
    }
}