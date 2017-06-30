using System;
using HPMacroFunctions;
using HPTypes;
using NUnit.Framework;
using System.Collections.Generic;

namespace UnitTestMacroCompiler
{
    [TestFixture]
    public class UnitTestMacrofunction
    {
        private List<HPType> args = new List<HPType>();
        private Func<List<HPType>, HPType> function;
        private string functionName = string.Empty;

        [TearDown]
        public void CleanUp()
        {
            functionName = string.Empty;
            function = null;
            args = new List<HPType>();
        }

        [Test]
        public void TestCallIntToFloatMacroFunction()
        {
            functionName = "ITOF";
            args = new List<HPType>{HPType.CreateType(10)};
            function = HPFUNC.GetFunction(functionName);
            var result = function.Invoke(args);
            Assert.AreEqual(VariableType.FLOAT, result.Type);
            Assert.AreEqual(10, float.Parse(result.Literal));
        }

        [Test]
        public void TestCallFloatToIntFunction()
        {
            functionName = "FTOI";
            args = new List<HPType> { HPType.CreateType(10.10f) };
            function = HPFUNC.GetFunction(functionName);
            var result = function.Invoke(args);
            Assert.AreEqual(VariableType.INT, result.Type);
            Assert.AreEqual(10, int.Parse(result.Literal));
        }

        [Test]
        public void TestCallAbsoluteFunction()
        {
            functionName = "ABS";
            args = new List<HPType> { HPType.CreateType(-10.10f) };
            function = HPFUNC.GetFunction(functionName);
            var result = function.Invoke(args);
            Assert.AreEqual(VariableType.FLOAT, result.Type);
            Assert.AreEqual(10.10f, float.Parse(result.Literal));
        }

        [Test]
        public void TestCallInvalidFunction()
        {
            functionName = "somename";
            args = new List<HPType> { HPType.CreateType(-10.10f) };
            Assert.Throws<Exception>(()=> HPFUNC.GetFunction(functionName));
        }
    }
}