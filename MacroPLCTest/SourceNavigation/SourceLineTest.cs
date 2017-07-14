using MacroPLC;
using NUnit.Framework;

namespace MacroPLCTest
{
    public class SourceLineTest:Specification
    {
        [Test]
        public void SourceLine_AssignmentType()
        {
            var src_line = new SourceLine("@10 = 1;", 1);
            Assert.AreEqual(1, src_line.LineNumber);
            Assert.AreEqual(StatementType.ASSIGNMENT, src_line.Type);
        }
         
    }
}