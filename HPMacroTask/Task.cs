using System.Collections.Generic;
using System.Globalization;
using MacroLexScn;

namespace HPMacroTask
{
    public class Task
    {
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
    }
}