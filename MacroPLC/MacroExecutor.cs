using HPMacroTask;
using HPVariableRepository;
using System.Collections.Generic;

namespace MacroPLC
{
    public class MacroExecutor
    {
        public VariableRepository Variables = new VariableRepository();
        private List<Task> compiledTasks;

        public MacroExecutor(List<Task> compiledTasks)
        {
            this.compiledTasks = compiledTasks;
            Variables.InitializeVariables();
        }

        public void Execute()
        {
            
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
                        new Assignment(curTask.Tokens, Variables).Execute();
                        return lineNumber;
                }
            }
            return -1;
        }
    }
}