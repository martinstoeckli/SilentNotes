using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using SilentNotes.Workers;

namespace SilentNotesTest.Workers
{
    [TestFixture]
    public class NoteFilterTest
    {
        [Test]
        public void ContainsTag_FindsTagCaseInsensitive()
        {
            NoteFilter filter = new NoteFilter(string.Empty, "mytag");
            List<string> tagList = new List<string> { "something", "MyTag" };

            Assert.IsTrue(filter.ContainsTag(tagList));
            Assert.IsFalse(filter.ContainsTag(null));
        }

        [Test]
        public void ContainsTag_AcceptsNullParameters()
        {
            NoteFilter filter = new NoteFilter(string.Empty, null);

            Assert.IsTrue(filter.ContainsTag(null));
        }
    }
}
