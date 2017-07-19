using System;
using System.Collections.Generic;
using HPMacroCommon;
using HPMacroTask;
using MacroLexScn;

namespace MacroPLC
{
    public partial class MacroCompiler
    {
        #region Private functions

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

        private SourceLine LookNextLine(out int lineNum)
        {
            return sourceManager.LookCurrentLine(out lineNum);
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
        
        #region Block
        private bool NotEndBlockLine(SourceLine lineContent)
        {
            if (lineContent == null)
                return false;

            switch (lineContent.Type)
            {
                case Keyword.END:
                case Keyword.ELSE:
                case Keyword.END_IF:
                case Keyword.END_WHILE:
                case Keyword.UNTIL:
                case Keyword.END_FOR:
                case Keyword.CASE:
                case Keyword.DEFAULT:
                case Keyword.END_SWITCH:
                    return false;
            }

            return true;
        }
        #endregion

        #region If


        private void CreateEndIf(string new_label1)
        {
            var next_line_num = 0;
            var next_line = LookNextLine(out next_line_num);
            if (next_line == null)
                throw new Exception(string.Format("Expected '{0}'", MacroKeywords.ENDIF));

            var token_mgr = new TokenManager(next_line.Tokens);
            token_mgr.IgnoreWhiteLookNextToken();
            token_mgr.Match(MacroKeywords.ENDIF);
            token_mgr.IgnoreWhiteLookNextToken();
            token_mgr.Match(MacroKeywords.END_STATEMENT);
            if (token_mgr.IgnoreWhiteLookNextToken().Type != TokenType.END)
                throw new Exception(string.Format("Unexpected '{0}'", token_mgr.IgnoreWhiteLookNextToken().Text));
            compiledTasks.Add(new Task(TaskType.LABEL, new_label1));
        }

        private void CreateElse(string new_label1, string new_label2)
        {
            int next_line_num;
            var next_line = LookNextLine(out next_line_num);
            if (next_line == null)
                throw new Exception(string.Format("Expected '{0}'", MacroKeywords.ENDIF));

            if (next_line.Type == Keyword.ELSE)
            {
                compiledTasks.Add(new Task(TaskType.BRANCH, new_label2));
                compiledTasks.Add(new Task(TaskType.LABEL, new_label1));
                next_line = GetNextLine(out next_line_num);
            }

            next_line = LookNextLine(out next_line_num);
            if (next_line.Type == Keyword.END_IF)
            {
                var token_mgr = new TokenManager(next_line.Tokens);
                token_mgr.IgnoreWhiteLookNextToken();
                token_mgr.Match(MacroKeywords.ENDIF);
                token_mgr.IgnoreWhiteLookNextToken();
                token_mgr.Match(MacroKeywords.END_STATEMENT);
                if (token_mgr.IgnoreWhiteLookNextToken().Type != TokenType.END)
                    throw new Exception(string.Format("Unexpected '{0}'", token_mgr.IgnoreWhiteLookNextToken().Text));
                compiledTasks.Add(new Task(TaskType.LABEL, new_label1));
                return;
            }

            CreateBlock();
        }

        private void CreateIfCondition(out string label1, out string label2)
        {
            var next_line_num = 0;
            var next_line = GetNextLine(out next_line_num);
            var token_mgr = new TokenManager(next_line.Tokens);
            token_mgr.IgnoreWhiteLookNextToken();

            token_mgr.Match(MacroKeywords.IF);

            var condition_tokens = new List<Token>();
            while (token_mgr.IgnoreWhiteLookNextToken().Type != TokenType.END)
                condition_tokens.Add(token_mgr.IgnoreWhiteGetNextToken());
            compiledTasks.Add(new Task(TaskType.BOOLEAN_EVALUATE, condition_tokens));

            label1 = CreateNewLabel();
            label2 = CreateNewLabel();
            compiledTasks.Add(new Task(TaskType.BRANCH_FALSE, label1));
        }
        #endregion

    }
}