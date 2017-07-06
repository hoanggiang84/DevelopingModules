using System;
using System.Collections.Generic;
using HPMacroCommon;
using HPMacroTask;
using HPVariableRepository;
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

                if(lineTokens.First().Type == TokenType.GLOBAL_VAR
                    || lineTokens.First().Type == TokenType.LOCAL_VAR)
                {
                    CreateAssignment(lineTokens,lineNum);
                }
                
              
                lineContent = sourceManager.GetNextLine(out lineNum);
            }
        }

        private void CreateAssignment(List<Token> tokens, int lineNumber )
        {
            if(tokens.Last().Text != MacroKeywords.END_STATEMENT)  
                throw new Exception(string.Format("Expected '{0}' at line {1}", MacroKeywords.END_STATEMENT, lineNumber));
            var assignTask = new Task(TaskType.ASSIGNMENT, tokens);
            assignTask.SetLineNumber(lineNumber);
            compiledTasks.Add(assignTask);
        }
    }
}
