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
        private SourceManager sourceManager;
        public List<Task> compiledTasks { get; private set; }
        public MacroCompiler(string source)
        {
            sourceManager = new SourceManager(source);
            compiledTasks = new List<Task>();
        }

        public void Compile()
        {
            compiledTasks.Clear();
            _label_count = 0;
            var end_main = create_program_label("ENDMAIN");
            create_block(end_main);
            create_post_label(create_program_label("ENDMAIN"),0);
        }

        #region Grammar terms create methods

        private void create_block(string loop_label)
        {
            var line_content = look_next_line();
            while (not_end_block_line(line_content))
            {
                switch (line_content.Type)
                {
                    case Keyword.WHITE_SPACE:
                        get_next_line();
                        break;
                    case Keyword.LABEL:
                        create_label_goto();
                        break;
                    case Keyword.VAR:
                        create_assignment();
                        break;
                    case Keyword.GCODE:
                        create_gcode();
                        break;
                    case Keyword.FUNCTION:
                        create_built_in_function();
                        break;
                    case Keyword.BREAK:
                        create_break(loop_label);
                        break;
                    case Keyword.GOTO:
                        create_goto();
                        break;
                    case Keyword.IF:
                        create_if(loop_label);
                        break;
                    case Keyword.WHILE:
                        create_while();
                        break;
                    case Keyword.LOOP:
                        create_loop();
                        break;
                    case Keyword.FOR:
                        create_for();
                        break;
                    case Keyword.REPEAT:
                        create_repeat();
                        break;
                    case Keyword.SWITCH:
                        create_switch();
                        break;
                    default:
                        line_content = get_next_line();
                        break;
                }

                line_content = look_next_line();
            }
        }

        private void create_switch()
        {
            var switch_line = get_next_line();
            var token_mgr = new TokenManager(switch_line.Tokens);
            match_next_token(MacroKeywords.SWITCH, token_mgr);
            var switch_key_tokens = get_remain_tokens(token_mgr);

            create_arithmetic_expression(switch_key_tokens, switch_line.LineNumber);

            var select_case_label = create_new_label();
            create_branch_label(select_case_label);

            var case_label_storage = new Dictionary<int, string>();
            var end_switch_label = create_new_label();

            var default_label = string.Empty;
            var next_line = look_next_line();
            while (next_line.Type != Keyword.END_SWITCH
                && next_line.Type != Keyword.END)
            {
                next_line = get_next_line();
                switch (next_line.Type)
                {
                    case Keyword.CASE:
                        var case_label = create_new_label();
                        var case_token_mgr = new TokenManager(next_line.Tokens);
                        match_next_token(MacroKeywords.CASE,case_token_mgr);
                        var next_token = case_token_mgr.IgnoreWhiteLookNextToken();
                        while (next_token.Text != MacroKeywords.COLON
                            && next_token.Type != TokenType.END)
                        {
                            next_token = case_token_mgr.IgnoreWhiteGetNextToken();
                            int case_value;
                            if(!next_token.Text.TryParseInt32(out case_value))
                                throw new Exception(string.Format("Expected an integer number '{0}'", 
                                    next_token.Text));

                            if(case_label_storage.ContainsKey(case_value))
                                throw new Exception(string.Format("Duplicate case '{0}'", case_value));

                            case_label_storage.Add(case_value, case_label);

                            if(case_token_mgr.IgnoreWhiteLookNextToken().Text == MacroKeywords.COMMA)
                                match_next_token(MacroKeywords.COMMA, case_token_mgr);
                            next_token = case_token_mgr.IgnoreWhiteLookNextToken();
                        }

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

            create_post_label(select_case_label, switch_line.LineNumber);
            foreach (var e in case_label_storage)
                compiledTasks.Add(Task.BranchIfEqual(e.Key, e.Value));

            if(default_label.IsNotNullOrWhite())
                create_branch_label(default_label);

            verify_next_source_line(Keyword.END_SWITCH);

            var end_switch_line = get_next_line();
            create_post_label(end_switch_label, end_switch_line.LineNumber);
        }

        private void create_arithmetic_expression(List<Token> tokens, int line_num)
        {
            var task = Task.ArithmeticExpression(tokens);
            task.SetLineNumber(line_num);
            compiledTasks.Add(task);
        }

        private void create_label_goto()
        {
            var label_line = get_next_line();
            var token_mgr = new TokenManager(label_line.Tokens);
            var goto_label = create_program_label(token_mgr.IgnoreWhiteGetNextToken().Text);
            match_next_token(MacroKeywords.COLON, token_mgr);
            create_post_label(goto_label, label_line.LineNumber);
        }

        private void create_goto()
        {
            var goto_line = get_next_line();
            var token_mgr = new TokenManager(goto_line.Tokens);
            match_next_token(MacroKeywords.GOTO, token_mgr);
            var label_token = token_mgr.IgnoreWhiteGetNextToken();
            if(label_token.Type != TokenType.IDENTIFIER)
                throw new Exception(string.Format("Invalid label '{0}'", label_token.Text));
            match_next_token(MacroKeywords.END_STATEMENT,token_mgr);
            compiledTasks.Add(Task.Branch(label_token.Text, goto_line.LineNumber));
        }

        private void create_repeat()
        {
            var repeat_line = get_next_line();
            verify_repeat_keyword(repeat_line);

            var repeat_label = create_new_label();
            create_post_label(repeat_label,repeat_line.LineNumber);

            var end_label = create_new_label();
            create_block(end_label);

            var until_line = get_next_line();
            var token_mgr = new TokenManager(until_line.Tokens);
            match_next_token(MacroKeywords.UNTIL, token_mgr);

            var condition_tokens = get_remain_tokens(token_mgr);
            compiledTasks.Add(Task.BoolCondition(condition_tokens, until_line.LineNumber));

            compiledTasks.Add(Task.BranchIfTrue(repeat_label));

            create_post_label(end_label, until_line.LineNumber);
        }

        private void create_for()
        {
            var next_line = get_next_line();
            var line_number = next_line.LineNumber;

            List<Token> to_value_tokens;
            List<Token> by_value_tokens;
            List<Token> assign_tokens;
            var variable_token = extract_for_variable_assignment_info(next_line, 
                out assign_tokens, out to_value_tokens, out by_value_tokens);

            // Initial assign for variable
            compiledTasks.Add(Task.CreateAssignmentTask(assign_tokens, line_number));

            // Continue for label
            var continue_loop_label = create_new_label();
            create_post_label(continue_loop_label, line_number);

            // Evaluate upper limit
            compiledTasks.Add(Task.ArithmeticExpression(to_value_tokens));
            
            // Jump to end for label if for variable is greater than upper limit
            var endfor_label = create_new_label();
            compiledTasks.Add(Task.BranchIfGreater(variable_token.Text, endfor_label));

            // Inside block
            create_block(endfor_label);

            // Update for variable (line number 'for' line number)
            compiledTasks.Add(Task.IncreaseVariable(variable_token, by_value_tokens, line_number));

            // Jump to continue for label
            create_branch_label(continue_loop_label);

            // End for label
            create_endfor(endfor_label);
        }

        private void create_break(string loop_label)
        {
            verify_next_source_line(Keyword.BREAK);
            var line_num = get_next_line().LineNumber;
            compiledTasks.Add(Task.Branch(loop_label, line_num));
        }

        private void create_loop()
        {
            var loop_line = get_next_line();
            var line_num = loop_line.LineNumber;
            var loop_label = create_new_label();
            var break_loop_label = create_new_label();

            verify_loop_keyword(loop_line);

            create_post_label(loop_label, line_num);

            create_block(break_loop_label);

            create_branch_label(loop_label);

            verify_next_source_line(Keyword.END_LOOP);

            var end_line_num = get_next_line().LineNumber;
            create_post_label(break_loop_label, end_line_num);
        }

        private void create_while()
        {
            var while_label = create_new_label();
            var end_label = create_new_label();

            create_while_condition(while_label);

            create_branch_false(end_label);

            create_block(end_label);

            create_branch_label(while_label);

            create_end_while(end_label);
        }

        private void create_if(string end_loop_label)
        {
            string if_false_label;
            create_if_clause(end_loop_label, out if_false_label);
            
            string end_label;
            create_elseif_clause(end_loop_label, ref if_false_label, out end_label);

            create_else_clause(end_loop_label, end_label, ref if_false_label);

            create_endif(if_false_label);
        }

        private void create_built_in_function()
        {
            int line_num;
            var tokens = extract_next_line_statement_tokens(out line_num);
            compiledTasks.Add(Task.CreateBuiltInFunctionTask(tokens, line_num));
        }

        private void create_gcode()
        {
            int line_num;
            var tokens = extract_next_line_statement_tokens(out line_num);
            compiledTasks.Add(Task.CreateGCodeTask(tokens, line_num));
        }

        private void create_assignment()
        {
            int line_num;
            var tokens = extract_next_line_statement_tokens(out line_num);
            compiledTasks.Add(Task.CreateAssignmentTask(tokens,line_num));
        }

        #endregion
    }
}
