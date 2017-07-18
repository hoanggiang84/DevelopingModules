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
            int index;
            var lineContent = sourceMgr.GetNextLine(out index);
            Assert.AreEqual(0,index);
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
            int index;
            var count = 0;
            var sourceLines = source.Split(new[] {'\n'}).RemoveEmptyString();
            var lineContent = sourceMgr.GetNextLine(out index);
            while (lineContent != null)
            {
                Assert.AreEqual(count, index);
                Assert.AreEqual(sourceLines[count], lineContent.Text);
                lineContent = sourceMgr.GetNextLine(out index);
                count++;
            }
        }
    }
}