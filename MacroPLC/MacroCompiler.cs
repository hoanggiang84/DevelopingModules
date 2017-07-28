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
            var end_main_label = "ENDMAIN";
            var end_main = create_program_label(end_main_label);
            create_block(end_main);
            create_post_label(create_program_label(end_main_label), 0);
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
            // Expression to evaluate switch value
            int switch_line_number;
            create_switch_expression(out switch_line_number);

            // Branch to select case label
            var select_case_label = create_new_label();
            create_branch_label(select_case_label, switch_line_number);

            // Post case labels
            // Create block for each case
            // Branch to end switch label after each block
            var case_label_storage = new Dictionary<int, string>();
            var end_switch_label = create_new_label();
            var default_label = create_all_cases(end_switch_label, ref case_label_storage);

            // Select case label
            create_post_label(select_case_label, switch_line_number);

            // Branch to the case with value equals switch value
            foreach (var e in case_label_storage)
                create_if_equal_branch(e.Key, e.Value);

            // Branch to default case if not equal case value
            if(default_label.IsNotNullOrWhite())
                create_branch_label(default_label, look_next_line().LineNumber);

            verify_next_source_line_with_end_statement(Keyword.END_SWITCH);

            // End switch label
            var end_switch_line = get_next_line();
            create_post_label(end_switch_label, end_switch_line.LineNumber);
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

            var p_label = create_program_label(label_token.Text);
            compiledTasks.Add(Task.Branch(p_label, goto_line.LineNumber));
        }

        private void create_repeat()
        {
            var repeat_line = get_next_line();
            verify_repeat_keyword(repeat_line);

            var repeat_label = create_new_label();
            create_post_label(repeat_label, repeat_line.LineNumber);

            var end_label = create_new_label();
            create_block(end_label);

            var until_line = get_next_line();
            create_until_condition(until_line);

            create_branch_true(repeat_label, until_line.LineNumber);

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
            create_for_assignment(assign_tokens, line_number);

            // 'Continue for' label
            var continue_loop_label = create_new_label();
            create_post_label(continue_loop_label, line_number);

            // Evaluate upper limit
            create_arithmetic_expression(to_value_tokens, line_number);
            
            // Branch to 'end for' label if for variable is greater than upper limit
            var endfor_label = create_new_label();
            create_branch_if_greater(variable_token, endfor_label);

            create_block(endfor_label);

            // Update for variable (with 'for' line number)
            create_update_for_variable(variable_token, by_value_tokens, line_number);

            // Branch to 'continue for' label
            create_branch_label(continue_loop_label, look_next_line().LineNumber);

            create_endfor(endfor_label);
        }

        private void create_break(string end_loop_label)
        {
            verify_next_source_line_with_end_statement(Keyword.BREAK);
            var line_num = get_next_line().LineNumber;
            compiledTasks.Add(Task.Branch(end_loop_label, line_num));
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

            create_branch_label(loop_label, look_next_line().LineNumber);

            verify_next_source_line_with_end_statement(Keyword.END_LOOP);

            var end_line_num = get_next_line().LineNumber;
            create_post_label(break_loop_label, end_line_num);
        }

        private void create_while()
        {
            var while_label = create_new_label();
            var end_label = create_new_label();

            create_while_condition(while_label);

            create_branch_false(end_label, look_next_line().LineNumber);

            create_block(end_label);

            create_branch_label(while_label, look_next_line().LineNumber);

            create_end_while(end_label);
        }

        private void create_if(string break_loop_label)
        {
            // In case there BREAK inside if statement blocks, branching to end label of nearest loop
            // Every 'loop' type control (LOOP, WHILE, FOR...) has an end label (break loop label for BREAK)
            string if_false_label;
            create_if_clause(break_loop_label, out if_false_label);
            
            string end_label;
            create_elseif_clause(break_loop_label, ref if_false_label, out end_label);

            create_else_clause(break_loop_label, end_label, ref if_false_label);

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
