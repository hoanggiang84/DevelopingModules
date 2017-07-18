using System.Collections.Generic;
using HPMacroTask;
using MacroPLC;
using NUnit.Framework;

namespace MacroPLCTest
{
    public class MacroCompilerTest:Specification
    {
        protected List<Task> tasks;

        private void compileToTaskList(string source)
        {
            var macro_compiler = new MacroCompiler(source);
            macro_compiler.Compile();
            tasks = macro_compiler.compiledTasks;
        }

        private void AssertTasksCount(int count)
        {
            Assert.AreEqual(count, tasks.Count);
        }

        private void AssertTaskType(int taskIndex, TaskType assertTask)
        {
            Assert.AreEqual(assertTask, tasks[taskIndex].Type);
        }

        [Test]
        public void Compile_Assigment_returnAsignmentTask()
        {
            compileToTaskList("@10 = 12.1;");
            AssertTasksCount(1);
            AssertTaskType(0, TaskType.ASSIGNMENT);
        }

        [Test]
        public void Compile_GCode_returnGCodeTask()
        {
            compileToTaskList("G01 X1 Y10 Z14 F11;");
            AssertTasksCount(1);
            AssertTaskType(0,TaskType.GCODE);
        }

        [Test]
        public void Compile_MacroFunction_returnFunctionTask()
        {
            compileToTaskList("WAIT();");
            AssertTasksCount(1);
            AssertTaskType(0, TaskType.BUILT_IN_FUNCTION);
        }

        [Test]
        public void Compile_ManySimpleStatements_returnTaskList()
        {
            compileToTaskList(
                "@10 = 12.1;\r\n" +
                "G01 X1 Y10 Z14 F11; \r\n"+
                "WAIT();");
            AssertTasksCount(3);
            AssertTaskType(0, TaskType.ASSIGNMENT);
            AssertTaskType(1, TaskType.GCODE);
            AssertTaskType(2, TaskType.BUILT_IN_FUNCTION);
        }

        [Test]
        public void Compile_IfStatement_returnTaskList()
        {
            var src_code = "IF (2<4) \r\n" +
                            "WAIT(); \r\n" +
                            "ENDIF; ";
            compileToTaskList(src_code);
            AssertTaskType(0, TaskType.BOOLEAN_EVALUATE);
            AssertTaskType(1, TaskType.BRANCH_FALSE);
            AssertTaskType(2, TaskType.BUILT_IN_FUNCTION);
            AssertTaskType(3, TaskType.LABEL);
        }

        [Test]
        public void Compile_IfStatementWithManyStatemets_returnTaskList()
        {
            var src_code = "IF (2<4) \r\n" +
                            "WAIT(); \r\n" +
                            "@1 = 1; \r\n" +
                            "ENDIF; ";
            compileToTaskList(src_code);
            AssertTaskType(0, TaskType.BOOLEAN_EVALUATE);
            AssertTaskType(1, TaskType.BRANCH_FALSE);
            AssertTaskType(2, TaskType.BUILT_IN_FUNCTION);
            AssertTaskType(3, TaskType.ASSIGNMENT);
            AssertTaskType(4, TaskType.LABEL);
        }

        [Test]
        public void Compile_IfElseStatement_returnTaskList()
        {
            var src_code = "IF (2>4) \r\n" +
                            "WAIT(); \r\n" +
                            "ELSE \r\n"    +
                            "@1 = 1; \r\n" +
                            "ENDIF; ";
            compileToTaskList(src_code);
            var cnt = 0;
            AssertTaskType(cnt++, TaskType.BOOLEAN_EVALUATE);
            AssertTaskType(cnt++, TaskType.BRANCH_FALSE);
            AssertTaskType(cnt++, TaskType.BUILT_IN_FUNCTION);
            AssertTaskType(cnt++, TaskType.BRANCH);
            AssertTaskType(cnt++, TaskType.LABEL);
            AssertTaskType(cnt++, TaskType.ASSIGNMENT);
            AssertTaskType(cnt++, TaskType.LABEL);
        }
    }
}