using MacroPLC;
using NUnit.Framework;
using System;

namespace MacroPLCTest
{
    public class MacroCompilerTest:Specification
    {
        private MacroCompiler compiler;
        private MacroExecutor executor;

        private void Compile(string source)
        {
            compiler = new MacroCompiler(source);
            compiler.Compile();
            executor = new MacroExecutor(compiler.compiledTasks);
        }

        private void AssertNextExecuteLine(int lineNumber)
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
            AssertNextExecuteLine(0);
            AssertVariableLiteral("1","@10");
        }

        [Test]
        public void StepCompileAssignmentWithIndexer()
        {
            Compile("@[11] = 1 + 11;");
            AssertNextExecuteLine(0);
            AssertVariableLiteral("12", "@11");
        }

        [Test]
        public void StepCompileGCode()
        {
            Compile("G01 X1 Y10 Z14 F11;");
            AssertNextExecuteLine(0);
        }
    }
}