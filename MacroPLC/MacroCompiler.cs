using System;
using System.Collections.Generic;
using HPGCodeValidation;
using HPMacroCommon;
using HPMacroFunctions;
using HPMacroTask;
using MacroLexScn;
using System.Linq;

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
            int line_num;
            var line_content = GetNextLine(out line_num);
            while (line_content != null)
            {
                var line_token = line_content.Tokens;
                switch (line_content.Type)
                {
                    case Keyword.VAR:
                        CreateAssignmentStatement(line_token, line_num);
                        break;
                    case Keyword.GCODE:
                        CreateGCodeStatement(line_token, line_num);
                        break;
                    case Keyword.FUNCTION:
                        CreateMacroBuiltInFunctionStatement(line_token, line_num);
                        break;
                    case Keyword.IF:
                        CreateIfStatementTasks(line_token, line_num);
                        break;
                }

                line_content = GetNextLine(out line_num);
            }
        }

        private void CreateIfStatementTasks(IEnumerable<Token> firstLineTokens, int line_num)
        {
            var token_mgr = new TokenManager(firstLineTokens);
            token_mgr.IgnoreWhiteLookNextToken();
            token_mgr.Match(MacroKeywords.IF);

        }

        private void CreateMacroBuiltInFunctionStatement(IEnumerable<Token> tokens, int line_num)
        {
            CreateOneLineStatement(TaskType.BUILT_IN_FUNCTION, tokens,line_num);
        }

        private void CreateGCodeStatement(IEnumerable<Token> tokens, int line_num)
        {
            CreateOneLineStatement(TaskType.GCODE, tokens, line_num);
        }

        private void CreateAssignmentStatement(IEnumerable<Token> tokens, int line_num)
        {
            CreateOneLineStatement(TaskType.ASSIGNMENT, tokens, line_num);
        }

     
    }
}
