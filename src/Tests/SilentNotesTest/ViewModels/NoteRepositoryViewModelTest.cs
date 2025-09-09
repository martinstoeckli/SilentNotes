using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SilentNotes.Crypto;
using SilentNotes.Models;
using SilentNotes.Services;
using SilentNotes.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using SilentNotes;

namespace SilentNotesTest.ViewModels
{
    [TestClass]
    public class NoteRepositoryViewModelTest
    {
        [TestMethod]
        public void NewNote_MarksRepositoryAsModified()
        {
            NoteRepositoryModel model = CreateTestRepository();
            NoteRepositoryViewModel viewModel = CreateMockedNoteRepositoryViewModel(model);
            Assert.IsFalse(viewModel.Modifications.IsModified());
            viewModel.NewNoteCommand.Execute(null);
            Assert.IsTrue(viewModel.Modifications.IsModified());
        }

        [TestMethod]
        public void NewNote_IsAddedAfterLastPinned()
        {
            NoteRepositoryModel model = CreateTestRepository();
            var oldNotes = new List<NoteModel>(model.Notes);
            model.Notes[0].IsPinned = true;

            NoteRepositoryViewModel viewModel = CreateMockedNoteRepositoryViewModel(model);
            viewModel.Filter = "b";
            viewModel.NewNoteCommand.Execute(null);

            // New note is at position 1, after first pinned note
            NoteModel newNote = model.Notes[1];
            Assert.IsFalse(oldNotes.Contains(newNote));
        }

        [TestMethod]
        public void NewNote_IsAddedAsFirstIfNoOtherNotesExist()
        {
            NoteRepositoryModel model = new NoteRepositoryModel();

            NoteRepositoryViewModel viewModel = CreateMockedNoteRepositoryViewModel(model);
            viewModel.NewNoteCommand.Execute(null);

            // New note is at position 1, after first pinned note
            Assert.AreEqual(1, model.Notes.Count);
            Assert.IsFalse(model.Notes[0].IsPinned);
        }

        [TestMethod]
        public void NewNote_IsAddedAsLastIfAllNotesArePinned()
        {
            NoteRepositoryModel model = CreateTestRepository();
            var oldNotes = new List<NoteModel>(model.Notes);
            model.Notes[0].IsPinned = true;
            model.Notes[1].IsPinned = true;
            model.Notes[2].IsPinned = true;

            NoteRepositoryViewModel viewModel = CreateMockedNoteRepositoryViewModel(model);
            viewModel.NewNoteCommand.Execute(null);

            // New note is at position 1, after first pinned note
            NoteModel newNote = model.Notes.Last();
            Assert.IsFalse(oldNotes.Contains(newNote));
        }

        [TestMethod]
        public void NewNote_RecyclingStateIsIgnored()
        {
            NoteRepositoryModel model = CreateTestRepository();
            var oldNotes = new List<NoteModel>(model.Notes);
            model.Notes[0].IsPinned = true;
            model.Notes[0].InRecyclingBin = true;

            NoteRepositoryViewModel viewModel = CreateMockedNoteRepositoryViewModel(model);
            viewModel.NewNoteCommand.Execute(null);

            // New note is at position 1, after first pinned note, even if it is in the recycle bin
            NoteModel newNote = model.Notes[1];
            Assert.IsFalse(oldNotes.Contains(newNote));
        }

        [TestMethod]
        public void DeleteNote_MarksRepositoryAsModified()
        {
            NoteRepositoryModel model = CreateTestRepository();
            NoteRepositoryViewModel viewModel = CreateMockedNoteRepositoryViewModel(model);

            Guid idToDelete = new Guid("22222222-2222-2222-2222-222222222222");
            Assert.IsFalse(viewModel.Modifications.IsModified());
            viewModel.DeleteNoteCommand.Execute(idToDelete);
            Assert.IsTrue(viewModel.Modifications.IsModified());
        }

        [TestMethod]
        public void DeleteNote_MarksNoteAsDeleted()
        {
            NoteRepositoryModel model = CreateTestRepository();
            NoteRepositoryViewModel viewModel = CreateMockedNoteRepositoryViewModel(model);

            Guid idToDelete = new Guid("22222222-2222-2222-2222-222222222222");
            viewModel.DeleteNoteCommand.Execute(idToDelete);

            var deletedNote = model.Notes.FindById(idToDelete);
            Assert.AreEqual(true, deletedNote.InRecyclingBin);
        }

        [TestMethod]
        public void DeleteNote_RemovedFromFilteredList()
        {
            NoteRepositoryModel model = CreateTestRepository();
            NoteRepositoryViewModel viewModel = CreateMockedNoteRepositoryViewModel(model);

            Guid idToDelete = new Guid("22222222-2222-2222-2222-222222222222");
            NoteViewModelReadOnly noteToDelete = viewModel.FilteredNotes.First(item => item.Id == idToDelete);

            viewModel.DeleteNoteCommand.Execute(idToDelete);

            Assert.IsFalse(viewModel.FilteredNotes.Contains(noteToDelete));
        }

        [TestMethod]
        public async Task DeleteNote_RemovedFromTagTree()
        {
            NoteRepositoryModel model = CreateTestRepository();
            Guid idToDelete = new Guid("22222222-2222-2222-2222-222222222222");
            var noteToDelete = model.Notes.FindById(idToDelete);
            noteToDelete.Tags.Add("fox");
            NoteRepositoryViewModel viewModel = CreateMockedNoteRepositoryViewModel(model);
            await viewModel.InitializeTagTree();

            Assert.IsTrue(viewModel.TagsRootNode.EnumerateSiblingsRecursive(false).Any(node => node.Title == "fox"));
            viewModel.DeleteNoteCommand.Execute(idToDelete);
            Assert.IsFalse(viewModel.TagsRootNode.EnumerateSiblingsRecursive(false).Any(node => node.Title == "fox"));
        }

        [TestMethod]
        public void AddNoteToSafe_MarksRepositoryAsModified()
        {
            NoteRepositoryModel model = CreateTestRepository();
            Guid noteId = new Guid("22222222-2222-2222-2222-222222222222");

            byte[] key = CommonMocksAndStubs.CryptoRandomService().GetRandomBytes(32);
            model.Safes.Add(new SafeModel());
            var keyService = CommonMocksAndStubs.SafeKeyService()
                .AddKey(model.Safes[0].Id, key); // safe must already be open

            NoteRepositoryViewModel viewModel = CreateMockedNoteRepositoryViewModel(model, keyService);
            viewModel.Modifications.MemorizeCurrentState();

            Assert.IsFalse(viewModel.Modifications.IsModified());
            viewModel.AddNoteToSafe(noteId);
            Assert.IsTrue(viewModel.Modifications.IsModified());
        }

        [TestMethod]
        public void RemoveNoteFromSafe_MarksRepositoryAsModified()
        {
            NoteRepositoryModel model = CreateTestRepository();
            Guid noteId = new Guid("22222222-2222-2222-2222-222222222222");
            NoteModel note = model.Notes.FindById(noteId);

            model.Safes.Add(new SafeModel());
            note.SafeId = model.Safes[0].Id;
            var keyService = CommonMocksAndStubs.SafeKeyService();

            NoteRepositoryViewModel viewModel = CreateMockedNoteRepositoryViewModel(model, keyService);
            viewModel.Modifications.MemorizeCurrentState();

            Assert.IsFalse(viewModel.Modifications.IsModified());
            viewModel.RemoveNoteFromSafe(noteId);
            Assert.IsNull(note.SafeId);
            Assert.IsTrue(viewModel.Modifications.IsModified());
        }

        [TestMethod]
        public void MoveSelectedOrderNote_MarksRepositoryAsModified()
        {
            NoteRepositoryModel model = CreateTestRepository();
            NoteRepositoryViewModel viewModel = CreateMockedNoteRepositoryViewModel(model);
            viewModel.SelectOrderNote(viewModel.FilteredNotes[0]);

            Assert.IsFalse(viewModel.Modifications.IsModified());
            viewModel.MoveSelectedOrderNote(false, true);
            Assert.IsTrue(viewModel.Modifications.IsModified());
        }

        private static NoteRepositoryViewModel CreateMockedNoteRepositoryViewModel(NoteRepositoryModel repository, ISafeKeyService keyService = null)
        {
            SettingsModel settingsModel = new SettingsModel { DefaultNoteInsertion = NoteInsertionMode.AtTop };
            Mock<ISettingsService> settingsService = new Mock<ISettingsService>();
            settingsService.
                Setup(m => m.LoadSettingsOrDefault()).Returns(settingsModel);
            if (keyService == null)
                keyService = CommonMocksAndStubs.SafeKeyService();

            return new NoteRepositoryViewModel(
                repository,
                new Mock<ILanguageService>().Object,
                new Mock<INavigationService>().Object,
                CommonMocksAndStubs.FeedbackService(),
                new Mock<IThemeService>().Object,
                settingsService.Object,
                CommonMocksAndStubs.EnvironmentService(),
                new Mock<IFolderPickerService>().Object,
                new Mock<IFilePickerService>().Object,
                new Mock<ISynchronizationService>().Object,
                CommonMocksAndStubs.CryptoRandomService(),
                null,
                keyService,
                new Mock<IMessengerService>().Object);
        }

        private static NoteRepositoryModel CreateTestRepository()
        {
            NoteRepositoryModel model = new NoteRepositoryModel { Id = new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa") };
            model.Notes.Add(new NoteModel { Id = new Guid("11111111-1111-1111-1111-111111111111"), HtmlContent = "a" });
            model.Notes.Add(new NoteModel { Id = new Guid("22222222-2222-2222-2222-222222222222"), HtmlContent = "a" });
            model.Notes.Add(new NoteModel { Id = new Guid("33333333-3333-3333-3333-333333333333"), HtmlContent = "b" });
            return model;
        }
    }
}
