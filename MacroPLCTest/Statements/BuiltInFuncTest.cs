using System;
using MacroPLC;
using NUnit.Framework;

namespace MacroPLCTest
{
    public class BuiltInFuncTest:StatementTest
    {
        [Test]
        public void BuiltInTest_ExistFunctionName()
        {
            var tokens = GetTokens("WAIT()");
            var funcStatement = new BuiltInFunctionStatement(tokens, varDB);
            funcStatement.Step();
        }

        [Test]
        public void BuiltInTest_NonExistFunctionName_throwException()
        {
            var tokens = GetTokens("WAIT_WHAT()");
            Assert.Throws<Exception>(()=> new BuiltInFunctionStatement(tokens, varDB));
        }

        [Test]
        public void BuiltInTest_FunctionNameWithoutParanthesis_throwException()
        {
            var tokens = GetTokens("WAIT)");
            Assert.Throws<Exception>(() => new BuiltInFunctionStatement(tokens, varDB));
            tokens = GetTokens("WAIT(");
            Assert.Throws<Exception>(() => new BuiltInFunctionStatement(tokens, varDB));
        }

        [Test]
        public void BuiltInTest_ReturnValueFunction()
        {
            var tokens = GetTokens("MAX(1,2)");
            var funcStatement = new BuiltInFunctionStatement(tokens, varDB);
            funcStatement.Execute();

        }
    }
}