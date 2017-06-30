using System;
using System.Collections.Generic;
using System.Linq;
using HPGCodeValidation;
using HPMacroComponents;
using HPMacroFunctions;
using HPMathExpression;
using HPTypes;
using MacroVariableDB;

namespace HPCompiler
{
    public static class MacroProgram
    {
        private static int LCount;
        private static string NewStatementLabel()
        {
            return string.Format("L{0}", LCount++);
        }

        private static string CreateProgramLabel(string label)
        {
            return "P" + label;
        }

        private static TokenManager SourceTokenManager = new TokenManager(new List<Token>());
        public static void SetSourceTokens(List<Token> tokens)
        {
            SourceTokenManager = new TokenManager(tokens);
            CompilerTasks.Clear();
            ResetExecution();
        }

        private static List<Task>  CompilerTasks = new List<Task>();
        public static void CreateBlock(string blockLabel)
        {
            var nextToken = SourceTokenManager.LookNextToken();
            while (NotSeperateBlockKeyword(nextToken))
            {
                var keyword = nextToken.Text;
                var detectLabel = false;
                if(IsGCommandCode(keyword))
                {
                    CreateGCode();
                }
                else if(HPFUNC.IsVoidMacroFunction(keyword))
                {
                    CreateVoidMacroFunction();
                }
                else
                    switch (keyword)
                    {
                        default:
                            CreateAssignmentOrLabel(out detectLabel);
                            break;
                        case MacroKeywords.IF:
                            CreateIf(blockLabel);
                            break;
                        case MacroKeywords.WHILE:
                            CreateWhile();
                            break;
                        case MacroKeywords.REPEAT:
                            CreateRepeat();
                            break;
                        case MacroKeywords.FOR:
                            CreateFor();
                            break;
                        case MacroKeywords.SWITCH:
                            CreateSwitch();
                            break;
                        case MacroKeywords.GOTO:
                            CreateGoto();
                            break;
                        case MacroKeywords.BREAK:
                            CreateBreak(blockLabel);
                            break;
                    }

                if (!(keyword == MacroKeywords.REPEAT 
                    || keyword == MacroKeywords.GOTO 
                    || detectLabel))
                    SourceTokenManager.Match(";");
                nextToken = SourceTokenManager.LookNextToken();
            }
        }

        private static void CreateVoidMacroFunction()
        {
            throw new NotImplementedException();
        }

        private static void CreateBreak(string blockLabel)
        {
            SourceTokenManager.Match(MacroKeywords.BREAK);
            CompilerTasks.Add(Task.Branch(blockLabel));
        }

        private static void CreateAssignmentOrLabel(out bool detectLabel)
        {
            var nextToken = SourceTokenManager.LookNextToken();
            var nextNextToken = SourceTokenManager.LookNextNextToken();
            if(nextToken.Type == TokenType.IDENTIFIER 
                && nextNextToken.Text == ":")
            {
                SourceTokenManager.Match(nextToken.Text);
                SourceTokenManager.Match(nextNextToken.Text);
                CompilerTasks.Add(Task.PostLabel(CreateProgramLabel(nextToken.Text)));
                detectLabel = true;
            }
            else
            {
                CreateAssignment();
                detectLabel = false;
            }
        }

        private static void CreateGoto()
        {
            SourceTokenManager.Match(MacroKeywords.GOTO);
            var labelToken = SourceTokenManager.GetNextToken();
            if(labelToken.Type != TokenType.IDENTIFIER)
                throw new Exception(string.Format("Invalid Label '{0}'",labelToken.Text));
            CompilerTasks.Add(Task.Branch(CreateProgramLabel(labelToken.Text)));
        }

        private static void CreateSwitch()
        {
            SourceTokenManager.Match(MacroKeywords.SWITCH);
            var switchExpr = MatchParantheseExpression();
            CompilerTasks.Add(Task.ArithmeticExpression(switchExpr));
            var selectLabel = NewStatementLabel();
            CompilerTasks.Add(Task.Branch(selectLabel));

            var keyword = SourceTokenManager.LookNextToken().Text;
            var caseLabelDict = new Dictionary<int, string>();
            var endLabel = NewStatementLabel();
            var defaultLabel = string.Empty;
            while (!(keyword == MacroKeywords.ENDSWITCH
                || keyword == MacroKeywords.END))
            {
                switch (keyword)
                {
                    case MacroKeywords.CASE:
                        SourceTokenManager.Match(keyword);
                        var caseLabel = NewStatementLabel();
                        var nextStr = SourceTokenManager.LookNextToken().Text;
                        while (!(nextStr == ":" || nextStr == MacroKeywords.END))
                        {
                            var caseEval = MathExpression.Create(SourceTokenManager.GetNextToken().Text).Evaluate();
                            var caseValue = int.Parse(caseEval.Literal);
                            if (caseLabelDict.ContainsKey(caseValue))
                                throw new Exception(string.Format("Duplicate Case '{0}'", caseValue));
                            caseLabelDict.Add(caseValue, caseLabel);
                           
                            if(SourceTokenManager.LookNextToken().Text ==",")
                                SourceTokenManager.Match(",");
                            nextStr = SourceTokenManager.LookNextToken().Text;
                        }

                        SourceTokenManager.Match(":");
                        CompilerTasks.Add(Task.PostLabel(caseLabel));
                        CreateBlock(endLabel);
                        CompilerTasks.Add(Task.Branch(endLabel));
                        break;

                    case MacroKeywords.DEFAULT:
                        defaultLabel = NewStatementLabel();
                        SourceTokenManager.Match(keyword);
                        SourceTokenManager.Match(":");
                        CompilerTasks.Add(Task.PostLabel(defaultLabel));
                        CreateBlock(endLabel);
                        CompilerTasks.Add(Task.Branch(endLabel));
                        break;

                }
                keyword = SourceTokenManager.LookNextToken().Text;
            }

            CompilerTasks.Add(Task.PostLabel(selectLabel));
            foreach (var c in caseLabelDict)
                CompilerTasks.Add(Task.BranchIfEqual(c.Key, c.Value));

            CompilerTasks.Add(Task.PostLabel(endLabel));
            SourceTokenManager.Match(MacroKeywords.ENDSWITCH);
        }

        private static void CreateFor()
        {
            SourceTokenManager.Match(MacroKeywords.FOR);
            // Get index name, assign initial value
            var name = SourceTokenManager.LookNextToken().Text;
            CreateAssignment();

            SourceTokenManager.Match(MacroKeywords.TO);
            // Get upper limit (to)
            var toEval = MatchExpression();
            // Get increment step (by)
            var byEval = new List<Token> {new Token("1", TokenType.NUMBER)};
            if(SourceTokenManager.LookNextToken().Text == MacroKeywords.BY)
            {
                SourceTokenManager.Match(MacroKeywords.BY);
                byEval = MatchExpression();
            }

            // label1            
            var label1 = NewStatementLabel();
            CompilerTasks.Add(Task.PostLabel(label1));

            // Evaluate upper limit
            CompilerTasks.Add(Task.ArithmeticExpression(toEval));
            var label2 = NewStatementLabel();

            // Jump to label2 if index variable is greater than upper limit
            CompilerTasks.Add(Task.BranchIfGreater(name, label2));

            // Inside block
            CreateBlock(label2);

            // Update index variable
            CompilerTasks.Add(Task.IncreaseVariable(name, byEval));

            // Jump to label1
            CompilerTasks.Add(Task.Branch(label1));

            // label2
            CompilerTasks.Add(Task.PostLabel(label2));

            SourceTokenManager.Match(MacroKeywords.ENDFOR);
        }

        private static void CreateRepeat()
        {
            SourceTokenManager.Match(MacroKeywords.REPEAT);
            var label1 = NewStatementLabel();
            var label2 = NewStatementLabel();
            CompilerTasks.Add(Task.PostLabel(label1));
            CreateBlock(label2);
            SourceTokenManager.Match(MacroKeywords.UNTIL);
            CompilerTasks.Add(Task.BoolCondition(MatchParantheseExpression()));
            CompilerTasks.Add(Task.BranchIfFalse(label1));
            CompilerTasks.Add(Task.PostLabel(label2));
        }

        private static void CreateWhile()
        {
            SourceTokenManager.Match(MacroKeywords.WHILE);
            var label1 = NewStatementLabel();
            var label2 = NewStatementLabel();
            CompilerTasks.Add(Task.PostLabel(label1));
            CompilerTasks.Add(Task.BoolCondition(MatchParantheseExpression()));
            CompilerTasks.Add(Task.BranchIfFalse(label2));
            CreateBlock(label2);
            SourceTokenManager.Match(MacroKeywords.ENDWHILE);
            CompilerTasks.Add(Task.Branch(label1));
            CompilerTasks.Add(Task.PostLabel(label2));
        }

        private static void CreateIf(string blockLabel)
        {
            SourceTokenManager.Match(MacroKeywords.IF);
            CompilerTasks.Add(Task.BoolCondition(MatchParantheseExpression()));

            var label1 = NewStatementLabel();
            CompilerTasks.Add(Task.BranchIfFalse(label1));
            CreateBlock(blockLabel);

            var label2 = label1;
            if(SourceTokenManager.LookNextToken().Text == MacroKeywords.ELSE)
            {
                SourceTokenManager.Match(MacroKeywords.ELSE);
                label2 = NewStatementLabel();
                CompilerTasks.Add(Task.Branch(label2));
                CompilerTasks.Add(Task.PostLabel(label1));
                CreateBlock(blockLabel);
            }
            SourceTokenManager.Match(MacroKeywords.ENDIF);
            CompilerTasks.Add(Task.PostLabel(label2));
        }

        private static List<Token> MatchExpression()
        {
            var nextToken = SourceTokenManager.LookNextToken();
            if (nextToken.Text == "(")
                return MatchParantheseExpression();
            
            return new List<Token>{SourceTokenManager.GetNextToken()};
        } 

        private static List<Token> MatchParantheseExpression()
        {
            SourceTokenManager.Match("(");

            var nestedParanCount = 1;
            var conditionTokens = new List<Token>();
            var keyword = SourceTokenManager.LookNextToken().Text;
            while (!((keyword == ")" && nestedParanCount == 1)
                || keyword == MacroKeywords.END))
            {
                switch (keyword)
                {
                    case "(":
                        nestedParanCount++;
                        break;
                    case ")":
                        nestedParanCount--;
                        break;
                }
                conditionTokens.Add(SourceTokenManager.GetNextToken());
                keyword = SourceTokenManager.LookNextToken().Text;
            }

            SourceTokenManager.Match(")");
            nestedParanCount--;
            if (nestedParanCount != 0)
                throw new Exception("Unbalanced Paranthesis");
            return conditionTokens;
        }

        private static void CreateGCode()
        {
            var gCodeTokens = new List<Token>();
            var keyword = SourceTokenManager.LookNextToken().Text;
            while (!(keyword == MacroKeywords.END
                || keyword == MacroKeywords.END_STATEMENT))   // <GCode> ';'
            {
                gCodeTokens.Add(SourceTokenManager.GetNextToken());
                keyword = SourceTokenManager.LookNextToken().Text;
            }
            CompilerTasks.Add(new Task(ExecuteTask.GCODE, gCodeTokens));
        }

        private static void CreateAssignment()
        {
            var assignmentTokens = new List<Token>();
            var keyword = SourceTokenManager.LookNextToken().Text;
            while (!(keyword == MacroKeywords.END
                || keyword == MacroKeywords.END_STATEMENT   // <assignment> ';'
                || keyword == MacroKeywords.TO))            // FOR <assignment> TO ...
            {
                assignmentTokens.Add(SourceTokenManager.GetNextToken());
                keyword = SourceTokenManager.LookNextToken().Text;
            }
            CompilerTasks.Add(new Task(ExecuteTask.ASSIGNMENT, assignmentTokens));
        }

        private static bool IsGCommandCode(string keyword)
        {
            try
            {
                return GCodeValidate.ValidateGCodeCommand(keyword);
            }
            catch(Exception)
            {
                return false;
            }
        }

        private static bool NotSeperateBlockKeyword(Token nextToken)
        {
            return !(nextToken.Text == MacroKeywords.END
                     || nextToken.Text == MacroKeywords.ELSE
                     || nextToken.Text == MacroKeywords.ENDIF
                     || nextToken.Text == MacroKeywords.ENDWHILE
                     || nextToken.Text == MacroKeywords.UNTIL
                     || nextToken.Text == MacroKeywords.ENDFOR
                     || nextToken.Text == MacroKeywords.CASE
                     || nextToken.Text == MacroKeywords.DEFAULT
                     || nextToken.Text == MacroKeywords.ENDSWITCH);
        }

        public static void Execute()
        {
            var condition = false;
            HPType arithmeticEval = null;
            var index = 0;
            while(index < CompilerTasks.Count)
            {
                var task = CompilerTasks[index++];
                switch (task.Type)
                {
                    case ExecuteTask.ASSIGNMENT:
                        new Assignment(task.Tokens).Execute();
                        break;

                    case ExecuteTask.GCODE:
                        new GCodeStatement(task.Tokens).Execute();
                        break;

                    case ExecuteTask.BRANCH:
                        index = GetIndexOfLabel(task);
                        break;

                    case ExecuteTask.BOOLEAN_EVALUATE:
                        var boolEval = MathExpression.Create(task.Tokens).Evaluate();
                        if(boolEval.Type != VariableType.BOOL)
                        {
                            var bExpr = task.Tokens.Aggregate(string.Empty, (current, t) => current + (t.Text + " "));
                            throw new Exception(string.Format("Invalid Boolean Expression '{0}'",bExpr));
                        }
                        condition = bool.Parse(boolEval.Literal);
                        break;

                    case ExecuteTask.BRANCH_TRUE:
                        if (condition)
                            index = GetIndexOfLabel(task);
                        break;

                    case ExecuteTask.BRANCH_FALSE:
                        if (!condition)
                            index = GetIndexOfLabel(task);
                        break;

                    case ExecuteTask.ARITHMETIC_EVALUATE:
                        arithmeticEval = MathExpression.Create(task.Tokens).Evaluate();
                        break;

                    case ExecuteTask.BRANCH_GREATER:
                        var indexName = task.Tokens[0].Text;
                        var indexValue = VariableDB.LoadVariable(indexName);
                        if(arithmeticEval == null)
                            throw new Exception("Execution Error: BRANCH_GREATER. Null Arithmetic Expression");

                        switch (indexValue.Type)
                        {
                            case VariableType.FLOAT:
                                if (float.Parse(indexValue.Literal) > float.Parse(arithmeticEval.Literal))
                                    index = GetIndexOfLabel(task);
                                break;
                            case VariableType.INT:
                                if (int.Parse(indexValue.Literal) > int.Parse(arithmeticEval.Literal))
                                    index = GetIndexOfLabel(task);
                                break;
                            default:
                                throw new Exception(string.Format("Execution Error: BRANCH_GREATER."));
                        }
                        break;
                        
                    case ExecuteTask.BRANCH_EQUAL:
                        var caseValue = MathExpression.Create(task.Tokens).Evaluate();
                        if (arithmeticEval == null)
                            throw new Exception("Execution Error: BRANCH_EQUAL. Null Arithmetic Expression");
                        switch (caseValue.Type)
                        {
                            case VariableType.FLOAT:
                                if (float.Parse(caseValue.Literal) == float.Parse(arithmeticEval.Literal))
                                    index = GetIndexOfLabel(task);
                                break;
                            case VariableType.INT:
                                if (int.Parse(caseValue.Literal) == int.Parse(arithmeticEval.Literal))
                                    index = GetIndexOfLabel(task);
                                break;
                            default:
                                if (bool.Parse(caseValue.Literal) == bool.Parse(arithmeticEval.Literal))
                                    index = GetIndexOfLabel(task);
                                break;
                        }
                        break;
                    //case ExecuteTask.LABEL:
                    //default:
                    //    // Label for Jump, Do nothing
                    //    break;
                }
            }
        }

        private static int GetIndexOfLabel(Task task)
        {
            return CompilerTasks.FindIndex(
                t => (t.Type == ExecuteTask.LABEL && t.Label == task.Label));
        }

        private static int TaskCurrentIndex;
        public static void ResetExecution()
        {
            TaskCurrentIndex = 0;
        }

        public static void Step()
        {
            var condition = false;
            HPType arithmeticEval = null;
            var stepDone = false;
            while (TaskCurrentIndex < CompilerTasks.Count)
            {
                if (stepDone) break;
                var task = CompilerTasks[TaskCurrentIndex++];
                switch (task.Type)
                {
                    case ExecuteTask.ASSIGNMENT:
                        new Assignment(task.Tokens).Execute();
                        stepDone = true;
                        break;

                    case ExecuteTask.GCODE:
                        new GCodeStatement(task.Tokens).Execute();
                        stepDone = true;
                        break;

                    case ExecuteTask.BRANCH:
                        TaskCurrentIndex = GetIndexOfLabel(task);
                        break;

                    case ExecuteTask.BOOLEAN_EVALUATE:
                        var boolEval = MathExpression.Create(task.Tokens).Evaluate();
                        if (boolEval.Type != VariableType.BOOL)
                        {
                            var bExpr = task.Tokens.Aggregate(string.Empty, (current, t) => current + (t.Text + " "));
                            throw new Exception(string.Format("Invalid Boolean Expression '{0}'", bExpr));
                        }
                        condition = bool.Parse(boolEval.Literal);
                        break;

                    case ExecuteTask.BRANCH_TRUE:
                        if (condition)
                            TaskCurrentIndex = GetIndexOfLabel(task);
                        break;

                    case ExecuteTask.BRANCH_FALSE:
                        if (!condition)
                            TaskCurrentIndex = GetIndexOfLabel(task);
                        break;

                    case ExecuteTask.ARITHMETIC_EVALUATE:
                        arithmeticEval = MathExpression.Create(task.Tokens).Evaluate();
                        break;

                    case ExecuteTask.BRANCH_GREATER:
                        var indexName = task.Tokens[0].Text;
                        var indexValue = VariableDB.LoadVariable(indexName);
                        if (arithmeticEval == null)
                            throw new Exception("Execution Error: BRANCH_GREATER. Null Arithmetic Expression");

                        switch (indexValue.Type)
                        {
                            case VariableType.FLOAT:
                                if (float.Parse(indexValue.Literal) > float.Parse(arithmeticEval.Literal))
                                    TaskCurrentIndex = GetIndexOfLabel(task);
                                break;
                            case VariableType.INT:
                                if (int.Parse(indexValue.Literal) > int.Parse(arithmeticEval.Literal))
                                    TaskCurrentIndex = GetIndexOfLabel(task);
                                break;
                            default:
                                throw new Exception(string.Format("Execution Error: BRANCH_GREATER."));
                        }
                        break;

                    case ExecuteTask.BRANCH_EQUAL:
                        var caseValue = MathExpression.Create(task.Tokens).Evaluate();
                        if (arithmeticEval == null)
                            throw new Exception("Execution Error: BRANCH_EQUAL. Null Arithmetic Expression");
                        switch (caseValue.Type)
                        {
                            case VariableType.FLOAT:
                                if (float.Parse(caseValue.Literal) == float.Parse(arithmeticEval.Literal))
                                    TaskCurrentIndex = GetIndexOfLabel(task);
                                break;
                            case VariableType.INT:
                                if (int.Parse(caseValue.Literal) == int.Parse(arithmeticEval.Literal))
                                    TaskCurrentIndex = GetIndexOfLabel(task);
                                break;
                            default:
                                if (bool.Parse(caseValue.Literal) == bool.Parse(arithmeticEval.Literal))
                                    TaskCurrentIndex = GetIndexOfLabel(task);
                                break;
                        }
                        break;
                    //case ExecuteTask.LABEL:
                    //default:
                    //    // Label for Jump, Do nothing
                    //    break;
                }
            }

            if (TaskCurrentIndex < CompilerTasks.Count) return;
            ResetExecution();
            throw new Exception("End Execution. Return.");
        }

        public static void Compile()
        {
            CreateBlock(string.Empty);
        }
    }
}