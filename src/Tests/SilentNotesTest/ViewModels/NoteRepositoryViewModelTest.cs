using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using SilentNotes.Models;
using SilentNotes.Services;
using SilentNotes.ViewModels;

namespace SilentNotesTest.ViewModels
{
    [TestFixture]
    public class NoteRepositoryViewModelTest
    {
        [Test]
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

        [Test]
        public void NewNote_IsAddedAsFirstIfNoOtherNotesExist()
        {
            NoteRepositoryModel model = new NoteRepositoryModel();

            NoteRepositoryViewModel viewModel = CreateMockedNoteRepositoryViewModel(model);
            viewModel.NewNoteCommand.Execute(null);

            // New note is at position 1, after first pinned note
            Assert.AreEqual(1, model.Notes.Count);
            Assert.IsFalse(model.Notes[0].IsPinned);
        }

        [Test]
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

        [Test]
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

        private static NoteRepositoryViewModel CreateMockedNoteRepositoryViewModel(NoteRepositoryModel repository)
        {
            SettingsModel settingsModel = new SettingsModel { DefaultNoteInsertion = NoteInsertionMode.AtTop };
            Mock<ISettingsService> settingsService = new Mock<ISettingsService>();
            settingsService.
                Setup(m => m.LoadSettingsOrDefault()).Returns(settingsModel);

            return new NoteRepositoryViewModel(
                repository,
                new Mock<ILanguageService>().Object,
                new Mock<INavigationService>().Object,
                new Mock<IFeedbackService>().Object,
                new Mock<IThemeService>().Object,
                settingsService.Object,
                CommonMocksAndStubs.EnvironmentService(),
                CommonMocksAndStubs.CryptoRandomService()
                );
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
