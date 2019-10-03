using System;
using NUnit.Framework;
using SilentNotes.Models;

namespace SilentNotesTest.Models
{
    [TestFixture]
    public class NoteModelTest
    {
        [Test]
        public void CloneCopiesAllProperties()
        {
            NoteModel note1 = new NoteModel
            {
                Id = Guid.NewGuid(),
                HtmlContent = "<html>",
                BackgroundColorHex = "#000000",
                InRecyclingBin = true,
                CreatedAt = new DateTime(2000, 10, 22, 18, 55, 30),
                ModifiedAt = new DateTime(2001, 10, 22, 18, 55, 30),
            };
            NoteModel note2 = note1.Clone();

            Assert.AreEqual(note1.Id, note2.Id);
            Assert.AreEqual(note1.HtmlContent, note2.HtmlContent);
            Assert.AreEqual(note1.BackgroundColorHex, note2.BackgroundColorHex);
            Assert.AreEqual(note1.InRecyclingBin, note2.InRecyclingBin);
            Assert.AreEqual(note1.CreatedAt, note2.CreatedAt);
            Assert.AreEqual(note1.ModifiedAt, note2.ModifiedAt);
        }
    }
}
