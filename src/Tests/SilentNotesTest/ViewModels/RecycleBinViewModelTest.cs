using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SilentNotes.Models;
using SilentNotes.Services;
using SilentNotes.ViewModels;

namespace SilentNotesTest.ViewModels
{
    [TestFixture]
    public class RecycleBinViewModelTest
    {
        [Test]
        public void RestoreNote_RemovesFromRecycledNotes()
        {
            NoteRepositoryModel model = CreateTestRepository();
            Guid idToRestore = new Guid("22222222-2222-2222-2222-222222222222");
            RecycleBinViewModel viewModel = CreateMockedRecycleBinViewModel(model);

            Assert.IsTrue(viewModel.RecycledNotes.Any(item => item.Id == idToRestore));

            viewModel.RestoreNoteCommand.Execute(idToRestore);

            Assert.IsFalse(viewModel.RecycledNotes.Any(item => item.Id == idToRestore));
            Assert.IsFalse(model.Notes.Find(item => item.Id == idToRestore).InRecyclingBin);
        }

        [Test]
        public void RestoreNote_MarksRepositoryAsModified()
        {
            NoteRepositoryModel model = CreateTestRepository();
            Guid idToRestore = new Guid("22222222-2222-2222-2222-222222222222");
            RecycleBinViewModel viewModel = CreateMockedRecycleBinViewModel(model);

            Assert.IsFalse(viewModel.Modifications.IsModified());
            viewModel.RestoreNoteCommand.Execute(idToRestore);
            Assert.IsTrue(viewModel.Modifications.IsModified());
        }

        [Test]
        public void EmptyRecycleBin_AddsAllToDeletedList()
        {
            NoteRepositoryModel model = CreateTestRepository();
            model.Notes[0].InRecyclingBin = true;
            model.Notes[1].InRecyclingBin = true;
            model.Notes[2].InRecyclingBin = false;
            RecycleBinViewModel viewModel = CreateMockedRecycleBinViewModel(model);

            viewModel.EmptyRecycleBinCommand.Execute(null);

            Assert.AreEqual(0, viewModel.RecycledNotes.Count); // recycle bin is empty now
            Assert.AreEqual(2, model.DeletedNotes.Count); // 2 notes moved from recycle bin to deleted
            Assert.AreEqual(1, model.Notes.Count); // 1 note remains because it was not in recycle bin
        }

        [Test]
        public void EmptyRecycleBin_MarksRepositoryAsModified()
        {
            NoteRepositoryModel model = CreateTestRepository();
            RecycleBinViewModel viewModel = CreateMockedRecycleBinViewModel(model);

            Assert.IsFalse(viewModel.Modifications.IsModified());
            viewModel.EmptyRecycleBinCommand.Execute(null);
            Assert.IsTrue(viewModel.Modifications.IsModified());
        }

        [Test]
        public void DeleteNotePermanently_AddsToDeletedList()
        {
            NoteRepositoryModel model = CreateTestRepository();
            model.Notes[0].InRecyclingBin = true;
            model.Notes[1].InRecyclingBin = true;
            model.Notes[2].InRecyclingBin = false;
            RecycleBinViewModel viewModel = CreateMockedRecycleBinViewModel(model);

            Guid idToDelete = new Guid("22222222-2222-2222-2222-222222222222");
            viewModel.DeleteNotePermanentlyCommand.Execute(idToDelete);

            Assert.AreEqual(1, viewModel.RecycledNotes.Count); // one note is still in recycle bin
            Assert.AreEqual(1, model.DeletedNotes.Count); // 1 note moved from recycle bin to deleted
            Assert.AreEqual(idToDelete, model.DeletedNotes[0]);
            Assert.AreEqual(2, model.Notes.Count);
        }

        [Test]
        public void DeleteNotePermanently_MarksRepositoryAsModified()
        {
            NoteRepositoryModel model = CreateTestRepository();
            Guid idToDelete = new Guid("22222222-2222-2222-2222-222222222222");
            RecycleBinViewModel viewModel = CreateMockedRecycleBinViewModel(model);

            Assert.IsFalse(viewModel.Modifications.IsModified());
            viewModel.DeleteNotePermanentlyCommand.Execute(idToDelete);
            Assert.IsTrue(viewModel.Modifications.IsModified());
        }

        private static RecycleBinViewModel CreateMockedRecycleBinViewModel(NoteRepositoryModel repository, ISafeKeyService keyService = null)
        {
            SettingsModel settingsModel = new SettingsModel { DefaultNoteInsertion = NoteInsertionMode.AtTop };
            Mock<ISettingsService> settingsService = new Mock<ISettingsService>();
            settingsService.
                Setup(m => m.LoadSettingsOrDefault()).Returns(settingsModel);
            if (keyService == null)
                keyService = CommonMocksAndStubs.SafeKeyService();

            return new RecycleBinViewModel(
                repository,
                new Mock<ILanguageService>().Object,
                new Mock<IThemeService>().Object,
                CommonMocksAndStubs.FeedbackService(),
                settingsService.Object,
                keyService,
                null);
        }

        private static NoteRepositoryModel CreateTestRepository()
        {
            NoteRepositoryModel model = new NoteRepositoryModel { Id = new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa") };
            model.Notes.Add(new NoteModel { Id = new Guid("11111111-1111-1111-1111-111111111111"), HtmlContent = "a" });
            model.Notes.Add(new NoteModel { Id = new Guid("22222222-2222-2222-2222-222222222222"), InRecyclingBin = true, HtmlContent = "a" });
            model.Notes.Add(new NoteModel { Id = new Guid("33333333-3333-3333-3333-333333333333"), HtmlContent = "b" });
            return model;
        }
    }
}
