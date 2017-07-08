using HPTypes;
using MacroPLC;
using NUnit.Framework;
using System;

namespace MacroPLCTest
{
    public class AssignmentTest:StatementTest
    {
        [SetUp]
        public void Setup()
        {
            varDB.InitializeVariables();
        }

        [Test]
        public void InvalidSyntaxNoValidVariable_ThrowException()
        {
            var tokens = GetTokens("x=12.1");
            Assert.Throws<Exception>(() => new Assignment(tokens, varDB), "Invalid variable");
        }

        [Test]
        public void InvalidSyntaxNoEqual_ThrowException()
        {
            var tokens = GetTokens("#1 - 12");
            Assert.Throws<Exception>(() => new Assignment(tokens, varDB), "Missing assign symbol");
        }

        [Test]
        public void VariableAssignment_VariableValueSet()
        {   
            var tokens = GetTokens("@10 = 12.1");
            var asign = new Assignment(tokens, varDB);
            asign.Step();
            Assert.AreEqual("12.1", varDB.LoadVariable("@10").Literal);
        }

        [Test]
        public void VariableWithNumberIndexAssignment_VariableValueSet()
        {   
            var tokens = GetTokens("@[11] = 12.1");
            var asign = new Assignment(tokens, varDB);
            asign.Step();
            Assert.AreEqual("12.1", varDB.LoadVariable("@11").Literal);
        }

        [Test]
        public void VariableWithNegativeIndexAssignment_ThrowException()
        {   
            var tokens = GetTokens("@[-11] = 12.1");
            Assert.Throws<Exception>(() => new Assignment(tokens, varDB).Execute(), "Negative index");
        }

        [Test]
        public void VariableWithNonIntegerIndexAssignment_ThrowException()
        { 
            var tokens = GetTokens("@[11.1] = 12.1");
            Assert.Throws<Exception>(() => new Assignment(tokens, varDB).Execute(), "Non-integer index");
        }

        [Test]
        public void InvalidIndexerSyntax_ThrowException()
        {
            var tokens = GetTokens("@[11 = 12.1");
            Assert.Throws<Exception>(() => new Assignment(tokens, varDB), "Missing index close");
        }

        [Test]
        public void VariableWithMathExpressionIndex_VariableValueSet()
        {
            var tokens = GetTokens("@[11+4] = 12.1");
            var asign = new Assignment(tokens, varDB);
            asign.Step();
            AssertVariableValue("12.1", "@15");
        }

        [Test]
        public void VariableWithVariableIndex_VariableValueSet()
        {   
            var tokens = GetTokens("@[#1+4] = 12.1");
            var asign = new Assignment(tokens, varDB);
            varDB.SetVariable("#1", HPType.CreateType(20));
            asign.Step();
            Assert.AreEqual("12.1", varDB.LoadVariable("@24").Literal);
        }
    }
}