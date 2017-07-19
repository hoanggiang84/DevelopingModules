using System;
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
            CreateBlock();
        }

        #region Grammar terms create methods

        private void CreateBlock()
        {
            int line_num;
            var line_content = LookNextLine(out line_num);
            while (NotEndBlockLine(line_content))
            {
                switch (line_content.Type)
                {
                    case Keyword.WHITE_SPACE:
                        break;
                    case Keyword.VAR:
                        CreateAssignmentTask();
                        break;
                    case Keyword.GCODE:
                        CreateGCodeTask();
                        break;
                    case Keyword.FUNCTION:
                        CreateMacroBuiltInFunctionTask();
                        break;
                    case Keyword.IF:
                        CreateIfStatement();
                        break;
                }

                line_content = LookNextLine(out line_num);
            }
        }

        private void CreateMacroBuiltInFunctionTask()
        {
            var line_num = 0;
            var tokens = GetNextLine(out line_num).Tokens;
            CreateOneLineStatement(TaskType.BUILT_IN_FUNCTION, tokens,line_num);
        }

        private void CreateGCodeTask()
        {
            var line_num = 0;
            var tokens = GetNextLine(out line_num).Tokens;
            CreateOneLineStatement(TaskType.GCODE, tokens, line_num);
        }

        private void CreateAssignmentTask()
        {
            var line_num = 0;
            var tokens = GetNextLine(out line_num).Tokens;
            CreateOneLineStatement(TaskType.ASSIGNMENT, tokens, line_num);
        }

        private void CreateIfStatement()
        {
            string new_label1;
            string new_label2;

            CreateIfCondition(out new_label1, out new_label2);

            CreateBlock();

            CreateElse(new_label1, new_label2);

            CreateEndIf(new_label1);
        }
        
        #endregion
    }
}
