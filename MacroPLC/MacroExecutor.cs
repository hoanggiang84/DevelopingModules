using HPMacroTask;
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
                }
            }
            return INVALID_LINE_NUMBER;
        }
    }
}