using System;
using System.Collections.Generic;
using System.Globalization;
using MacroLexScn;

namespace HPMacroTask
{
    public class Task
    {
        #region Single line statements
        private static Task createSingleLineStatement(TaskType type, List<Token> tokens, int line_num)
        {
            if(type != TaskType.ASSIGNMENT
                && type != TaskType.GCODE
                && type != TaskType.BUILT_IN_FUNCTION
                && type != TaskType.BOOLEAN_EVALUATE)
                throw new Exception(string.Format("Not single line statement type '{0}'", type));

            var task = new Task(type, tokens);
            task.SetLineNumber(line_num);
            return task;
        }

        public static Task CreateAssignmentTask(List<Token> tokens, int line_num)
        {
            return createSingleLineStatement(TaskType.ASSIGNMENT, tokens, line_num);
        }

        public static Task CreateGCodeTask(List<Token> tokens, int line_num)
        {
            return createSingleLineStatement(TaskType.GCODE, tokens, line_num);
        }

        public static Task CreateBuiltInFunctionTask(List<Token> tokens, int line_num)
        {
            return createSingleLineStatement(TaskType.BUILT_IN_FUNCTION, tokens, line_num);
        }

        public static Task BoolCondition(List<Token> tokens, int line_num)
        {
            return createSingleLineStatement(TaskType.BOOLEAN_EVALUATE, tokens, line_num);
        }

        #endregion

        #region Branch label

        private static Task CreateBranchLabel(TaskType type, string label, int line_num)
        {
            if (type != TaskType.LABEL
                && type != TaskType.BRANCH_TRUE
                && type != TaskType.BRANCH_FALSE
                && type != TaskType.BRANCH
                && type != TaskType.BRANCH_GREATER)
                throw new Exception(string.Format("Not label branch type '{0}'", type));
            var task = new Task(type, label);
            task.SetLineNumber(line_num);
            return task;
        }

        public static Task BranchIfFalse(string label, int line_num)
        {
            return CreateBranchLabel(TaskType.BRANCH_FALSE, label, line_num);
        }

        public static Task BranchIfTrue(string label, int line_num)
        {
            return CreateBranchLabel(TaskType.BRANCH_TRUE, label, line_num);
        }

        public static Task Branch(string label, int line_num)
        {
            return CreateBranchLabel(TaskType.BRANCH, label, line_num);
        }

        public static Task PostLabel(string label, int line_num)
        {
            return CreateBranchLabel(TaskType.LABEL, label, line_num);
        }

        public static Task BranchIfGreater(string name, string label, int line_num)
        {
            return CreateBranchLabel(TaskType.BRANCH_GREATER, label, line_num);
        }

        public static Task BranchIfEqual(int caseValue, string label, int line_num)
        {
            var task = new Task(TaskType.BRANCH_EQUAL, caseValue, label);
            task.SetLineNumber(line_num);
            return task;
        }
        #endregion

        #region Origin
        public readonly TaskType Type;
        public readonly List<Token> Tokens;
        public readonly string Label;
        public int LineNumber
        {
            get; private set; 
        }

        public void SetLineNumber(int num)
        {
            LineNumber = num;
        }

        public Task(TaskType taskType, List<Token> tokens)
        {
            Type = taskType;
            Tokens = tokens;
        }

        public Task(TaskType taskType, string label)
        {
            Type = taskType;
            Label = label;
        }

        public Task(TaskType taskType, string name, string label)
        {
            Type = taskType;
            Label = label;
            var identToken = new Token(name, TokenType.IDENTIFIER);
            Tokens = new List<Token> {identToken};
        }

        public Task(TaskType taskType, int caseValue, string label)
        {
            Type = taskType;
            Label = label;
            var identToken = new Token(caseValue.ToString(CultureInfo.InvariantCulture), TokenType.NUMBER);
            Tokens = new List<Token> { identToken };
        }

        public static Task BoolCondition(List<Token> tokens)
        {
            return new Task(TaskType.BOOLEAN_EVALUATE, tokens);
        }

        public static Task ArithmeticExpression(List<Token> tokens)
        {
            return new Task(TaskType.ARITHMETIC_EVALUATE, tokens);
        }

        public static Task BranchIfFalse(string label)
        {
            return new Task(TaskType.BRANCH_FALSE, label);
        }

        public static Task BranchIfTrue(string label)
        {
            return new Task(TaskType.BRANCH_TRUE, label);
        }

        public static Task Branch(string label)
        {
            return new Task(TaskType.BRANCH, label);
        }

        public static Task PostLabel(string label)
        {
            return new Task(TaskType.LABEL, label);
        }

        public static Task BranchIfGreater(string name, string label1)
        {
            return new Task(TaskType.BRANCH_GREATER, name, label1);
        }

        public static Task BranchIfEqual(int caseValue, string label1)
        {
            return new Task(TaskType.BRANCH_EQUAL, caseValue, label1);
        }

        /// <summary>
        /// Variable += byEval
        /// </summary>
        public static Task IncreaseVariable(Token variable, List<Token> byEval)
        {
            var assignmentTokens = new List<Token>
                                       {
                                           new Token(variable),
                                           new Token("=",TokenType.SYMBOL),
                                           new Token(variable),
                                           new Token("+",TokenType.SYMBOL)
                                       };
            assignmentTokens.AddRange(byEval);
            return new Task(TaskType.ASSIGNMENT, assignmentTokens);
        }
        #endregion
    }
}