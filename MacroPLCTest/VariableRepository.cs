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

        [Test]
        public void InstantiateVariable_ResetToInitialState()
        {
            var varDB1 = new VariableRepository();
            varDB1.SetVariable("#1",HPType.CreateType(10));

            var varDB2 = new VariableRepository(varDB1);
            var val = varDB2.LoadVariable("#1");
            Assert.AreEqual("10",val.Literal);

            varDB2.SetVariable("#1", HPType.CreateType(20));
            val = varDB2.LoadVariable("#1");
            Assert.AreEqual("20", val.Literal);

            varDB2.ResetLocalVariables();
            val = varDB2.LoadVariable("#1");
            Assert.AreEqual("10", val.Literal);


        }

    }
}