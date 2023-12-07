using System;
using NUnit.Framework;
using SilentNotes.Models;

namespace SilentNotesTest.Models
{
    [TestFixture]
    public class NoteListModelTest
    {
        [Test]
        public void FindByIdReturnsCorrectElement()
        {
            Guid id1 = Guid.NewGuid();
            Guid id2 = Guid.NewGuid();
            NoteListModel notes = new NoteListModel();
            notes.Add(new NoteModel { Id = id1 });
            notes.Add(new NoteModel { Id = id2 });

            NoteModel foundNote = notes.FindById(id2);
            Assert.AreEqual(id2, foundNote.Id);
        }

        [Test]
        public void FindByIdReturnsNullIfNotFound()
        {
            Guid id1 = Guid.NewGuid();
            Guid id2 = Guid.NewGuid();
            NoteListModel notes = new NoteListModel();
            notes.Add(new NoteModel { Id = id1 });

            Assert.IsNull(notes.FindById(id2));
        }

        [Test]
        public void IndexOfByIdReturnsCorrectElement()
        {
            Guid id1 = Guid.NewGuid();
            Guid id2 = Guid.NewGuid();
            NoteListModel notes = new NoteListModel();
            notes.Add(new NoteModel { Id = id1 });
            notes.Add(new NoteModel { Id = id2 });

            int foundNote = notes.IndexOfById(id2);
            Assert.AreEqual(1, foundNote);
        }

        [Test]
        public void IndexOfByIdReturnsMinusIfNotFound()
        {
            Guid id1 = Guid.NewGuid();
            Guid id2 = Guid.NewGuid();
            NoteListModel notes = new NoteListModel();
            notes.Add(new NoteModel { Id = id1 });

            Assert.AreEqual(-1, notes.IndexOfById(id2));
        }

        [Test]
        public void IndexToInsertNewNoteAppendsNote()
        {
            List<NoteModel> notes = new NoteListModel();

            int res = NoteListModelExtensions.IndexToInsertNewNote(notes, NoteInsertionMode.AtBottom);
            Assert.AreEqual(0, res);

            Guid id1 = Guid.NewGuid();
            Guid id2 = Guid.NewGuid();
            notes.Add(new NoteModel { Id = id1 });
            notes.Add(new NoteModel { Id = id2 });
            res = notes.IndexToInsertNewNote(NoteInsertionMode.AtBottom);
            Assert.AreEqual(2, res);
        }

        [Test]
        public void IndexToInsertNewNoteInsertsAfterPinned()
        {
            List<NoteModel> notes = new NoteListModel();

            int res = NoteListModelExtensions.IndexToInsertNewNote(notes, NoteInsertionMode.AtTop);
            Assert.AreEqual(0, res);

            Guid id1 = Guid.NewGuid();
            Guid id2 = Guid.NewGuid();
            notes.Add(new NoteModel { Id = id1, IsPinned = true });
            notes.Add(new NoteModel { Id = id2 });
            res = notes.IndexToInsertNewNote(NoteInsertionMode.AtTop);
            Assert.AreEqual(1, res);

            notes.Clear();
            notes.Add(new NoteModel { Id = id1, IsPinned = true });
            notes.Add(new NoteModel { Id = id2, IsPinned = true });
            res = notes.IndexToInsertNewNote(NoteInsertionMode.AtTop);
            Assert.AreEqual(2, res);
        }
    }
}
