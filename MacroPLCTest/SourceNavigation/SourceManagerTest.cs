using MacroPLC;
using NUnit.Framework;
using UtilitiesVS2008WinCE;

namespace MacroPLCTest
{
    public class SourceManagerTest:Specification
    {
        private string source = "one\ntwo\nfour\n";
        private SourceManager sourceMgr;

        [SetUp]
        public void SetUp()
        {
            sourceMgr = new SourceManager(source);
        }

        [Test]
        public void GetFirstLine_returnLineIndexAndContent()
        {
            var lineContent = sourceMgr.GetNextLine();
            Assert.AreEqual(0,lineContent.LineNumber);
            Assert.AreEqual("one", lineContent.Text);
        }

        [Test]
        public void GetLineAtIndex_returnLineContent()
        {
            var index = 2;
            var lineContent = sourceMgr.GetLineAt(index);
            Assert.AreEqual("four", lineContent.Text);
        }

        [Test]
        public void GetLinesUntilEnd_returnIndicesAndLineContents()
        {
            var count = 0;
            var sourceLines = source.Split(new[] {'\n'}).RemoveEmptyString();
            var lineContent = sourceMgr.GetNextLine();
            while (lineContent != null)
            {
                Assert.AreEqual(count, lineContent.LineNumber);
                Assert.AreEqual(sourceLines[count], lineContent.Text);
                lineContent = sourceMgr.GetNextLine();
                count++;
            }
        }
    }
}