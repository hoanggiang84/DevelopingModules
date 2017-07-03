using MacroPLC;
using NUnit.Framework;

namespace MacroPLCTest
{
    public class SourceReaderTest:Specification
    {
        [Test]
        public void InvalidSourceTest()
        {
            Assert.Throws<InvalidSourceCodeException>(() => new SourceReader(null),"null source");
            Assert.Throws<InvalidSourceCodeException>(() => new SourceReader(string.Empty), "empty source");
            Assert.Throws<InvalidSourceCodeException>(() => new SourceReader(" \t\r \n"), "white source");
        }

        [Test]
        public void ReadNextLine_OneLine()
        {
            var source = "dbete";
            var reader = new SourceReader(source);
            var lineContent = reader.ReadNextLine();
            Assert.AreEqual(source, lineContent);
        }

        [Test]
        public void ReadNextLine_OneLineWithEnter()
        {
            var source = "dbete\n";
            var reader = new SourceReader(source);
            var lineContent = reader.ReadNextLine();
            Assert.AreEqual(source.Substring(0,source.Length - 1), lineContent);
        }

        [Test]
        public void ReadBeyondSource_OneLineWithEnter()
        {
            var source = "dbete\n";
            var reader = new SourceReader(source);
            var lineContent = reader.ReadNextLine();
            Assert.AreEqual(source.Substring(0, source.Length - 1), lineContent);
            lineContent = reader.ReadNextLine();
            Assert.IsNull(lineContent);
            lineContent = reader.ReadNextLine();
            Assert.IsNull(lineContent);
        }

        [Test]
        public void ResetSourceIndex()
        {
            var source = "dbete\n";
            var reader = new SourceReader(source);
            var lineContent = reader.ReadNextLine();
            Assert.AreEqual(source.Substring(0, source.Length - 1), lineContent);
            reader.ResetIndex();
            lineContent = reader.ReadNextLine();
            Assert.AreEqual(source.Substring(0, source.Length - 1), lineContent);
        }

        [Test]
        public void ReadSource_ManyLinesWithEnter()
        {
            var source = "abc\n  \n efgh\n";
            var reader = new SourceReader(source);
            var lineContent = reader.ReadNextLine();
            Assert.AreEqual("abc", lineContent,"first line");
            lineContent = reader.ReadNextLine();
            Assert.IsEmpty(lineContent,"second line");
            lineContent = reader.ReadNextLine();
            Assert.AreEqual(" efgh", lineContent,"third line");
            lineContent = reader.ReadNextLine();
            Assert.IsNull(lineContent,"end of source");
        }

        [Test]
        public void ReadSource_withExpectedResults()
        {
            var whiteStr = " \t \r";
            var sourceLines = new []
                             {
                                 "x1=2;",
                                 whiteStr,
                                 "dfefef sfege",
                                 "// dfegeg",
                                 whiteStr,
                                 "if (true) then"                                 
                             };

            var source = string.Join("\n", sourceLines);
            var reader = new SourceReader(source);
            var lineContent = reader.ReadNextLine();
            Assert.AreEqual("x1=2;", lineContent, "first line");
            lineContent = reader.ReadNextLine();
            Assert.IsEmpty(lineContent, "second line");
            lineContent = reader.ReadNextLine();
            Assert.AreEqual("dfefef sfege", lineContent, "third line");
            lineContent = reader.ReadNextLine();
            Assert.AreEqual("// dfegeg", lineContent, "fourth line");
            lineContent = reader.ReadNextLine();
            Assert.IsEmpty(lineContent, "fifth line");
            lineContent = reader.ReadNextLine();
            Assert.AreEqual("if (true) then", lineContent, "sixth line");
            lineContent = reader.ReadNextLine();
            Assert.IsNull(lineContent, "end of source");
        }
    }
}
