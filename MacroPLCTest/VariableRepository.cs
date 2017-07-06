using System;
using HPTypes;
using HPVariableRepository;
using NUnit.Framework;

namespace MacroPLCTest
{
    public class VariableRepositoryTest:Specification
    {
        private VariableRepository VarDB = new VariableRepository();

        [SetUp]
        public void SetUp()
        {
            VarDB.InitializeVariables();
        }

        [TearDown]
        public void TearDown()
        {
            VarDB.ResetLocalVariables();
        }

        [Test]
        public void SetAndLoadGlobalVariable()
        {
            VarDB.SetVariable("@1", HPType.CreateType("0.12"));
            var variableValue = VarDB.LoadVariable("@1").Literal;
            Assert.AreEqual(0.12f, float.Parse(variableValue));
        }

        [Test]
        public void SetAndLoadLocalVariable()
        {
            VarDB.SetVariable("#1", HPType.CreateType("0.12"));
            var variableValue = VarDB.LoadVariable("#1").Literal;
            Assert.AreEqual(0.12f, float.Parse(variableValue));
        }

        [Test]
        public void LoadLocalVariablesHaveSameNameInDifferentScope_ReturnCurrentScopeVariable()
        {
            var varName1 = "#1";
            VarDB.SetVariable(varName1, HPType.CreateType("0.12"));
            var variableValue = VarDB.LoadVariable(varName1).Literal;
            Assert.AreEqual(0.12f, float.Parse(variableValue));

            VarDB.CreateNewLocalVariablesScope();
            VarDB.SetVariable(varName1,HPType.CreateType("10"));
            variableValue = VarDB.LoadVariable(varName1).Literal;
            Assert.AreEqual(10, int.Parse(variableValue));
        }

        [Test]
        public void LoadLocalVariableInDifferentScope_ThrowException()
        {
            var varName1 = "#1";
            VarDB.SetVariable(varName1, HPType.CreateType("0.12"));
            var variableValue = VarDB.LoadVariable(varName1).Literal;
            Assert.AreEqual(0.12f, float.Parse(variableValue));

            VarDB.CreateNewLocalVariablesScope();
            Assert.Throws<Exception>(() => VarDB.LoadVariable(varName1));
        }


    }
}