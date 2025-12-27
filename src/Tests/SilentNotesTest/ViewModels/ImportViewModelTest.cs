using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SilentNotes.Models;
using SilentNotes.ViewModels;

namespace SilentNotesTest.ViewModels
{
    [TestClass]
    public class ImportViewModelTest
    {
        [TestMethod]
        public void LoadNotesFromImportedNoteList_WithStrategyIgnore_IgnoresNoteWithSameId()
        {
            NoteRepositoryModel repository = new NoteRepositoryModel();
            repository.Notes.Add(new NoteModel { Id = new Guid("4c85ba38aea8400982b74e53f37e27db"), HtmlContent = "content1" });

            var importNotes = new NoteListModel();
            importNotes.Add(new NoteModel { Id = new Guid("4c85ba38aea8400982b74e53f37e27db"), HtmlContent = "content2" });
            ImportViewModel.LoadNotesFromImportedNoteList(repository, importNotes, ImportStrategy.IgnoreExisting);

            Assert.AreEqual(1, repository.Notes.Count);
            Assert.AreEqual("content1", repository.Notes[0].HtmlContent);
        }

        [TestMethod]
        public void LoadNotesFromImportedNoteList_WithStrategyOverwrite_OverwritesNoteWithSameId()
        {
            NoteRepositoryModel repository = new NoteRepositoryModel();
            repository.Notes.Add(new NoteModel { Id = new Guid("4c85ba38aea8400982b74e53f37e27db"), HtmlContent = "content1" });

            var importNotes = new NoteListModel();
            importNotes.Add(new NoteModel { Id = new Guid("4c85ba38aea8400982b74e53f37e27db"), HtmlContent = "content2" });
            ImportViewModel.LoadNotesFromImportedNoteList(repository, importNotes, ImportStrategy.OverwriteExisting);

            Assert.AreEqual(1, repository.Notes.Count);
            Assert.AreEqual("content2", repository.Notes[0].HtmlContent);
        }

        [TestMethod]
        public void LoadNotesFromImportedNoteList_UpdatedTimeStamps()
        {
            NoteRepositoryModel repository = new NoteRepositoryModel();
            repository.Notes.Add(new NoteModel
            {
                Id = new Guid("4c85ba38aea8400982b74e53f37e27db"),
                ModifiedAt = new DateTime(1999, 01, 30),
            });

            var importNotes = new NoteListModel();
            importNotes.Add(new NoteModel
            {
                Id = new Guid("4c85ba38aea8400982b74e53f37e27db"),
                ModifiedAt = new DateTime(1999, 01, 01),
            });
            ImportViewModel.LoadNotesFromImportedNoteList(repository, importNotes, ImportStrategy.OverwriteExisting);

            Assert.IsTrue(TestExtensions.IsNearlyUtcNow(repository.Notes[0].ModifiedAt));
        }
    }
}
