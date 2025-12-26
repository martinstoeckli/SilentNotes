using Microsoft.VisualStudio.TestTools.UnitTesting;
using SilentNotes.Models;

namespace SilentNotesTest.Models
{
    [TestClass]
    public class DeletedNoteModelTest
    {
        [TestMethod]
        public void FindByIdReturnsCorrectElement()
        {
            Guid id1 = Guid.NewGuid();
            Guid id2 = Guid.NewGuid();
            var notes = new DeletedNoteListModel();
            notes.Add(new DeletedNoteModel { Id = id1 });
            notes.Add(new DeletedNoteModel { Id = id2 });

            DeletedNoteModel foundNote = notes.FindById(id2);
            Assert.AreEqual(id2, foundNote.Id);
        }

        [TestMethod]
        public void AddIdOrRefreshDeletedAt_AddsNewId()
        {
            Guid id1 = Guid.NewGuid();
            var notes = new DeletedNoteListModel();
            notes.AddIdOrRefreshDeletedAt(id1);

            Assert.IsNotNull(notes.FindById(id1));
        }

        [TestMethod]
        public void AddIdOrRefreshDeletedAt_UpdatedDateOfExistingId()
        {
            Guid id1 = Guid.NewGuid();
            var notes = new DeletedNoteListModel();
            notes.Add(new DeletedNoteModel
            {
                Id = id1,
                DeletedAt = new DateTime(1999, 01, 01)
            });

            notes.AddIdOrRefreshDeletedAt(id1);
            Assert.AreEqual(1, notes.Count); // no new note was added
            Assert.IsTrue(DateTime.UtcNow - notes.FindById(id1).DeletedAt < TimeSpan.FromSeconds(1)); // timestamp is now
        }
    }
}
