using System.Collections.Generic;
using HPMacroCommon;
using HPMacroTask;
using MacroLexScn;

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
                }

                line_content = look_next_line();
            }
        }

        private void create_for()
        {
            throw new System.NotImplementedException();
        }

        private void create_break(string loop_label)
        {
            verify_next_source_line(Keyword.BREAK);
            var line_num = 0;
            get_next_line(out line_num);
            compiledTasks.Add(Task.Branch(loop_label, line_num));
        }

        private void create_loop()
        {
            var line_num = 0;
            var loop_line = get_next_line(out line_num);
            verify_loop_keyword(loop_line);

            var loop_label = create_new_label();
            create_post_label(loop_label, line_num);

            var break_loop_label = create_new_label();
            create_block(break_loop_label);

            verify_next_source_line(Keyword.END_LOOP);
            create_branch_label(loop_label);

            var end_line_num = 0;
            get_next_line(out end_line_num);
            create_post_label(break_loop_label, end_line_num);
        }

        private static void verify_loop_keyword(SourceLine loop_line)
        {
            var token_mgr = new TokenManager(loop_line.Tokens);
            match_next_token(MacroKeywords.LOOP, token_mgr);
            verify_next_end_token(token_mgr);
        }

        private void create_while()
        {
            var while_label = create_new_label();
            create_while_condition(while_label);

            var end_label = create_new_label();
            create_branch_false(end_label);

            create_block(end_label);

            verify_next_source_line(Keyword.END_WHILE);

            create_branch_label(while_label);

            var end_line_num = 0;
            get_next_line(out end_line_num);
            create_post_label(end_label, end_line_num);
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
            var line_num = 0;
            var tokens = get_next_line(out line_num).Tokens;
            compiledTasks.Add(Task.CreateBuiltInFunctionTask(extract_statement_tokens(tokens), line_num));
        }

        private void create_gcode()
        {
            var line_num = 0;
            var tokens = get_next_line(out line_num).Tokens;
            compiledTasks.Add(Task.CreateGCodeTask(extract_statement_tokens(tokens), line_num));
        }

        private void create_assignment()
        {
            var line_num = 0;
            var tokens = get_next_line(out line_num).Tokens;
            compiledTasks.Add(Task.CreateAssignmentTask(extract_statement_tokens(tokens),line_num));
        }

        #endregion
    }
}
