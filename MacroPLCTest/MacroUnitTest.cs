using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using MacroPLC;
using NUnit.Framework;

namespace MacroPLCTest
{
    [TestFixture]
    public class MacroUnitTest
    {
        [Test]
        public void DetectNextLine()
        {
            var source = "";
            var cmplr = new MacroCompiler(source);
            var lineContent = cmplr.CompileNextLine();
            Assert.IsEmpty(lineContent);
        }

    }
}
