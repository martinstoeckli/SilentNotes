using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SilentNotes.Models;
using SilentNotes.ViewModels;

namespace SilentNotesTest.ViewModels
{
    [TestClass]
    public class ExportViewModelTest
    {
        [TestMethod]
        public void EnumerateNotesToExport_ReturnsUnprotectedNotesOnly()
        {
            NoteRepositoryModel repository = CreateTestRepository();
            repository.Notes[0].SafeId = repository.Safes[0].Id;
            var keyService = CommonMocksAndStubs.SafeKeyService();

            var notes = ExportViewModel.EnumerateNotesToExport(repository, keyService, true, false).ToList();
            Assert.AreEqual(2, notes.Count);
            Assert.AreSame(repository.Notes[1], notes[0]);
            Assert.AreSame(repository.Notes[2], notes[1]);
        }

        [TestMethod]
        public void EnumerateNotesToExport_DoesNotReturnProtectedNoteBecauseSafeIsClosed()
        {
            NoteRepositoryModel repository = CreateTestRepository();
            var keyService = CommonMocksAndStubs.SafeKeyService();
            var notes = ExportViewModel.EnumerateNotesToExport(repository, keyService, true, true).ToList();
            Assert.AreEqual(2, notes.Count);
            Assert.AreSame(repository.Notes[1], notes[0]);
            Assert.AreSame(repository.Notes[2], notes[1]);
        }

        [TestMethod]
        public void EnumerateNotesToExport_ReturnsProtectedNotesOnly()
        {
            NoteRepositoryModel repository = CreateTestRepository();
            var keyService = CommonMocksAndStubs.SafeKeyService()
                .AddKey(repository.Safes[0].Id, new byte[] { 88 });
            var notes = ExportViewModel.EnumerateNotesToExport(repository, keyService, false, true).ToList();
            Assert.AreEqual(1, notes.Count);
            Assert.AreSame(repository.Notes[0], notes[0]);
        }

        private NoteRepositoryModel CreateTestRepository()
        {
            NoteRepositoryModel model = new NoteRepositoryModel();
            model.Id = new Guid("3538c76a-eee9-4905-adcf-946f8b527c37");
            model.Safes.Add(new SafeModel { Id = new Guid("543d7b84-db8b-4c2b-a9e2-6e105c686f26") });

            // Note inside safe
            model.Notes.Add(new NoteModel { Id = new Guid("6821aab9-d388-49f9-94a7-5ab366ca168e"), SafeId = model.Safes[0].Id });

            // Notes outside safe
            model.Notes.Add(new NoteModel { Id = new Guid("2ed4d12d-b1a8-4107-9bcf-736e80899465") });
            model.Notes.Add(new NoteModel { Id = new Guid("08e28535-88a8-4fc0-b6bb-3a5651cc594c") });

            // Deleted note
            model.DeletedNotes.Add(new Guid("c84e7eb9-f671-4b9f-a7e7-a013a5e1cef7"));
            return model;
        }
    }
}
