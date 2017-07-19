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
            create_single_line_statement(TaskType.BUILT_IN_FUNCTION, tokens,line_num);
        }

        private void create_gcode()
        {
            var line_num = 0;
            var tokens = get_next_line(out line_num).Tokens;
            create_single_line_statement(TaskType.GCODE, tokens, line_num);
        }

        private void create_assignment()
        {
            var line_num = 0;
            var tokens = get_next_line(out line_num).Tokens;
            create_single_line_statement(TaskType.ASSIGNMENT, tokens, line_num);
        }

        private void create_if()
        {
            string new_label1;
            string new_label2;

            create_if_condition(out new_label1, out new_label2);

            create_block();

            create_else_block(new_label1, ref new_label2);

            create_endif(new_label2);
        }
        
        #endregion
    }
}
