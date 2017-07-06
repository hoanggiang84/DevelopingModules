using HPMacroCommon;
using HPTypes;
using HPVariableRepository;
using MacroLexScn;
using MacroPLC;
using NUnit.Framework;
using System.Collections.Generic;
using System;

namespace MacroPLCTest
{
    public class AssignmentTest:Specification
    {
        private VariableRepository varDB = new VariableRepository();

        [SetUp]
        public void Setup()
        {
            varDB.InitializeVariables();
        }

        [Test]
        public void InvalidSyntaxNoValidVariable_ThrowException()
        {   //x = 12.1
            var tokens = new List<Token>
                             {
                                 new Token("x", TokenType.IDENTIFIER),
                                 new Token("=",TokenType.SYMBOL),
                                 new Token("12.1",TokenType.NUMBER)
                             };
            Assert.Throws<Exception>(() => new Assignment(tokens, varDB), "Invalid variable");
        }

        [Test]
        public void InvalidSyntaxNoEqual_ThrowException()
        {   //#1 = 12.1
            var tokens = new List<Token>
                             {
                                 new Token("#1", TokenType.LOCAL_VAR),
                                 new Token("-",TokenType.SYMBOL),
                                 new Token("12.1",TokenType.NUMBER)
                             };
            Assert.Throws<Exception>(() => new Assignment(tokens, varDB), "Missing assign symbol");
        }

        [Test]
        public void VariableAssignment_VariableValueSet()
        {   // @10 = 12.1
            var whiteToken = new Token("  ", TokenType.WHITE_SPACE);
            var tokens = new List<Token>
                             {
                                 new Token("@10", TokenType.GLOBAL_VAR),
                                 whiteToken,
                                 new Token("=",TokenType.IDENTIFIER),
                                 whiteToken,
                                 new Token("12.1",TokenType.NUMBER)
                             };
            var asign = new Assignment(tokens, varDB);
            asign.Step();

            Assert.AreEqual("12.1", varDB.LoadVariable("@10").Literal);
        }

        [Test]
        public void VariableWithNumberIndexAssignment_VariableValueSet()
        {   //@[11] = 12.1
            var whiteToken = new Token("  ", TokenType.WHITE_SPACE);
            var tokens = new List<Token>
                             {
                                 new Token("@", TokenType.GLOBAL_VAR),
                                 new Token(MacroKeywords.INDEX_OPEN,TokenType.SYMBOL),
                                 new Token("11",TokenType.NUMBER),
                                 new Token(MacroKeywords.INDEX_CLOSE,TokenType.SYMBOL),
                                 whiteToken,
                                 new Token("=",TokenType.IDENTIFIER),
                                 whiteToken,
                                 new Token("12.1",TokenType.NUMBER)
                             };
            var asign = new Assignment(tokens, varDB);
            asign.Step();

            Assert.AreEqual("12.1", varDB.LoadVariable("@11").Literal);
        }

        [Test]
        public void VariableWithNegativeIndexAssignment_ThrowException()
        {   //@[-11] = 12.1
            var whiteToken = new Token("  ", TokenType.WHITE_SPACE);
            var tokens = new List<Token>
                             {
                                 new Token("@", TokenType.GLOBAL_VAR),
                                 new Token(MacroKeywords.INDEX_OPEN,TokenType.SYMBOL),
                                 new Token("-",TokenType.SYMBOL),
                                 new Token("11",TokenType.NUMBER),
                                 new Token(MacroKeywords.INDEX_CLOSE,TokenType.SYMBOL),
                                 whiteToken,
                                 new Token("=",TokenType.IDENTIFIER),
                                 whiteToken,
                                 new Token("12.1",TokenType.NUMBER)
                             };
            Assert.Throws<Exception>(() => new Assignment(tokens, varDB), "Negative index");
        }

        [Test]
        public void VariableWithNonIntegerIndexAssignment_ThrowException()
        {   //@[-11] = 12.1
            var whiteToken = new Token("  ", TokenType.WHITE_SPACE);
            var tokens = new List<Token>
                             {
                                 new Token("@", TokenType.GLOBAL_VAR),
                                 new Token(MacroKeywords.INDEX_OPEN,TokenType.SYMBOL),
                                 new Token("11.2",TokenType.NUMBER),
                                 new Token(MacroKeywords.INDEX_CLOSE,TokenType.SYMBOL),
                                 whiteToken,
                                 new Token("=",TokenType.IDENTIFIER),
                                 whiteToken,
                                 new Token("12.1",TokenType.NUMBER)
                             };
            Assert.Throws<Exception>(() => new Assignment(tokens, varDB), "Non-integer index");
        }

        [Test]
        public void InvalidIndexerSyntax_ThrowException()
        {   //@[-11] = 12.1
            var whiteToken = new Token("  ", TokenType.WHITE_SPACE);
            var tokens = new List<Token>
                             {
                                 new Token("@", TokenType.GLOBAL_VAR),
                                 new Token(MacroKeywords.INDEX_OPEN,TokenType.SYMBOL),
                                 new Token("11.2",TokenType.NUMBER),
                                 whiteToken,
                                 new Token("=",TokenType.IDENTIFIER),
                                 whiteToken,
                                 new Token("12.1",TokenType.NUMBER)
                             };
            Assert.Throws<Exception>(() => new Assignment(tokens, varDB), "Missing index close");
        }

        [Test]
        public void VariableWithMathExpressionIndex_VariableValueSet()
        {   //@[-11] = 12.1
            var whiteToken = new Token("  ", TokenType.WHITE_SPACE);
            var tokens = new List<Token>
                             {
                                 new Token("@", TokenType.GLOBAL_VAR),
                                 new Token(MacroKeywords.INDEX_OPEN,TokenType.SYMBOL),
                                 new Token("11",TokenType.NUMBER),
                                 new Token("+",TokenType.SYMBOL),
                                 new Token("4",TokenType.NUMBER),
                                 new Token(MacroKeywords.INDEX_CLOSE,TokenType.SYMBOL),
                                 whiteToken,
                                 new Token("=",TokenType.IDENTIFIER),
                                 whiteToken,
                                 new Token("12.1",TokenType.NUMBER)
                             };

            var asign = new Assignment(tokens, varDB);
            asign.Step();
            Assert.AreEqual("12.1", varDB.LoadVariable("@15").Literal);
        }

        [Test]
        public void VariableWithVariableIndex_VariableValueSet()
        {   //@[#1+4] = 12.1
            varDB.SetVariable("#1",HPType.CreateType(20));
            var whiteToken = new Token("  ", TokenType.WHITE_SPACE);
            var tokens = new List<Token>
                             {
                                 new Token("@", TokenType.GLOBAL_VAR),
                                 new Token(MacroKeywords.INDEX_OPEN,TokenType.SYMBOL),
                                 new Token("#1",TokenType.LOCAL_VAR),
                                 new Token("+",TokenType.SYMBOL),
                                 new Token("4",TokenType.NUMBER),
                                 new Token(MacroKeywords.INDEX_CLOSE,TokenType.SYMBOL),
                                 whiteToken,
                                 new Token("=",TokenType.IDENTIFIER),
                                 whiteToken,
                                 new Token("12.1",TokenType.NUMBER)
                             };

            var asign = new Assignment(tokens, varDB);
            asign.Step();
            Assert.AreEqual("12.1", varDB.LoadVariable("@24").Literal);
        }
    }
}