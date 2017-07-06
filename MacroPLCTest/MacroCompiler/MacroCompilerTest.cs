using MacroPLC;
using NUnit.Framework;
using System;

namespace MacroPLCTest
{
    public class MacroCompilerTest:Specification
    {
        private MacroCompiler compiler;
        private MacroExecutor executor;

        [Test]
        public void StepCompileAssignment()
        {
            compiler = new MacroCompiler("@10 = 1;");
            compiler.Compile();

            executor = new MacroExecutor(compiler.compiledTasks);
            var executeLine = executor.StepExecute();

            Assert.AreEqual(0, executeLine);
            Assert.AreEqual("1", executor.Variables.LoadVariable("@10").Literal);
        }

        [Test]
        public void StepCompileAssignment_throwExceptionOfMissedEndStatement()
        {
            compiler = new MacroCompiler("@10 = 1");
            Assert.Throws<Exception>(compiler.Compile);
        }
    }
}