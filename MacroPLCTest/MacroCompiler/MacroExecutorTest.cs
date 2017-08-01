using HPTypes;
using HPVariableRepository;
using MacroPLC;
using NUnit.Framework;
using System;

namespace MacroPLCTest
{
    public class MacroExecutorTest:Specification
    {
        private MacroCompiler compiler;
        private MacroExecutor executor;
        private string ExecutedCode;

        private void Compile(string source)
        {
            compiler = new MacroCompiler(source);
            compiler.Compile();
            executor = new MacroExecutor(compiler.compiledTasks);
            executor.Variables.GCodeGenerated += VariablesOnGCodeGenerated;
            ExecutedCode = string.Empty;
        }

        private void VariablesOnGCodeGenerated(object sender, GCodeStatementArg gCodeStatementArg)
        {
            ExecutedCode = gCodeStatementArg.Statement;
        }

        private void ExecuteTasksAndAssertNextExecuteLine(int lineNumber)
        {
            var executeLine = executor.StepExecute();
            Assert.AreEqual(lineNumber, executeLine);
        }

        private void AssertVariableLiteral(string literal, string varName)
        {
            Assert.AreEqual(literal, executor.Variables.LoadVariable(varName).Literal);
        }

        [Test]
        public void StepCompileAssignment_throwExceptionOfMissedEndStatement()
        {
            compiler = new MacroCompiler("@10 = 1");
            Assert.Throws<Exception>(compiler.Compile);
        }

        [Test]
        public void StepCompileAssignment()
        {
            Compile("@10 = 1;");
            ExecuteTasksAndAssertNextExecuteLine(0);
            AssertVariableLiteral("1","@10");
        }

        [Test]
        public void StepCompileAssignmentWithIndexer()
        {
            Compile("@[11] = 1 + 11;");
            ExecuteTasksAndAssertNextExecuteLine(0);
            AssertVariableLiteral("12", "@11");
        }

        [Test]
        public void StepCompileGCode()
        {
            Compile("G01 X1 Y10 Z14 F11;");
            ExecuteTasksAndAssertNextExecuteLine(0);
        }

        [Test]
        public void StepCompileGcode_withVariableParameters()
        {
            Compile("G01 X1 Y345.34 Z(#12 + 1) F(@15);");
            executor.Variables.SetVariable("#12", HPType.CreateType(10));
            executor.Variables.SetVariable("@15", HPType.CreateType(12));
            ExecuteTasksAndAssertNextExecuteLine(0);
            Assert.AreEqual("G01 X1. Y345.34 Z11. F12.",ExecutedCode);
        }

        [Test]
        public void StepCompileGcode_withNestedParantheses()
        {
            Compile("G01 X1 Y345.34 Z(#12+(2 + 1)) F(@15);");
            executor.Variables.SetVariable("#12", HPType.CreateType(10));
            executor.Variables.SetVariable("@15", HPType.CreateType(12));
            ExecuteTasksAndAssertNextExecuteLine(0);
            Assert.AreEqual("G01 X1. Y345.34 Z13. F12.", ExecutedCode);
        }

        [Test]
        public void StepCompile_Built_InFunction()
        {
            Compile("WAIT();");
            ExecuteTasksAndAssertNextExecuteLine(0);
        }


    }
}