using HPMacroComponents;
using MacroPLC;
using NUnit.Framework;

namespace MacroPLCTest
{
    public class MacroLexicalScannerTest:Specification
    {
        [Test]
        public void ScanNullorEmptyString()
        {
            Assert.Throws<InvalidSourceCodeException>(() => new MacroLexicalScanner(null));
            Assert.Throws<InvalidSourceCodeException>(() => new MacroLexicalScanner(string.Empty));
        }

        [Test]
        public void ScanWhiteString()
        {
            var whiteStr = " \t ";
            var lexScanner = new MacroLexicalScanner(whiteStr);
            var token = lexScanner.ScanNext();
            Assert.AreEqual(whiteStr, token.Text);
        }

        [Test]
        public void ScanWhiteString_scanEndSource()
        {
            var whiteStr = " \t ";
            var lexScanner = new MacroLexicalScanner(whiteStr);
            var token = lexScanner.ScanNext();
            Assert.AreEqual(whiteStr, token.Text);
            token = lexScanner.ScanNext();
            Assert.IsNull(token);
        }

        [Test]
        public void ScanNaturalNumberToken()
        {
            var number = "10";
            var lexScanner = new MacroLexicalScanner(number);
            var token = lexScanner.ScanNext();
            Assert.AreEqual(10, int.Parse(token.Text));
            Assert.AreEqual(TokenType.NUMBER,token.Type);
        }

        [Test]
        public void ScanRationalNumberToken()
        {
            var number = "10.11";
            var lexScanner = new MacroLexicalScanner(number);
            var token = lexScanner.ScanNext();
            Assert.AreEqual(number,token.Text);
            Assert.DoesNotThrow(() => float.Parse(token.Text));
            Assert.AreEqual(TokenType.NUMBER, token.Type);
        }

        [Test]
        public void ScanInvalidRationalNumberToken()
        {
            var number = ".11";
            var lexScanner = new MacroLexicalScanner(number);
            var token = lexScanner.ScanNext();
            Assert.IsNull(token);
        }
    }
}