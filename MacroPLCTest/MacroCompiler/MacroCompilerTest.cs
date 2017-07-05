using MacroPLC;
using MacroVariableDB;
using NUnit.Framework;
using System;

namespace MacroPLCTest
{
    public class MacroCompilerTest:Specification
    {
        [Test]
        public void StepCompileAssignment()
        {
            var compiler = new MacroCompiler("@10 = 1;");
            compiler.Compile();
            var executeLine = compiler.StepExecute();
            Assert.AreEqual(0, executeLine);
            Assert.AreEqual("1", VariableDB.LoadVariable("@10").Literal);
        }

        [Test]
        public void StepCompileAssignment_throwExceptionOfMissedEndStatement()
        {
            var compiler = new MacroCompiler("@10 = 1");
            Assert.Throws<Exception>(compiler.Compile);
        }
    }
}