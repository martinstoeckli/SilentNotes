using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SilentNotes.Models;
using SilentNotes.Workers;

namespace SilentNotesTest.Models
{
    [TestClass]
    public class NoteModelTest
    {
        [TestMethod]
        public void LazyGenerationOfId()
        {
            NoteModel model = new NoteModel();
            Guid id1 = model.Id;
            Guid id2 = model.Id;

            Assert.AreNotEqual(Guid.Empty, model.Id); // Id is created when first accessed
            Assert.AreEqual(id1, id2); // Id does not change when accessed repeatedly
        }

        [TestMethod]
        public void CloneCopiesAllProperties()
        {
            NoteModel note1 = new NoteModel
            {
                Id = Guid.NewGuid(),
                NoteType = NoteType.Checklist,
                HtmlContent = "<html>",
                BackgroundColorHex = "#000000",
                InRecyclingBin = true,
                CreatedAt = new DateTime(2000, 10, 22, 18, 55, 30),
                ModifiedAt = new DateTime(2001, 10, 22, 18, 55, 30),
                MetaModifiedAt = new DateTime(2001, 10, 23, 18, 55, 30),
                SafeId = new Guid("10000000000000000000000000000000"),
                Tags = new List<string>() { "Aa", "Bb" },
            };
            NoteModel note2 = note1.Clone();

            Assert.AreEqual(note1.Id, note2.Id);
            Assert.AreEqual(note1.NoteType, note2.NoteType);
            Assert.AreEqual(note1.HtmlContent, note2.HtmlContent);
            Assert.AreEqual(note1.BackgroundColorHex, note2.BackgroundColorHex);
            Assert.AreEqual(note1.InRecyclingBin, note2.InRecyclingBin);
            Assert.AreEqual(note1.CreatedAt, note2.CreatedAt);
            Assert.AreEqual(note1.ModifiedAt, note2.ModifiedAt);
            Assert.AreEqual(note1.MetaModifiedAt, note2.MetaModifiedAt);
            Assert.AreEqual(note1.SafeId, note2.SafeId);
            Assert.AreEqual(note1.Tags.Count, note2.Tags.Count);
            Assert.AreEqual(note1.Tags[0], note2.Tags[0]);
            Assert.AreEqual(note1.Tags[1], note2.Tags[1]);

            // Serialized notes must be identical
            string note1Xml = XmlUtils.SerializeToString(note1);
            string note2Xml = XmlUtils.SerializeToString(note2);
            Assert.AreEqual(note1Xml, note2Xml);
        }

        [TestMethod]
        public void CloneTo_OnSameInstanceKeepsNoteUntouched()
        {
            NoteModel note1 = new NoteModel
            {
                Id = Guid.NewGuid(),
                NoteType = NoteType.Checklist,
                HtmlContent = "<html>",
                BackgroundColorHex = "#000000",
                InRecyclingBin = true,
                CreatedAt = new DateTime(2000, 10, 22, 18, 55, 30),
                ModifiedAt = new DateTime(2001, 10, 22, 18, 55, 30),
                MetaModifiedAt = new DateTime(2001, 10, 23, 18, 55, 30),
                SafeId = new Guid("10000000000000000000000000000000"),
                Tags = new List<string>() { "Aa", "Bb" },
            };
            string noteBeforeXml = XmlUtils.SerializeToString(note1);

            note1.CloneTo(note1);
            string noteAfterXml = XmlUtils.SerializeToString(note1);

            Assert.AreEqual(noteBeforeXml, noteAfterXml);
        }

        [TestMethod]
        public void MetaModifiedAt_ReturnsNullIfNotNewerThanModifiedAt()
        {
            NoteModel note1 = new NoteModel { ModifiedAt = new DateTime(2020, 1, 1) };

            note1.MetaModifiedAt = new DateTime(2010, 1, 1);
            Assert.IsNull(note1.MetaModifiedAt);

            note1.MetaModifiedAt = new DateTime(2020, 1, 1);
            Assert.IsNull(note1.MetaModifiedAt);

            note1.MetaModifiedAt = null;
            Assert.IsNull(note1.MetaModifiedAt);

            note1.MetaModifiedAt = new DateTime(2030, 1, 1);
            Assert.IsNotNull(note1.MetaModifiedAt);
        }
    }
}
