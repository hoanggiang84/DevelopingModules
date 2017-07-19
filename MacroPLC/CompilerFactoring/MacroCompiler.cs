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

        private static int _label_count;
        private static string create_new_label()
        {
            return string.Format("L{0}", _label_count++);
        }

        //private static string create_program_label(string label)
        //{
        //    return "P" + label;
        //}

        private SourceLine get_next_line(out int lineNum)
        {
            return sourceManager.GetNextLine(out lineNum);
        }

        private SourceLine look_next_line()
        {
            var lineNum = 0;
            return sourceManager.LookCurrentLine(out lineNum);
        }

        private static List<Token> extract_statement_tokens(IEnumerable<Token> tokens)
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

        private void create_single_line_statement(TaskType task_type, IEnumerable<Token> tokens, int line_num)
        {
            var funcTask = new Task(task_type, extract_statement_tokens(tokens));
            funcTask.SetLineNumber(line_num);
            compiledTasks.Add(funcTask);
        }

        private void check_if_next_line_null(Keyword expected)
        {
            if (look_next_line() == null)
                throw new Exception(string.Format("Expected '{0}'", expected));
        }

        #endregion
        
        #region Block
        private static bool not_end_block_line(SourceLine lineContent)
        {
            if (lineContent == null)
                return false;

            switch (lineContent.Type)
            {
                case Keyword.END:
                case Keyword.ELSEIF:
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

        private void create_endif(string new_label1)
        {
            verify_next_source_line(Keyword.END_IF);
            var line_num = 0;
            get_next_line(out line_num);
            var label_task = new Task(TaskType.LABEL, new_label1);
            label_task.SetLineNumber(line_num);
            compiledTasks.Add(label_task);
        }

        private void verify_next_source_line(Keyword line_type)
        {
            check_if_next_line_null(line_type);

            var token_mgr = new TokenManager(look_next_line().Tokens);
            token_mgr.IgnoreWhiteLookNextToken();
            token_mgr.Match(MacroKeywords.GetKeywordString(line_type));

            token_mgr.IgnoreWhiteLookNextToken();
            token_mgr.Match(MacroKeywords.END_STATEMENT);

            if (token_mgr.IgnoreWhiteLookNextToken().Type != TokenType.END)
                throw new Exception(string.Format("Unexpected '{0}'", token_mgr.IgnoreWhiteLookNextToken().Text));
        }

        private void create_else_block(string new_label1, ref string new_label2)
        {
            check_if_next_line_null(Keyword.END_IF);

            if (look_next_line().Type == Keyword.ELSE)
            {
                create_else(new_label1, out new_label2);
            }
            else if (look_next_line().Type == Keyword.END_IF)
            {
                return;
            }

            create_block();
        }

        private void create_else(string new_label1, out string new_label2)
        {
            new_label2 = create_new_label();
            var next_line_num = 0;
            get_next_line(out next_line_num);

            var branch_task = new Task(TaskType.BRANCH, new_label2);
            branch_task.SetLineNumber(next_line_num);
            compiledTasks.Add(branch_task);

            var label_task = new Task(TaskType.LABEL, new_label1);
            label_task.SetLineNumber(next_line_num);
            compiledTasks.Add(label_task);
        }

        private void create_if_condition(out string label1, out string label2)
        {
            var next_line_num = 0;
            var next_line = get_next_line(out next_line_num);
            
            // If
            var token_mgr = new TokenManager(next_line.Tokens);
            token_mgr.IgnoreWhiteLookNextToken();
            token_mgr.Match(MacroKeywords.IF);

            // Condition
            var condition_tokens = new List<Token>();
            while (token_mgr.IgnoreWhiteLookNextToken().Type != TokenType.END)
                condition_tokens.Add(token_mgr.IgnoreWhiteGetNextToken());

            var condtion_task = new Task(TaskType.BOOLEAN_EVALUATE, condition_tokens);
            condtion_task.SetLineNumber(next_line_num);
            compiledTasks.Add(condtion_task);

            // Branch label1 if condition is false
            label1 = create_new_label();
            label2 = label1;
            var label_task = new Task(TaskType.BRANCH_FALSE, label1);
            label_task.SetLineNumber(next_line_num);
            compiledTasks.Add(label_task);
        }

        #endregion

    }
}