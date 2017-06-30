using HPGCodeValidation;
using NUnit.Framework;
using System;

namespace UnitTestMacroCompiler
{
    [TestFixture]
    public class UnitTestGCodeValidate
    {
        private GCodeValidate GValidate;

        private static string FailMessage(GCodeSatus status, string word)
        {
            return string.Format("G-Code Error: {1} '{0}'", word, status);
        }

        [Test]
        public void TestInvalidGCode()
        {
            GValidate = new GCodeValidate("g1");
            Assert.Throws<Exception>(() => GValidate.Validate());

            GValidate = new GCodeValidate("G");
            Assert.Throws<Exception>(() => GValidate.Validate());

            GValidate = new GCodeValidate("G0.1");
            Assert.Throws<Exception>(() => GValidate.Validate());

            GValidate = new GCodeValidate("G01 U35");
            Assert.Throws<Exception>(() => GValidate.Validate());
            
            GValidate = new GCodeValidate("G28.001");
            Assert.Throws<Exception>(() => GValidate.Validate());

            GValidate = new GCodeValidate("G30.2");
            Assert.Throws<Exception>(() => GValidate.Validate());

            GValidate = new GCodeValidate("G28.3");
            Assert.Throws<Exception>(() => GValidate.Validate());

            GValidate = new GCodeValidate("G92.15");
            Assert.Throws<Exception>(() => GValidate.Validate());

            GValidate = new GCodeValidate("M6");
            Assert.Throws<Exception>(() => GValidate.Validate());

            GValidate = new GCodeValidate("M6.2");
            Assert.Throws<Exception>(() => GValidate.Validate());
        }


        [Test]
        public void TestConflictModalGroup()
        {
            GValidate = new GCodeValidate("G10 G30");
            try
            {
                GValidate.Validate();
            }
            catch (Exception ex)
            {
                var msg = FailMessage(GCodeSatus.AXIS_COMMAND_CONFLICT, "G30");
                Assert.AreEqual(msg, ex.Message);
            }
            
        }

        [Test]
        public void TestValidGCode()
        {
            GValidate = new GCodeValidate("G28.1 M1");
            GValidate.Validate();
            Assert.Pass();
        }

        [Test]
        public void TestDuplicateParameter()
        {
            GValidate = new GCodeValidate("G01 X2 X124 Y3 Z30 F345.45");
            try
            {
                GValidate.Validate();
            }
            catch (Exception ex)
            {
                var msg = FailMessage(GCodeSatus.WORD_REPEATED, "X124");
                Assert.AreEqual(msg, ex.Message);
            }
        }

        [Test]
        public void TestInvalidNegativeParameter()
        {
            GValidate = new GCodeValidate("G01 X2 T-10 Y3 Z30 F345.45");
            try
            {
                GValidate.Validate();
            }
            catch (Exception ex)
            {
                var msg = FailMessage(GCodeSatus.NEGATIVE_VALUE, "T-10");
                Assert.AreEqual(msg, ex.Message);
            }
        }

        [Test]
        public void TestNoGcodeCommand()
        {
            GValidate = new GCodeValidate("X2 T10 Y3 Z30 F345.45");
            try
            {
                GValidate.Validate();
            }
            catch (Exception ex)
            {
                var msg = FailMessage(GCodeSatus.INVALID_STATEMENT, "No Command");
                Assert.AreEqual(msg, ex.Message);
            }
        }
    }
}
