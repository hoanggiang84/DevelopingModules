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
                        CreateAssignmentStatement();
                        break;
                    case Keyword.GCODE:
                        CreateGCodeStatement();
                        break;
                    case Keyword.FUNCTION:
                        CreateMacroBuiltInFunctionStatement();
                        break;
                    case Keyword.IF:
                        CreateIfStatementTasks();
                        break;
                }

                line_content = LookNextLine(out line_num);
            }
        }

        private void CreateIfStatementTasks()
        {
            var line_num = 0;
            var firstLineTokens = GetNextLine(out line_num).Tokens;
            var token_mgr = new TokenManager(firstLineTokens);
            token_mgr.IgnoreWhiteLookNextToken();

            token_mgr.Match(MacroKeywords.IF);

            var condition_tokens = new List<Token>();
            while (token_mgr.IgnoreWhiteLookNextToken().Type != TokenType.END)
                condition_tokens.Add(token_mgr.IgnoreWhiteGetNextToken());
            compiledTasks.Add(new Task(TaskType.BOOLEAN_EVALUATE, condition_tokens));

            var new_label = CreateNewLabel();
            var new_label2 = CreateNewLabel();
            compiledTasks.Add(new Task(TaskType.BRANCH_FALSE, new_label));

            CreateBlock();

            var next_line_num = 0;
            var next_line = LookNextLine(out next_line_num);
            if(next_line == null)
                throw new Exception(string.Format("Expected '{0}'", MacroKeywords.ENDIF));

            if(next_line.Type == Keyword.ELSE)
            {
                compiledTasks.Add(new Task(TaskType.BRANCH, new_label2));
                compiledTasks.Add(new Task(TaskType.LABEL, new_label));
            }

            if(next_line.Type == Keyword.END_IF)
            {
                token_mgr = new TokenManager(next_line.Tokens);
                token_mgr.IgnoreWhiteLookNextToken();
                token_mgr.Match(MacroKeywords.ENDIF);
                token_mgr.IgnoreWhiteLookNextToken();
                token_mgr.Match(MacroKeywords.END_STATEMENT);
                if (token_mgr.IgnoreWhiteLookNextToken().Type != TokenType.END)
                    throw new Exception(string.Format("Unexpected '{0}'", token_mgr.IgnoreWhiteLookNextToken().Text));
                compiledTasks.Add(new Task(TaskType.LABEL, new_label));
                return;
            }

            CreateBlock();
            next_line_num = 0;
            next_line = LookNextLine(out next_line_num);
            if (next_line == null)
                throw new Exception(string.Format("Expected '{0}'", MacroKeywords.ENDIF));

            token_mgr = new TokenManager(next_line.Tokens);
            token_mgr.IgnoreWhiteLookNextToken();
            token_mgr.Match(MacroKeywords.ENDIF);
            token_mgr.IgnoreWhiteLookNextToken();
            token_mgr.Match(MacroKeywords.END_STATEMENT);
            if(token_mgr.IgnoreWhiteLookNextToken().Type != TokenType.END)
                throw new Exception(string.Format("Unexpected '{0}'", token_mgr.IgnoreWhiteLookNextToken().Text));
            compiledTasks.Add(new Task(TaskType.LABEL, new_label));
        }

        private void CreateMacroBuiltInFunctionStatement()
        {
            var line_num = 0;
            var tokens = GetNextLine(out line_num).Tokens;
            CreateOneLineStatement(TaskType.BUILT_IN_FUNCTION, tokens,line_num);
        }

        private void CreateGCodeStatement()
        {
            var line_num = 0;
            var tokens = GetNextLine(out line_num).Tokens;
            CreateOneLineStatement(TaskType.GCODE, tokens, line_num);
        }

        private void CreateAssignmentStatement()
        {
            var line_num = 0;
            var tokens = GetNextLine(out line_num).Tokens;
            CreateOneLineStatement(TaskType.ASSIGNMENT, tokens, line_num);
        }

     
    }
}
