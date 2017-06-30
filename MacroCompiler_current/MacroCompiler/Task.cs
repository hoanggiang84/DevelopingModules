using System.Collections.Generic;
using System.Globalization;
using HPMacroComponents;
using MacroVariableDB;

namespace HPCompiler
{
    internal class Task
    {
        public readonly ExecuteTask Type;
        public readonly List<Token> Tokens;
        public readonly string Label;

        public Task(ExecuteTask taskType, List<Token> tokens)
        {
            Type = taskType;
            Tokens = tokens;
        }

        public Task(ExecuteTask taskType, string label)
        {
            Type = taskType;
            Label = label;
        }

        public Task(ExecuteTask taskType, string name, string label)
        {
            Type = taskType;
            Label = label;
            var identToken = new Token(name, TokenType.IDENTIFIER);
            Tokens = new List<Token> {identToken};
        }

        public Task(ExecuteTask taskType, int caseValue, string label)
        {
            Type = taskType;
            Label = label;
            var identToken = new Token(caseValue.ToString(CultureInfo.InvariantCulture), TokenType.NUMBER);
            Tokens = new List<Token> { identToken };
        }

        public static Task BoolCondition(List<Token> tokens)
        {
            return new Task(ExecuteTask.BOOLEAN_EVALUATE, tokens);
        }

        public static Task ArithmeticExpression(List<Token> tokens)
        {
            return new Task(ExecuteTask.ARITHMETIC_EVALUATE, tokens);
        }

        public static Task BranchIfFalse(string label)
        {
            return new Task(ExecuteTask.BRANCH_FALSE, label);
        }

        public static Task BranchIfTrue(string label)
        {
            return new Task(ExecuteTask.BRANCH_TRUE, label);
        }

        public static Task Branch(string label)
        {
            return new Task(ExecuteTask.BRANCH, label);
        }

        public static Task PostLabel(string label)
        {
            return new Task(ExecuteTask.LABEL, label);
        }

        public static Task BranchIfGreater(string name, string label1)
        {
            return new Task(ExecuteTask.BRANCH_GREATER, name, label1);
        }

        public static Task BranchIfEqual(int caseValue, string label1)
        {
            return new Task(ExecuteTask.BRANCH_EQUAL, caseValue, label1);
        }
        /// <summary>
        /// name = name + byEval
        /// </summary>
        /// <param name="name"></param>
        /// <param name="byEval"></param>
        /// <returns></returns>
        public static Task IncreaseVariable(string name, List<Token> byEval)
        {
            var tokenType = TokenType.LOCAL_VAR;
            if(VariableDB.IsGlobal(name))
                tokenType = TokenType.GLOBAL_VAR;

            var assignmentTokens = new List<Token>
                                       {
                                           new Token(name, tokenType),
                                           new Token("=",TokenType.SYMBOL),
                                           new Token(name, tokenType),
                                           new Token("+",TokenType.SYMBOL)
                                       };
            assignmentTokens.AddRange(byEval);
            return new Task(ExecuteTask.ASSIGNMENT, assignmentTokens);
        }
    }
}