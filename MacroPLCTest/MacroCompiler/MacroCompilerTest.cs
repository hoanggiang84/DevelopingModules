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

        private void AssertLineNumber(int taskIndex, int lineNum)
        {
            Assert.AreEqual(lineNum, tasks[taskIndex].LineNumber);
        }

        private void AssertString(int index, string expect)
        {
            Assert.AreEqual(expect, tasks[index].Label);
        }

        [Test]
        public void Compile_assigment_returnAsignmentTask()
        {
            compileToTaskList("@10 = 12.1;");
            AssertTasksCount(2);
            AssertTaskType(0, TaskType.ASSIGNMENT);
            AssertTaskType(1, TaskType.LABEL);
        }

        [Test]
        public void Compile_gcode_returnGCodeTask()
        {
            compileToTaskList("G01 X1 Y10 Z14 F11;");
            AssertTasksCount(2);
            AssertTaskType(0,TaskType.GCODE);
        }

        [Test]
        public void Compile_macroFunction_returnFunctionTask()
        {
            compileToTaskList("WAIT();");
            AssertTasksCount(2);
            AssertTaskType(0, TaskType.BUILT_IN_FUNCTION);
        }

        [Test]
        public void Compile_manySimpleStatements_returnTaskList()
        {
            compileToTaskList(
                "@10 = 12.1;\r\n" +
                "G01 X1 Y10 Z14 F11; \r\n"+
                "WAIT();");
            AssertTasksCount(4);
            AssertTaskType(0, TaskType.ASSIGNMENT);
            AssertTaskType(1, TaskType.GCODE);
            AssertTaskType(2, TaskType.BUILT_IN_FUNCTION);
        }

        [Test]
        public void Compile_if_returnTaskList()
        {
            var src_code =  "IF (2<4) \r\n" +
                            "   \tWAIT(); \r\n" +
                            "ENDIF; ";
            compileToTaskList(src_code);
            AssertTaskType(0, TaskType.BOOLEAN_EVALUATE);
            AssertLineNumber(0, 0);
            AssertTaskType(1, TaskType.BRANCH_FALSE);
            AssertLineNumber(1, 0);
            AssertString(1,"L0");
            AssertTaskType(2, TaskType.BUILT_IN_FUNCTION);
            AssertLineNumber(2, 1);
            AssertTaskType(3, TaskType.LABEL);
            AssertLineNumber(3, 2);
            AssertString(3, "L0");
        }

        [Test]
        public void Compile_ifWithManyStatemets_returnTaskList()
        {
            var src_code =  "IF (2<4) \r\n" +
                            "   \tWAIT(); \r\n" +
                            "   \t@1 = 1; \r\n" +
                            "ENDIF; ";
            compileToTaskList(src_code);
            AssertTaskType(0, TaskType.BOOLEAN_EVALUATE);
            AssertLineNumber(0, 0);
            AssertTaskType(1, TaskType.BRANCH_FALSE);
            AssertLineNumber(1, 0);
            AssertString(1,"L0");
            AssertTaskType(2, TaskType.BUILT_IN_FUNCTION);
            AssertLineNumber(2, 1);
            AssertTaskType(3, TaskType.ASSIGNMENT);
            AssertLineNumber(3, 2);
            AssertTaskType(4, TaskType.LABEL);
            AssertLineNumber(4, 3);
            AssertString(4, "L0");
        }

        [Test]
        public void Compile_ifElseStatement_returnTaskList()
        {
            var src_code = "IF (2>4) \r\n" +
                            "   \tWAIT(); \r\n" +
                            "ELSE \r\n"    +
                            "   \t@1 = 1; \r\n" +
                            "ENDIF; ";
            compileToTaskList(src_code);
            AssertTaskType(0, TaskType.BOOLEAN_EVALUATE);
            AssertLineNumber(0, 0);
            AssertTaskType(1, TaskType.BRANCH_FALSE);
            AssertString(1, "L0");
            AssertLineNumber(1, 0);
            AssertTaskType(2, TaskType.BUILT_IN_FUNCTION);
            AssertLineNumber(2, 1);
            AssertTaskType(3, TaskType.BRANCH);
            AssertString(3, "L1");
            AssertTaskType(4, TaskType.LABEL);
            AssertString(4, "L0");
            AssertLineNumber(4, 2);
            AssertTaskType(5, TaskType.ASSIGNMENT);
            AssertLineNumber(5, 3);
            AssertTaskType(6, TaskType.LABEL);
            AssertString(6, "L1");
            AssertLineNumber(6, 4);
        }

        [Test]
        public void Compile_ifElseIfStatement_returnTaskList()
        {
            var src_code = "IF (2>4) \r\n" +
                            "   \tWAIT(); \r\n" +
                            "ELSEIF (1>10) \r\n" +
                            "   \t@1 = 2; \r\n" +
                            "ELSE \r\n" +
                            "   \t@1 = 1; \r\n" +
                            "ENDIF; ";
            compileToTaskList(src_code);
            AssertTaskType(0, TaskType.BOOLEAN_EVALUATE);
            AssertLineNumber(0, 0);
            AssertTaskType(1, TaskType.BRANCH_FALSE);
            AssertString(1, "L0");
            AssertLineNumber(1, 0);
            AssertTaskType(2, TaskType.BUILT_IN_FUNCTION);
            AssertLineNumber(2, 1);
            AssertTaskType(3, TaskType.BRANCH);
            AssertString(3, "L1");
            AssertTaskType(4, TaskType.LABEL);
            AssertString(4, "L0");
            AssertLineNumber(4, 2);
            AssertTaskType(5, TaskType.BOOLEAN_EVALUATE);
            AssertLineNumber(5, 2);
            AssertTaskType(6, TaskType.BRANCH_FALSE);
            AssertString(6, "L2");
            AssertLineNumber(6, 2);
            AssertTaskType(7, TaskType.ASSIGNMENT);
            AssertLineNumber(7, 3);
            AssertTaskType(8, TaskType.BRANCH);
            AssertString(8, "L1");
            AssertTaskType(9, TaskType.LABEL);
            AssertLineNumber(9, 4);
            AssertString(9, "L2");
            AssertTaskType(10, TaskType.ASSIGNMENT);
            AssertLineNumber(10, 5);
            AssertTaskType(11, TaskType.LABEL);
            AssertLineNumber(11, 6);
            AssertString(11, "L1");
        }

        [Test]
        public void Compile_while_returnWhileTasks()
        {
            var src_code = "WHILE 1>0\r\n" +
                           "    \t@2 = 1;\r\n" +
                           "    \tWAIT();\r\n" +
                           "ENDWHILE;";
            compileToTaskList(src_code);
            var cnt = 0;
            AssertTaskType(cnt++, TaskType.LABEL);
            AssertLineNumber(0, 0);
            AssertTaskType(cnt++, TaskType.BOOLEAN_EVALUATE);
            AssertLineNumber(1, 0);
            AssertTaskType(cnt++, TaskType.BRANCH_FALSE);
            AssertLineNumber(2, 0);
            AssertTaskType(cnt++, TaskType.ASSIGNMENT);
            AssertLineNumber(3, 1);
            AssertTaskType(cnt++, TaskType.BUILT_IN_FUNCTION);
            AssertLineNumber(4, 2);
            AssertTaskType(cnt++, TaskType.BRANCH);
            AssertTaskType(cnt++, TaskType.LABEL);
            AssertLineNumber(6, 3);
        }

        [Test]
        public void Compile_loop_returnLoopTasks()
        {
            var src_code = "LOOP\r\n" +
                           "    \t@2 = 1;\r\n" +
                           "    \tWAIT();\r\n" +
                           "ENDLOOP;\r\n";
            compileToTaskList(src_code);
            var cnt = 0;
            AssertTaskType(cnt++, TaskType.LABEL);
            AssertLineNumber(0, 0);
            AssertTaskType(cnt++, TaskType.ASSIGNMENT);
            AssertLineNumber(1, 1);
            AssertTaskType(cnt++, TaskType.BUILT_IN_FUNCTION);
            AssertLineNumber(2, 2);
            AssertTaskType(cnt++, TaskType.BRANCH);
        }

        [Test]
        public void Compile_loopBreak_returnLoopTasks()
        {
            var src_code = "LOOP\r\n" +
                           "    \t@2 = 1;\r\n" +
                           "    \tBREAK;\r\n" +
                           "ENDLOOP;\r\n";
            compileToTaskList(src_code);
            var cnt = 0;
            AssertTaskType(cnt, TaskType.LABEL);
            AssertString(cnt++, "L0");
            AssertLineNumber(0, 0);

            AssertTaskType(cnt++, TaskType.ASSIGNMENT);
            AssertLineNumber(1, 1);

            AssertTaskType(cnt++, TaskType.BRANCH);
            AssertLineNumber(2,2);
            AssertString(2,"L1");

            AssertTaskType(cnt++, TaskType.BRANCH);
            AssertString(3, "L0");

            AssertTaskType(cnt, TaskType.LABEL);
            AssertString(4, "L1");
        }

        [Test]
        public void Compile_loopIfBreak_returnLoopTasks()
        {
            var src_code = "LOOP\r\n" +
                           " \t @2 = 1;\r\n" +
                           " \t IF @2>0\r\n" +
                           " \t     \tBREAK;\r\n" +
                           " \t ENDIF;\r\n"+
                           "ENDLOOP;\r\n";
            compileToTaskList(src_code);

            AssertTaskType(0, TaskType.LABEL);
            AssertString(0, "L0");
            AssertLineNumber(0, 0);

            AssertTaskType(1, TaskType.ASSIGNMENT);
            AssertLineNumber(1, 1);

            AssertTaskType(2, TaskType.BOOLEAN_EVALUATE);
            AssertLineNumber(2, 2);

            AssertTaskType(3, TaskType.BRANCH_FALSE);
            AssertString(3, "L2");

            AssertTaskType(4, TaskType.BRANCH);
            AssertString(4, "L1");

            AssertTaskType(5, TaskType.LABEL);
            AssertString(5, "L2");
            AssertLineNumber(5, 4);

            AssertTaskType(6, TaskType.BRANCH);
            AssertString(6, "L0");

            AssertTaskType(7, TaskType.LABEL);
            AssertString(7, "L1");

        }
    }
}