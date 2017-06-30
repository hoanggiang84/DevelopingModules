using HPMacroComponents;

using NUnit.Framework;

namespace MacroPLCTest
{
    public class NotEmptyStringLexicalScannerTest:MacroLexicalScannerTest
    {
        [Test]
        public void ScanWhiteString()
        {
            source = " \t ";
            CreateScanner();
            var token = lexScanner.ScanNext();
            Assert.AreEqual(source, token.Text);
        }

        [Test]
        public void ScanscanEndSource()
        {
            source = " \t ";
            CreateScanner();
            var token = lexScanner.ScanNext();
            token = lexScanner.ScanNext();
            Assert.IsNull(token);
        }

        [Test]
        public void ScanNaturalNumberToken()
        {
            source = "10";
            CreateScanner();
            var token = lexScanner.ScanNext();
            Assert.AreEqual(10, int.Parse(token.Text));
            Assert.AreEqual(TokenType.NUMBER, token.Type);
        }

        [Test]
        public void ScanWhiteSpacesThenNaturalNumberToken()
        {
            source = " \t 10";
            CreateScanner();

            var token = lexScanner.ScanNext();
            Assert.AreEqual(" \t ", token.Text);
            Assert.AreEqual(TokenType.WHITE_SPACE, token.Type);

            token = lexScanner.ScanNext();
            Assert.AreEqual(10, int.Parse(token.Text));
            Assert.AreEqual(TokenType.NUMBER, token.Type);

            token = lexScanner.ScanNext();
            Assert.IsNull(token);
        }

        [Test]
        public void ScanRationalNumberToken()
        {
            source = "10.11";
            CreateScanner();
            var token = lexScanner.ScanNext();
            Assert.AreEqual(source, token.Text);
            Assert.DoesNotThrow(() => float.Parse(token.Text));
            Assert.AreEqual(TokenType.NUMBER, token.Type);
        }

        [Test]
        public void ScanInvalidRationalNumberToken()
        {
            source = ".11";
            CreateScanner();
            var token = lexScanner.ScanNext();
            Assert.IsNull(token);
        }

        [Test]
        public void ScanIdentifierToken()
        {
            source = "fegeg";
            CreateScanner();
            var token = lexScanner.ScanNext();
            Assert.AreEqual(source, token.Text);
            Assert.AreEqual(TokenType.IDENTIFIER, token.Type);
        }

        [Test]
        public void ScanIdentifierAndThenNumberToken()
        {
            source = "fegeg2123";
            CreateScanner();
            var token = lexScanner.ScanNext();
            Assert.AreEqual(TokenType.IDENTIFIER, token.Type);
            
            token = lexScanner.ScanNext();
            Assert.AreEqual(TokenType.NUMBER, token.Type);
        }

        [Test]
        public void ScanSymbolToken()
        {
            source = "<";
            CreateScanner();
            var token = lexScanner.ScanNext();
            Assert.AreEqual(source, token.Text);
            Assert.AreEqual(TokenType.SYMBOL, token.Type);
        }
    }
}