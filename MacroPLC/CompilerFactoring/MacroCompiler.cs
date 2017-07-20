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

            match_next_token(MacroKeywords.END_STATEMENT, token_mgr);
            
            verify_next_end_token(token_mgr);

            return tokenList;
        }

        private void verify_next_line_not_null(Keyword expected)
        {
            if (look_next_line() == null)
                throw new Exception(string.Format("Expected '{0}' statement", expected));
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

        // IF CLAUSE
        private void create_if_clause(string end_label, out string if_false_label)
        {
            var if_line_num = 0;
            var if_line = get_next_line(out if_line_num);

            create_if_condition(if_line, if_line_num, out if_false_label);

            create_branch_false(if_line_num, if_false_label);

            create_block();
        }

        private void create_if_condition(SourceLine if_line, int if_line_num, out string if_false_label)
        {
            create_condition(MacroKeywords.IF, if_line, if_line_num);
            if_false_label = create_new_label();
        }

        private void create_branch_false(string label)
        {
            compiledTasks.Add(Task.BranchIfFalse(label));
        }

        private void create_branch_false(int line_num, string false_label)
        {
            compiledTasks.Add(Task.BranchIfFalse(false_label, line_num));
        }

        // ELSE CLAUSE
        private void create_else_clause(string end_label, string if_false_label)
        {
            verify_next_line_not_null(Keyword.END_IF);

            if (look_next_line().Type != Keyword.ELSE) return;

            create_else(end_label, if_false_label);
            create_block();
        }

        private void create_else(string end_label, string if_false_label)
        {
            var next_line_num = 0;
            get_next_line(out next_line_num);
            create_branch_label(end_label);
            create_post_label(if_false_label, next_line_num);
        }

        private void create_branch_label(string label)
        {
            compiledTasks.Add(Task.Branch(label));
        }

        private void create_post_label(string label, int line_num)
        {
            compiledTasks.Add(Task.PostLabel(label, line_num));
        }
        
        // END IF
        private void create_endif(string label)
        {
            verify_next_source_line(Keyword.END_IF);
            var line_num = 0;
            get_next_line(out line_num);
            compiledTasks.Add(Task.PostLabel(label, line_num));
        }

        private static IEnumerable<Token> get_remain_tokens( TokenManager token_mgr)
        {
            var condition_tokens = new List<Token>();
            while (token_mgr.IgnoreWhiteLookNextToken().Type != TokenType.END)
                condition_tokens.Add(token_mgr.IgnoreWhiteGetNextToken());
            return condition_tokens;
        }

        private static void match_next_token(string expect_str, TokenManager token_mgr)
        {
            token_mgr.IgnoreWhiteLookNextToken();
            token_mgr.Match(expect_str);
        }

        private static void verify_next_end_token(TokenManager token_mgr)
        {
            if (token_mgr.IgnoreWhiteLookNextToken().Type != TokenType.END)
                throw new Exception(string.Format("Unexpected string '{0}'",
                    token_mgr.IgnoreWhiteLookNextToken().Text));
        }

        private void verify_next_source_line(Keyword line_type)
        {
            verify_next_line_not_null(line_type);

            var token_mgr = new TokenManager(look_next_line().Tokens);
            match_next_token(MacroKeywords.GetKeywordString(line_type), token_mgr);
            match_next_token(MacroKeywords.END_STATEMENT, token_mgr);

            verify_next_end_token(token_mgr);
        }

        private void create_elseif_clause(string end_label, ref string if_false_label)
        {
            while (look_next_line().Type == Keyword.ELSEIF)
            {
                create_branch_label(end_label);

                var line_num = 0;
                var elseif_line = get_next_line(out line_num);
                create_post_label(if_false_label, line_num);

                create_condition(MacroKeywords.ELSEIF, elseif_line, line_num);
                
                if_false_label = create_new_label();
                create_branch_false(line_num, if_false_label);

                create_block();
            }
        }

        #endregion

        #region While

        private void create_while_condition(string while_label)
        {
            var line_num = 0;
            var while_line = get_next_line(out line_num);

            create_post_label(while_label, line_num);

            create_condition(MacroKeywords.WHILE, while_line, line_num);
        }

        private void create_condition(string keyword, SourceLine while_line, int line_num)
        {
            var token_mgr = new TokenManager(while_line.Tokens);
            match_next_token(keyword, token_mgr);

            var condition_tokens = new List<Token>();
            condition_tokens.AddRange(get_remain_tokens(token_mgr));
            compiledTasks.Add(Task.BoolCondition(condition_tokens, line_num));
        }

        #endregion
    }
}