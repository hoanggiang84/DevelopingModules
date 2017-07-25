using System;
using System.Collections.Generic;
using HPMacroCommon;
using HPMacroTask;
using MacroLexScn;
using UtilitiesVS2008WinCE;

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

        private static string create_program_label(string label)
        {
            return "P" + label;
        }

        private SourceLine get_next_line()
        {
            return sourceManager.GetNextLine();
        }

        private SourceLine look_next_line()
        {
            return sourceManager.LookCurrentLine();
        }

        private static List<Token> extract_statement_tokens(IEnumerable<Token> tokens)
        {
            // Skip last end statement token ';'
            var token_mgr = new TokenManager(tokens);
            var tokenList = get_tokens_until_keyword_or_end(MacroKeywords.END_STATEMENT, token_mgr);
            match_next_token(MacroKeywords.END_STATEMENT, token_mgr);
           
            verify_next_end_token(token_mgr);

            return tokenList;
        }

        private void verify_next_line_not_null(Keyword expected)
        {
            if (look_next_line() == null)
                throw new Exception(string.Format("Expected '{0}' statement", expected));
        }

        private List<Token> extract_next_line_statement_tokens(out int line_num)
        {
            var next_line = get_next_line();
            var tokens = next_line.Tokens;
            line_num = next_line.LineNumber;
            return extract_statement_tokens(tokens);
        }

        private static List<Token> get_tokens_until_keyword_or_end(string keyword, TokenManager token_mgr)
        {
            var tokens = new List<Token>();
            var next_token = token_mgr.IgnoreWhiteLookNextToken();
            while (next_token.Type != TokenType.END
                   && next_token.Text != keyword)
            {
                tokens.Add(token_mgr.IgnoreWhiteGetNextToken());
                next_token = token_mgr.IgnoreWhiteLookNextToken();
            }
            return tokens;
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
                case Keyword.END_LOOP:
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
        private void create_if_clause(string end_loop_label, out string if_false_label)
        {
            var if_line = get_next_line();
            var if_line_num = if_line.LineNumber;
            if_false_label = create_new_label();

            create_if_condition(if_line, if_line_num);

            create_branch_false(if_line_num, if_false_label);

            create_block(end_loop_label);
        }

        private void create_if_condition(SourceLine if_line, int if_line_num)
        {
            create_condition(MacroKeywords.IF, if_line, if_line_num);
        }

        private void create_branch_false(string label)
        {
            compiledTasks.Add(Task.BranchIfFalse(label));
        }

        private void create_branch_false(int line_num, string false_label)
        {
            compiledTasks.Add(Task.BranchIfFalse(false_label, line_num));
        }

        // ELSEIF CLAUSE
        private void create_elseif_clause(string end_loop_label, ref string if_false_label, out string end_label)
        {
            if(look_next_line().Type != Keyword.ELSEIF)
            {
                end_label = string.Empty;
                return;
            }

            end_label = create_new_label();
            while (look_next_line().Type == Keyword.ELSEIF)
            {
                create_branch_label(end_label);

                var elseif_line = get_next_line();
                var line_num = elseif_line.LineNumber;
                create_post_label(if_false_label, line_num);

                create_condition(MacroKeywords.ELSEIF, elseif_line, line_num);

                if_false_label = create_new_label();
                create_branch_false(line_num, if_false_label);

                create_block(end_loop_label);
            }
        }

        // ELSE CLAUSE
        private void create_else_clause(string end_loop_label, string end_label, ref string if_false_label)
        {
            verify_next_line_not_null(Keyword.END_IF);

            if (look_next_line().Type != Keyword.ELSE) 
                return;

            var next_line_num = get_next_line().LineNumber;

            if(end_label.IsNullOrWhite())
                end_label = create_new_label();

            create_branch_label(end_label);

            create_post_label(if_false_label, next_line_num);

            create_block(end_loop_label);

            if_false_label = end_label;
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
            var line_num = get_next_line().LineNumber;
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

        #endregion

        #region While

        private void create_while_condition(string while_label)
        {
            var while_line = get_next_line();
            var line_num = while_line.LineNumber;

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

        private void create_end_while(string end_label)
        {
            verify_next_source_line(Keyword.END_WHILE);
            var end_line_num = get_next_line().LineNumber;
            create_post_label(end_label, end_line_num);
        }

        #endregion

        #region Loop
        private static void verify_loop_keyword(SourceLine loop_line)
        {
            // Only LOOP, no end statement ';'
            var token_mgr = new TokenManager(loop_line.Tokens);
            match_next_token(MacroKeywords.LOOP, token_mgr);
            verify_next_end_token(token_mgr);
        }
        #endregion

        #region For

        private static Token extract_for_variable_assignment_info(SourceLine next_line, out List<Token> assign_tokens, out List<Token> to_value_tokens, out List<Token> by_value_tokens)
        {
            // For <Assigment>
            var token_mgr = new TokenManager(next_line.Tokens);
            match_next_token(MacroKeywords.FOR, token_mgr);
            var variable_token = token_mgr.IgnoreWhiteLookNextToken();
            assign_tokens = get_tokens_until_keyword_or_end(MacroKeywords.TO, token_mgr);

            // To 
            match_next_token(MacroKeywords.TO, token_mgr);
            to_value_tokens = get_tokens_until_keyword_or_end(MacroKeywords.BY, token_mgr);

            // By
            by_value_tokens = new List<Token>();
            if (token_mgr.IgnoreWhiteLookNextToken().Text == MacroKeywords.BY)
            {
                match_next_token(MacroKeywords.BY, token_mgr);
                by_value_tokens = get_tokens_until_keyword_or_end(MacroKeywords.END, token_mgr);
            }
            else
            {
                by_value_tokens.Add(new Token("1", TokenType.NUMBER));
            }
            return variable_token;
        }

        private void create_endfor(string endfor_label)
        {
            verify_next_source_line(Keyword.END_FOR);
            var line_number = get_next_line().LineNumber;
            create_post_label(endfor_label, line_number);
        }

        #endregion
    }
}