using System;
using System.Collections.Generic;
using HPMacroCommon;
using HPMacroTask;
using MacroLexScn;
using System.Linq;
using MacroVariableDB;

namespace MacroPLC
{
    public class MacroCompiler
    {
        private SourceManager sourceManager;
        private List<Task> compiledTasks = new List<Task>();
        public MacroCompiler(string source)
        {
            sourceManager = new SourceManager(source);
            VariableDB.InitializeVariables();
        }

        public void Compile()
        {
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

        private int taskIndex;
        public int StepExecute()
        {
            if (taskIndex < compiledTasks.Count)
            {
                var curTask = compiledTasks[taskIndex];
                taskIndex++;

                var lineNumber = curTask.LineNumber;
                switch (curTask.Type)
                {
                    case TaskType.ASSIGNMENT:
                        new Assignment(curTask.Tokens).Execute();
                        return lineNumber;
                }
                
            }
            return -1;
        }

    }
}
