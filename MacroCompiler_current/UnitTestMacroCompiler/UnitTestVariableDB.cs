using HPTypes;
using MacroVariableDB;
using NUnit.Framework;
using System;

namespace UnitTestMacroCompiler
{
    [TestFixture]
    public class VariablesDBTest
    {

        [SetUp]
        public void Setup()
        {
            VariableDB.InitializeVariables();
        }

        [TearDown]
        public void Clear()
        {
            VariableDB.ResetLocalVariables();
        }

        [Test]
        public void SetGlobalVariable()
        {
            VariableDB.SetVariable("@1", HPType.CreateType("0.12"));
            var variableValue = VariableDB.LoadVariable("@1").Literal;
            Assert.AreEqual(0.12f,float.Parse(variableValue));
        }

        [Test]
        public void SetLocalVariable()
        {
            VariableDB.SetVariable("#1", HPType.CreateType("0.12"));
            var variableValue = VariableDB.LoadVariable("#1").Literal;
            Assert.AreEqual(0.12f, float.Parse(variableValue));
        }

        [Test]
        public void SetLocalVariableDifferentScopes()
        {
            var varName1 = "#1";
            VariableDB.SetVariable(varName1, HPType.CreateType("0.12"));
            var variableValue = VariableDB.LoadVariable(varName1).Literal;
            Assert.AreEqual(0.12f, float.Parse(variableValue));

            var varName2 = "local1";
            VariableDB.CreateNewLocalVariablesScope();
            VariableDB.SetVariable(varName2, HPType.CreateType("15"));
            variableValue = VariableDB.LoadVariable(varName2).Literal;
            Assert.AreEqual(15, int.Parse(variableValue));
            Assert.Throws<Exception>(() => VariableDB.LoadVariable(varName1));

            VariableDB.ReturnPreviousLocalVariablesScope();
            variableValue = VariableDB.LoadVariable(varName1).Literal;
            Assert.AreEqual(0.12f, float.Parse(variableValue));
            Assert.Throws<Exception>(() => VariableDB.LoadVariable(varName2));
        }
    }
}