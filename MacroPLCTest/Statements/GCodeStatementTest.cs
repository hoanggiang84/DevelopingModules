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
    }
}