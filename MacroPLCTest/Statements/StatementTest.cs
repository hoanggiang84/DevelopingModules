using HPVariableRepository;
using MacroLexScn;
using System.Collections.Generic;
using NUnit.Framework;

namespace MacroPLCTest
{
    public class StatementTest:Specification
    {
        [SetUp]
        public void Setup()
        {
            varDB.InitializeVariables();
        }

        protected VariableRepository varDB = new VariableRepository();

        protected void AssertVariableValue(string literal, string varName)
        {
            Assert.AreEqual(literal, varDB.LoadVariable(varName).Literal);
        }

        protected IEnumerable<Token> GetTokens(string source)
        {
            var scanner = new MacroLexicalScanner(source);
            var token = scanner.ScanNext();
            var returnTokens = new List<Token>();
            while (token.Type != TokenType.END)
            {
                returnTokens.Add(token);
                token = scanner.ScanNext();
            }
            return returnTokens;
        }
    }
}