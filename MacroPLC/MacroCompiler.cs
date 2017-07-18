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

            var condition_tokens = new List<Token>();
            while (token_mgr.IgnoreWhiteLookNextToken().Type != TokenType.END)
                condition_tokens.Add(token_mgr.IgnoreWhiteGetNextToken());
            compiledTasks.Add(new Task(TaskType.BOOLEAN_EVALUATE, condition_tokens));

            var new_label = CreateNewLabel();
            var new_label2 = CreateNewLabel();
            compiledTasks.Add(new Task(TaskType.BRANCH_FALSE, new_label));

            var next_line_num = 0;
            var next_line = GetNextLine(out next_line_num);
            while (next_line != null)
            {
                if(next_line.Type == Keyword.END_IF)
                    break;

                if(next_line.Type == Keyword.ELSE)
                    break;

                switch (next_line.Type)
                {
                    case Keyword.VAR:
                        CreateAssignmentStatement(next_line.Tokens, line_num);
                        break;
                    case Keyword.GCODE:
                        CreateGCodeStatement(next_line.Tokens, line_num);
                        break;
                    case Keyword.FUNCTION:
                        CreateMacroBuiltInFunctionStatement(next_line.Tokens, line_num);
                        break;
                }
                next_line = GetNextLine(out next_line_num);
            }

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

            next_line = GetNextLine(out line_num);
            while (next_line != null)
            {
                if (next_line.Type == Keyword.END_IF)
                    break;

                switch (next_line.Type)
                {
                    case Keyword.VAR:
                        CreateAssignmentStatement(next_line.Tokens, line_num);
                        break;
                    case Keyword.GCODE:
                        CreateGCodeStatement(next_line.Tokens, line_num);
                        break;
                    case Keyword.FUNCTION:
                        CreateMacroBuiltInFunctionStatement(next_line.Tokens, line_num);
                        break;
                }

                next_line = GetNextLine(out next_line_num);
            }

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
