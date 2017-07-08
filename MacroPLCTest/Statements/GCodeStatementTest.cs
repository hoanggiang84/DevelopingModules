using HPTypes;
using MacroPLC;
using NUnit.Framework;
using System;

namespace MacroPLCTest
{
    public class GCodeStatementTest:StatementTest
    {
        [SetUp]
        public void Setup()
        {
            varDB.InitializeVariables();
        }

        [Test]
        public void GCodeStatement_WithNumberParameters()
        {
            var tokens = GetTokens("G01 X1 Y10 Z14 F11;");
            new GCodeStatement(tokens, varDB).Step();
        }

        [Test]
        public void GCodeStatement_InvalidGCode()
        {
            var tokens = GetTokens("G92.15");
            Assert.Throws<Exception>(()=> new GCodeStatement(tokens, varDB).Step());
        }

        [Test]
        public void GCodeStatement_WithVariableParameters()
        {
            varDB.SetVariable("#12", HPType.CreateType(10));
            varDB.SetVariable("@15", HPType.CreateType(11));
            var tokens = GetTokens("G01 X1 Y345.34 Z(#12 + 1) F(@15);");
            var statement = new GCodeStatement(tokens,varDB);
            statement.Step();
        }
    }
}