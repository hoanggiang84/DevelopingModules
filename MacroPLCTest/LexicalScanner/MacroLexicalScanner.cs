using HPMacroComponents;
using MacroPLC;
using NUnit.Framework;

namespace MacroPLCTest
{
    public class MacroLexicalScannerTest:Specification
    {
        protected string source = string.Empty;
        protected MacroLexicalScanner lexScanner;

        protected void CreateScanner()
        {
            lexScanner = new MacroLexicalScanner(source); 
        }

        [Test]
        public void ScanNullorEmptyString()
        {
            Assert.Throws<InvalidSourceCodeException>(() => new MacroLexicalScanner(null));
            Assert.Throws<InvalidSourceCodeException>(() => new MacroLexicalScanner(string.Empty));
        }

    }
}