using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using MacroPLC;
using NUnit.Framework;

namespace MacroPLCTest
{
    [TestFixture]
    public class SourceReaderTest
    {
        [Test]
        public void ReadNextLine_nullSource()
        {
            var reader = new SourceReader(null);
            var lineContent = reader.ReadNextLine();
            Assert.IsNull(lineContent);
        }

        [Test]
        public void ReadNextLine_EmptySource()
        {
            var source = string.Empty;
            var reader = new SourceReader(source);
            var lineContent = reader.ReadNextLine();
            Assert.IsEmpty(lineContent);
        }

    }
}
