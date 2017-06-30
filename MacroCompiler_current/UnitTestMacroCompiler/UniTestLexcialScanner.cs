using NUnit.Framework;
using HPMacroComponents;
using System.Collections.Generic;

namespace UnitTestMacroCompiler
{
    [TestFixture]
    public class UniTestLexcialScanner
    {
        private LexicalScanner scanner = new LexicalScanner(string.Empty);

        [Test]
        public void TestScanWhiteSpace()
        {
            var source = " \r\n\t";
            scanner.SetSource(source);
            var token = scanner.Scan();
            Assert.AreEqual(TokenType.END, token.Type);
        }

        [Test]
        public void TestScanIdentifier()
        {
            var source = "d123";
            scanner.SetSource(source);
            var token = scanner.Scan();
            Assert.AreEqual(TokenType.IDENTIFIER, token.Type);
        }

        [Test]
        public void TestScanInteger()
        {
            var source = "2123";
            scanner.SetSource(source);
            var token = scanner.Scan();
            Assert.AreEqual(TokenType.NUMBER, token.Type);
        }

        [Test]
        public void TestScanIdentifierInteger()
        {
            var source = " db23sd 3434";
            scanner.SetSource(source);
            var token = scanner.Scan();
            Assert.AreEqual(TokenType.IDENTIFIER, token.Type);

            token = scanner.Scan();
            Assert.AreEqual(TokenType.NUMBER, token.Type);
        }

        [Test]
        public void TestScanSymbol()
        {
            var source = string.Join(" ", MacroKeywords.ValidSymbols.ToArray());
            scanner.SetSource(source);
            var token = scanner.Scan();
            var count = 0;
            while (token.Type != TokenType.END)
            {
                count++;
                Assert.AreEqual(TokenType.SYMBOL, token.Type);
                token = scanner.Scan();
            }
            Assert.AreEqual(MacroKeywords.ValidSymbols.Count, count);
        }

        [Test]
        public void TestScanUndefinedSymbol()
        {
            var source = " \t add 1 #32 35.435 < > != [fevei de";
            scanner.SetSource(source);
            var tokens = new List<Token>();
            var count = 0;
            var token = scanner.Scan();
            while(token.Type != TokenType.END)
            {
                count++;
                tokens.Add(token);
                token = scanner.Scan();
            }
            Assert.AreEqual(count,10);
            Assert.AreEqual(tokens[3].Type, TokenType.NUMBER);
        }
    }
}