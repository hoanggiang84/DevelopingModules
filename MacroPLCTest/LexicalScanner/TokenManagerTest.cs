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
            var t = tokenMgr.LookNextToken();
            tokenMgr.Match(t.Text);
            t = tokenMgr.LookNextToken();
            Assert.IsTrue(t.Text.IsNotNullOrWhite());
            Assert.AreEqual("<",t.Text);
         }

        [Test]
        public void GetNextToken_IgnoredWhiteToken()
        {
            var tokenMgr = new TokenManager(tokens);
            var t = tokenMgr.GetNextToken();
            t = tokenMgr.GetNextToken();
            Assert.IsTrue(t.Text.IsNotNullOrWhite());
            Assert.AreEqual("<", t.Text);
        }

        [Test]
        public void LookNextToken_LastToken_ReturnEndToken()
        {
            var tokenMgr = new TokenManager(new List<Token>(){new Token(" ",TokenType.WHITE_SPACE)});
            var t = tokenMgr.LookNextToken();
            Assert.AreEqual(TokenType.END, t.Type);
        }

        [Test]
        public void GetNextToken_LastToken_ReturnEndToken()
        {
            var tokenMgr = new TokenManager(new List<Token>() { new Token(" ", TokenType.WHITE_SPACE) });
            var t = tokenMgr.GetNextToken();
            Assert.AreEqual(TokenType.END, t.Type);
        }
    }
}
