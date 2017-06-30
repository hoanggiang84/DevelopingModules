using HPTypes;
using NUnit.Framework;
using System;

namespace UnitTestMacroCompiler
{
    [TestFixture]
    public class HPTypeTest
    {
        [Test]
        public void TestInteger()
        {
            var intLiteral = HPType.CreateType("1243");
            Assert.AreEqual(1243,int.Parse(intLiteral.Literal));
        }

        [Test]
        public void TestBool()
        {
            var boolLiteral = HPType.CreateType("tRuE");
            Assert.IsTrue(bool.Parse(boolLiteral.Literal));
        }

        [Test]
        public void TestFloat()
        {
            var floatLiteral = HPType.CreateType(".3435");
            Assert.AreEqual(0.3435f, float.Parse(floatLiteral.Literal));
        }

        [Test]
        public void TestInvalidType()
        {
            Assert.Throws<Exception>(() => HPType.CreateType("3f3v"));
        }
    }
}