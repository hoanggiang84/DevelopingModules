using System.Collections.Generic;
using HPMacroCommon;
using HPMacroTask;

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
            create_block();
        }

        #region Grammar terms create methods

        private void create_block()
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
                    case Keyword.IF:
                        create_if();
                        break;
                }

                line_content = look_next_line();
            }
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

        private void create_if()
        {
            var end_label = create_new_label();

            string if_false_label;
            create_if_clause(end_label, out if_false_label);

            create_elseif_clause(end_label, ref if_false_label);

            create_else_clause(end_label, if_false_label);

            create_endif(end_label);
        }
        
        #endregion
    }
}
