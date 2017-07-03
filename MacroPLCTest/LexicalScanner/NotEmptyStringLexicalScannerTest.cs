using HPMacroCommon;
using HPMacroComponents;
using NUnit.Framework;
using System;

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
            Assert.AreEqual(TokenType.UNDEFINED, token.Type);
            Assert.AreEqual(".", token.Text);
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
        public void ScanLessSymbolToken()
        {
            source = "<";
            CreateScanner();
            var token = lexScanner.ScanNext();
            Assert.AreEqual(source, token.Text);
            Assert.AreEqual(TokenType.SYMBOL, token.Type);
        }

        [Test]
        public void ScanRandomSymbolToken()
        {
            var numberOfValidSymbols = MacroKeywords.ValidSymbols.Count;
            var index = new Random().Next(numberOfValidSymbols);

            source = MacroKeywords.ValidSymbols[index];
            CreateScanner();
            var token = lexScanner.ScanNext();
            Assert.AreEqual(source, token.Text);
            Assert.AreEqual(TokenType.SYMBOL, token.Type);
        }

        [Test]
        public void ScanUndenfiedSymbolToken()
        {
            source = "~_'?";
            CreateScanner();
            var token = lexScanner.ScanNext();
            Assert.AreEqual(source, token.Text);
            Assert.AreEqual(TokenType.UNDEFINED, token.Type);
        }

        [Test]
        public void ScanUndenfiedLetterToken()
        {
            source = "à";
            CreateScanner();
            var token = lexScanner.ScanNext();
            Assert.AreEqual(source, token.Text);
            Assert.AreEqual(TokenType.UNDEFINED, token.Type);
        }
    }
}