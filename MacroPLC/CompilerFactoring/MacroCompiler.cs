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

        private void create_condition(string keyword, SourceLine source_line, int line_num)
        {
            var token_mgr = new TokenManager(source_line.Tokens);
            match_next_token(keyword, token_mgr);

            var condition_tokens = new List<Token>();
            condition_tokens.AddRange(get_remain_tokens(token_mgr));
            compiledTasks.Add(Task.BoolCondition(condition_tokens, line_num));
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

        private void verify_next_source_line_with_end_statement(Keyword line_type)
        {
            verify_next_line_not_null(line_type);

            var token_mgr = new TokenManager(look_next_line().Tokens);
            match_next_token(MacroKeywords.GetKeywordString(line_type), token_mgr);
            match_next_token(MacroKeywords.END_STATEMENT, token_mgr);

            verify_next_end_token(token_mgr);
        }

        private void create_branch_false(string label)
        {
            compiledTasks.Add(Task.BranchIfFalse(label));
        }

        private void create_branch_false(int line_num, string false_label)
        {
            compiledTasks.Add(Task.BranchIfFalse(false_label, line_num));
        }

        private void create_branch_true(string repeat_label)
        {
            compiledTasks.Add(Task.BranchIfTrue(repeat_label));
        }

        private void create_branch_label(string label)
        {
            compiledTasks.Add(Task.Branch(label));
        }

        private void create_post_label(string label, int line_num)
        {
            compiledTasks.Add(Task.PostLabel(label, line_num));
        }

        private static List<Token> get_remain_tokens(TokenManager token_mgr)
        {
            var condition_tokens = new List<Token>();
            while (token_mgr.IgnoreWhiteLookNextToken().Type != TokenType.END)
                condition_tokens.Add(token_mgr.IgnoreWhiteGetNextToken());
            return condition_tokens;
        }

        private void create_arithmetic_expression(List<Token> tokens, int line_num)
        {
            var task = Task.ArithmeticExpression(tokens);
            task.SetLineNumber(line_num);
            compiledTasks.Add(task);
        }

        private void create_if_equal_branch(int value, string label)
        {
            compiledTasks.Add(Task.BranchIfEqual(value, label));
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

        // ELSEIF CLAUSE
        private void create_elseif_clause(string end_loop_label, 
            ref string if_false_label, 
            out string end_label)
        {
            if(look_next_line().Type != Keyword.ELSEIF)     // No ELSEIF
            {
                end_label = string.Empty;
                return;
            }

            end_label = create_new_label();
            while (look_next_line().Type == Keyword.ELSEIF)
            {
                // End label needed only when there are ELSEIF
                // After each IF or ELSEIF block, branching to end label (ENDIF)
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
        private void create_else_clause(string end_loop_label, 
            string end_label, 
            ref string if_false_label)
        {
            verify_next_line_not_null(Keyword.END_IF);

            if (look_next_line().Type != Keyword.ELSE) // No ELSE
                return;

            var next_line_num = get_next_line().LineNumber;


            if (end_label.IsNullOrWhite())   // No ELSEIF
                end_label = create_new_label();

            // IF condition is true, branching to end label
            create_branch_label(end_label);             

            create_post_label(if_false_label, next_line_num);

            create_block(end_loop_label);

            if_false_label = end_label;
        }

        // END IF
        private void create_endif(string label)
        {
            verify_next_source_line_with_end_statement(Keyword.END_IF);
            var line_num = get_next_line().LineNumber;
            compiledTasks.Add(Task.PostLabel(label, line_num));
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


        private void create_end_while(string end_label)
        {
            verify_next_source_line_with_end_statement(Keyword.END_WHILE);
            var end_line_num = get_next_line().LineNumber;
            create_post_label(end_label, end_line_num);
        }

        #endregion

        #region Loop

        private void verify_loop_keyword(SourceLine loop_line)
        {
            verify_keyword_without_endstatement(MacroKeywords.LOOP, loop_line);
        }

        #endregion

        #region For

        private void create_update_for_variable(Token variable_token, List<Token> by_value_tokens, int line_number)
        {
            compiledTasks.Add(Task.IncreaseVariable(variable_token, by_value_tokens, line_number));
        }

        private void create_branch_if_greater(Token variable_token, string endfor_label)
        {
            compiledTasks.Add(Task.BranchIfGreater(variable_token.Text, endfor_label));
        }

        private void create_for_assignment(List<Token> assign_tokens, int line_number)
        {
            compiledTasks.Add(Task.CreateAssignmentTask(assign_tokens, line_number));
        }

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
                by_value_tokens = get_remain_tokens(token_mgr);
            }
            else
            {
                by_value_tokens.Add(new Token("1", TokenType.NUMBER));
            }
            return variable_token;
        }

        private void create_endfor(string endfor_label)
        {
            verify_next_source_line_with_end_statement(Keyword.END_FOR);
            var line_number = get_next_line().LineNumber;
            create_post_label(endfor_label, line_number);
        }

        #endregion

        #region Repeat

        private void create_until_condition(SourceLine until_line)
        {
            create_condition(MacroKeywords.UNTIL, until_line, until_line.LineNumber);
        }

        private void verify_repeat_keyword(SourceLine repeat_line)
        {
            verify_keyword_without_endstatement(MacroKeywords.REPEAT, repeat_line);
        }

        private void verify_keyword_without_endstatement(string keyword, SourceLine repeat_line)
        {
            var token_mgr = new TokenManager(repeat_line.Tokens);
            match_next_token(keyword, token_mgr);
            verify_next_end_token(token_mgr);
        }

        #endregion

        #region Switch

        private void create_switch_expression(out int switch_line_num)
        {
            var switch_line = get_next_line();
            var token_mgr = new TokenManager(switch_line.Tokens);
            match_next_token(MacroKeywords.SWITCH, token_mgr);
            var switch_key_tokens = get_remain_tokens(token_mgr);

            create_arithmetic_expression(switch_key_tokens, switch_line.LineNumber);
            switch_line_num = switch_line.LineNumber;
        }

        private string create_all_cases(string end_switch_label, 
            ref Dictionary<int, string> case_label_storage)
        {
            var default_label = string.Empty;
            var next_line = look_next_line();
            while (next_line.Type != Keyword.END_SWITCH
                   && next_line.Type != Keyword.END)
            {
                next_line = get_next_line();
                switch (next_line.Type)
                {
                    case Keyword.CASE:
                        var case_token_mgr = new TokenManager(next_line.Tokens);
                        match_next_token(MacroKeywords.CASE, case_token_mgr);

                        var case_label = create_new_label();
                        var next_token = case_token_mgr.IgnoreWhiteLookNextToken();
                        extract_all_values_in_this_case(case_label, next_token, case_token_mgr, 
                            ref case_label_storage);

                        match_next_token(MacroKeywords.COLON, case_token_mgr);

                        create_post_label(case_label, next_line.LineNumber);

                        create_block(end_switch_label);

                        create_branch_label(end_switch_label);

                        break;

                    case Keyword.DEFAULT:
                        default_label = create_new_label();
                        create_post_label(default_label, next_line.LineNumber);

                        create_block(end_switch_label);

                        create_branch_label(end_switch_label);

                        break;

                    default:
                        throw new Exception(string.Format("Unrecognized statement '{0}'",
                                                          next_line.Text));
                }
                next_line = look_next_line();
            }
            return default_label;
        }

        private void extract_all_values_in_this_case(string case_label, 
            Token next_token, 
            TokenManager case_token_mgr, 
            ref Dictionary<int, string> case_label_storage)
        {
            while (next_token.Text != MacroKeywords.COLON
                   && next_token.Type != TokenType.END)
            {
                next_token = case_token_mgr.IgnoreWhiteGetNextToken();

                int case_value;
                if (!next_token.Text.TryParseInt32(out case_value))
                    throw new Exception(string.Format("Expected an integer number '{0}'",
                                                      next_token.Text));

                if (case_label_storage.ContainsKey(case_value))
                    throw new Exception(string.Format("Duplicate case '{0}'", case_value));

                case_label_storage.Add(case_value, case_label);

                if (case_token_mgr.IgnoreWhiteLookNextToken().Text == MacroKeywords.COMMA)
                    match_next_token(MacroKeywords.COMMA, case_token_mgr);

                next_token = case_token_mgr.IgnoreWhiteLookNextToken();
            }
        }

        #endregion
    }
}