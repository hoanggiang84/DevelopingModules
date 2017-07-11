using MacroLexScn;
using NUnit.Framework;
using System.Collections.Generic;
using UtilitiesVS2008WinCE;

namespace MacroPLCTest
{
    public class TokenManagerTest:Specification
    {
        private List<Token> tokens;

        [SetUp]
        public void SetUp()
        {
            tokens = new List<Token>()
                         {
                             new Token("first",TokenType.IDENTIFIER),
                             new Token(" \t ",TokenType.WHITE_SPACE),
                             new Token("<",TokenType.SYMBOL),
                             new Token("second",TokenType.IDENTIFIER),
                         };
        }

        [Test]
        public void LookNextToken_IgnoredWhiteToken()
         {
            var tokenMgr = new TokenManager(tokens);
            var t = tokenMgr.IgnoreWhiteLookNextToken();
            tokenMgr.Match(t.Text);
            t = tokenMgr.IgnoreWhiteLookNextToken();
            Assert.IsTrue(t.Text.IsNotNullOrWhite());
            Assert.AreEqual("<",t.Text);
         }

        [Test]
        public void GetNextToken_IgnoredWhiteToken()
        {
            var tokenMgr = new TokenManager(tokens);
            var t = tokenMgr.IgnoreWhiteGetNextToken();
            t = tokenMgr.IgnoreWhiteGetNextToken();
            Assert.IsTrue(t.Text.IsNotNullOrWhite());
            Assert.AreEqual("<", t.Text);
        }

        [Test]
        public void LookNextToken_LastToken_ReturnEndToken()
        {
            var tokenMgr = new TokenManager(new List<Token>(){new Token(" ",TokenType.WHITE_SPACE)});
            var t = tokenMgr.IgnoreWhiteLookNextToken();
            Assert.AreEqual(TokenType.END, t.Type);
        }

        [Test]
        public void GetNextToken_LastToken_ReturnEndToken()
        {
            var tokenMgr = new TokenManager(new List<Token>() { new Token(" ", TokenType.WHITE_SPACE) });
            var t = tokenMgr.IgnoreWhiteGetNextToken();
            Assert.AreEqual(TokenType.END, t.Type);
        }
    }
}
