using System;
using MacroLexScn;
using NUnit.Framework;
using MacroPLC;
using System.Collections.Generic;

namespace MacroPLCTest
{
    public class GMCodeExtensionTest:Specification
    {
        [Test]
        public void validateValidCode_returnTrue()
        {
            var code = "M100 10 20 30";
            var gm_ext = new GMCodeExtension(code);
            Assert.True(gm_ext.Validate());
        }

        [Test]
        public void validateValidTokens_returnTrue()
        {
            var tokens = new List<Token>()
                             {
                                 new Token("M100", TokenType.IDENTIFIER),
                                 new Token("10", TokenType.NUMBER),
                                 new Token("@24", TokenType.GLOBAL_VAR),
                                 new Token("#42", TokenType.LOCAL_VAR)
                             };
            var gm_ext = new GMCodeExtension(tokens);
            Assert.True(gm_ext.Validate());
        }

        [Test]
        public void validateValidCodeWithParameter_returnTrue()
        {
            var code = "G100 @10 20 #30";
            var gm_ext = new GMCodeExtension(code);
            Assert.True(gm_ext.Validate());
        }

        [Test]
        public void validateInValidCode_returnFalse()
        {
            var code = "M10000 10 20 30";
            var gm_ext = new GMCodeExtension(code);
            try
            {
                gm_ext.Validate();
            }
            catch (Exception ex)
            {
                Assert.True(ex.Message.Contains("Invalid") && ex.Message.Contains("command"));
            }
        }
    }
}