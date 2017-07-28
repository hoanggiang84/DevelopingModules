using System;
using System.Linq;
using HPMacroTask;
using HPMathExpression;
using HPTypes;
using HPVariableRepository;
using System.Collections.Generic;

namespace MacroPLC
{
    public class MacroExecutor
    {
        private List<Task> compiledTasks;
        private const int INVALID_LINE_NUMBER = -1;

        public VariableRepository Variables { get; set; }

        public MacroExecutor(List<Task> compiledTasks)
        {
            this.compiledTasks = compiledTasks;
            Variables = new VariableRepository();
            Variables.InitializeVariables();
            MathExpression.VarDB = Variables;
        }

        public void Execute()
        {
            ResetExecute();
            var lineIndex = 0;
            while (lineIndex != INVALID_LINE_NUMBER)
            {
                lineIndex = StepExecute();
            }
        }

        private void ResetExecute()
        {
            taskIndex = 0;
            Variables.InitializeVariables();
        }

        private int taskIndex;
        private HPType pre_condition;
        private HPType pre_value;
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
                    case TaskType.GCODE:
                    case TaskType.BUILT_IN_FUNCTION:
                        MacroStatement.CreateStatement(curTask.Type, curTask.Tokens, Variables).Execute();
                        return lineNumber;

                    case TaskType.LABEL:
                        return lineNumber;

                    case TaskType.BOOLEAN_EVALUATE:
                        var boolEval = MathExpression.Create(curTask.Tokens).Evaluate();
                        if (boolEval.Type != VariableType.BOOL)
                        {
                            var bExpr = curTask.Tokens.Aggregate(string.Empty, (current, t) => current + (t.Text + " "));
                            throw new Exception(string.Format("Invalid Boolean Expression '{0}'", bExpr));
                        }
                        pre_condition = boolEval;
                        break;

                    case TaskType.ARITHMETIC_EVALUATE:
                        pre_value = MathExpression.Create(curTask.Tokens).Evaluate();
                        return lineNumber;

                    case TaskType.BRANCH_FALSE:
                        var condition = bool.Parse(pre_condition.Literal);
                        if (condition == false)
                        {
                            taskIndex = GetIndexOfLabel(curTask);
                            if (taskIndex < 0)
                                throw new Exception("Execution error: BRANCH FALSE. Cannot find label ");
                        }
                        break;

                    case TaskType.BRANCH:
                        taskIndex = GetIndexOfLabel(curTask);
                        break;

                    case TaskType.BRANCH_TRUE:
                        var true_condition = bool.Parse(pre_condition.Literal);
                        if (true_condition)
                        {
                            taskIndex = GetIndexOfLabel(curTask);
                            if (taskIndex < 0)
                                throw new Exception("Execution error: BRANCH TRUE. Cannot find label ");
                        }
                        break;

                    case TaskType.BRANCH_EQUAL:
                        var caseValue = MathExpression.Create(curTask.Tokens).Evaluate();

                        if (pre_value == null)
                            throw new Exception("Execution Error: BRANCH_EQUAL. Null Arithmetic Expression");

                        switch (caseValue.Type)
                        {
                            case VariableType.FLOAT:
                                if (float.Parse(caseValue.Literal) == float.Parse(pre_value.Literal))
                                    taskIndex = GetIndexOfLabel(curTask);
                                break;
                            case VariableType.INT:
                                if (int.Parse(caseValue.Literal) == int.Parse(pre_value.Literal))
                                    taskIndex = GetIndexOfLabel(curTask);
                                break;
                            default:
                                if (bool.Parse(caseValue.Literal) == bool.Parse(pre_value.Literal))
                                    taskIndex = GetIndexOfLabel(curTask);
                                break;
                        }

                        break;

                    case TaskType.BRANCH_GREATER:
                        var indexName = curTask.Tokens[0].Text;
                        var indexValue = Variables.LoadVariable(indexName);
                        if (pre_value == null)
                            throw new Exception("Execution Error: BRANCH_GREATER. Null Arithmetic Expression");

                        switch (indexValue.Type)
                        {
                            case VariableType.FLOAT:
                                if (float.Parse(indexValue.Literal) > float.Parse(pre_value.Literal))
                                    taskIndex = GetIndexOfLabel(curTask);
                                break;
                            case VariableType.INT:
                                if (int.Parse(indexValue.Literal) > int.Parse(pre_value.Literal))
                                    taskIndex = GetIndexOfLabel(curTask);
                                break;
                            default:
                                throw new Exception(string.Format("Execution Error: BRANCH_GREATER."));
                        }
                        break;








                }
            }
            return INVALID_LINE_NUMBER;
        }

        private int GetIndexOfLabel(Task task)
        {
            return compiledTasks.FindIndex(
                t => (t.Type == TaskType.LABEL && t.Label == task.Label));
        }
    }
}