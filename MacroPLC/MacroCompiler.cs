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
                else if(isVariableToken(lineTokens.First()))
                {
                    CreateAssignmentStatement(lineTokens,lineNum);
                }
                else if (HPFUNC.IsMacroFuntion(lineTokens.First().Text))
                {
                    CreateMacroBuiltInFunctionStatement(lineTokens, lineNum);
                }
              
                lineContent = sourceManager.GetNextLine(out lineNum);
            }
        }

        private void CreateMacroBuiltInFunctionStatement(List<Token> tokens, int line_num)
        {
            CreateOneLineStatement(TaskType.BUILT_IN_FUNCTION, tokens,line_num);
        }

        private void CreateGCodeStatement(List<Token> tokens, int line_num)
        {
            CreateOneLineStatement(TaskType.GCODE, tokens, line_num);
        }

        private void CreateAssignmentStatement(List<Token> tokens, int line_num)
        {
            CreateOneLineStatement(TaskType.ASSIGNMENT, tokens, line_num);
        }


        #region Private refactoring functions

        private static List<Token> ExtractStatementTokens(ICollection<Token> tokens)
        {
            // Skip last token (end statement token ';')
            return tokens.Take(tokens.Count - 1).ToList();
        }

        private void CreateOneLineStatement(TaskType task_type, List<Token> tokens, int line_num)
        {
            validateEndStatement(tokens, line_num);
            var funcTask = new Task(task_type, ExtractStatementTokens(tokens));
            funcTask.SetLineNumber(line_num);
            compiledTasks.Add(funcTask);
        }

        private static void validateEndStatement(List<Token> tokens, int line_num)
        {
            if (tokens.Last().Text != MacroKeywords.END_STATEMENT)
                throw new Exception(string.Format(
                    "Expected '{0}' at line {1}", 
                    MacroKeywords.END_STATEMENT, line_num));
        }

        private static bool isVariableToken(Token tok)
        {
            return tok.Type == TokenType.GLOBAL_VAR
                || tok.Type == TokenType.LOCAL_VAR;
        }
        #endregion
    }
}
