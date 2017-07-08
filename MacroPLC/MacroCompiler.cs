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
    public class MacroCompiler
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
            int lineNum;
            var lineContent = sourceManager.GetNextLine(out lineNum);
            while (lineContent != null)
            {
                var lexScanner = new MacroLexicalScanner(lineContent);
                var token = lexScanner.ScanNext();
                var lineTokens = new List<Token>();
                while (token.Type != TokenType.END)
                {
                    lineTokens.Add(token);
                    token = lexScanner.ScanNext();
                }

                if(GCodeValidate.ValidateGCodeCommand(lineTokens.First().Text))
                {
                    CreateGCodeStatement(lineTokens,lineNum);
                }
                else if(lineTokens.First().Type == TokenType.GLOBAL_VAR
                    || lineTokens.First().Type == TokenType.LOCAL_VAR)
                {
                    CreateAssignmentStatement(lineTokens,lineNum);
                }
                else if (HPFUNC.IsMacroReturnValueFunction(lineTokens.First().Text)
                    || HPFUNC.IsVoidMacroFunction(lineTokens.First().Text))
                {
                    CreateMacroBuiltInFunctionStatement(lineTokens, lineNum);
                }
              
                lineContent = sourceManager.GetNextLine(out lineNum);
            }
        }

        private void CreateMacroBuiltInFunctionStatement(List<Token> tokens, int lineNumber)
        {
            validateEndStatement(tokens,lineNumber);
            var funcTask = new Task(TaskType.BUILT_IN_FUNCTION, tokens.Take(tokens.Count - 1).ToList());
            funcTask.SetLineNumber(lineNumber);
            compiledTasks.Add(funcTask);
        }

        private static void validateEndStatement(List<Token> tokens, int lineNumber)
        {
            if (tokens.Last().Text != MacroKeywords.END_STATEMENT)
                throw new Exception(string.Format("Expected '{0}' at line {1}", MacroKeywords.END_STATEMENT, lineNumber));
        }

        private void CreateGCodeStatement(List<Token> tokens, int lineNumber)
        {
            validateEndStatement(tokens, lineNumber);
            var gCodeTask = new Task(TaskType.GCODE, tokens.Take(tokens.Count - 1).ToList());
            gCodeTask.SetLineNumber(lineNumber);
            compiledTasks.Add(gCodeTask);
        }

        private void CreateAssignmentStatement(List<Token> tokens, int lineNumber )
        {
            validateEndStatement(tokens,lineNumber);
            var assignTask = new Task(TaskType.ASSIGNMENT, tokens.Take(tokens.Count - 1).ToList());
            assignTask.SetLineNumber(lineNumber);
            compiledTasks.Add(assignTask);
        }
    }
}
