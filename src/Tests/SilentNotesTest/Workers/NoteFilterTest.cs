using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SilentNotes.Workers;

namespace SilentNotesTest.Workers
{
    [TestClass]
    public class NoteFilterTest
    {
        [TestMethod]
        public void ContainsTag_FindsTagCaseInsensitive()
        {
            NoteFilter filter = new NoteFilter(string.Empty, new[] { "mytag" }, NoteFilter.FilterOptions.FilterByTagList);
            List<string> noteTagList = new List<string> { "something", "MyTag" };

            Assert.IsTrue(filter.MatchTags(noteTagList));
            Assert.IsFalse(filter.MatchTags(null));
        }

        [TestMethod]
        public void ContainsTag_FindsOnlyWhenAllTagsAreFound()
        {
            NoteFilter filter = new NoteFilter(string.Empty, new[] { "Jan", "Mar" }, NoteFilter.FilterOptions.FilterByTagList);

            List<string> noteTagList = new List<string> { "Jan", "Feb", "Mar", "Apr" };
            Assert.IsTrue(filter.MatchTags(noteTagList));

            noteTagList = new List<string> { "Feb", "Mar", "Apr" };
            Assert.IsFalse(filter.MatchTags(noteTagList));
        }

        [TestMethod]
        public void ContainsTag_AcceptsNullParameters()
        {
            NoteFilter filter = new NoteFilter(string.Empty, null, NoteFilter.FilterOptions.FilterByTagList);

            Assert.IsTrue(filter.MatchTags(null));
        }

        [TestMethod]
        public void ContainsTag_Applies_SpecialTagNotesWithoutTags()
        {
            NoteFilter filter = new NoteFilter(string.Empty, null, NoteFilter.FilterOptions.NotesWithoutTags);

            List<string> nonEmptyTagList = new List<string> { "something", "MyTag" };
            Assert.IsFalse(filter.MatchTags(nonEmptyTagList));

            List<string> emptyTagList = new List<string>();
            Assert.IsTrue(filter.MatchTags(emptyTagList));
        }
    }
}
