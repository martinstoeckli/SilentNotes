using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SilentNotes;
using SilentNotes.Crypto;
using SilentNotes.Models;
using SilentNotes.Services;
using SilentNotes.ViewModels;
using SilentNotes.Workers;

namespace SilentNotesTest.ViewModels
{
    [TestClass]
    public class NoteViewModelTest
    {
        [TestMethod]
        public void UsesNotFoundNote_WhenConstructorGetsNullModel()
        {
            NoteModel inexistingModel = null;
            NoteViewModel viewModel = CreateMockedNoteViewModel(inexistingModel);
            Assert.AreSame(NoteModel.NotFound, viewModel.Model);
        }

        [TestMethod]
        public void DoesNotShowContentWhenSafeIsClosed()
        {
            SearchableHtmlConverter searchableConverter = new SearchableHtmlConverter();
            Mock<IRepositoryStorageService> repositoryService = new Mock<IRepositoryStorageService>();
            Mock<ISettingsService> settingsService = new Mock<ISettingsService>();
            Mock<ICryptor> cryptor = new Mock<ICryptor>();

            SafeListModel safes = new SafeListModel(new[]
            {
                new SafeModel { Id = Guid.NewGuid() },
            });
            var keyService = CommonMocksAndStubs.SafeKeyService();
            NoteModel note = new NoteModel { Id = Guid.NewGuid(), HtmlContent = "secret", SafeId = safes[0].Id };

            var noteViewModel = new NoteViewModelReadOnly(
                note,
                searchableConverter,
                new Mock<IThemeService>().Object,
                settingsService.Object,
                keyService,
                cryptor.Object,
                safes);

            Assert.IsNull(noteViewModel.UnlockedHtmlContent);
            Assert.IsNull(noteViewModel.SearchableContent);
        }

        [TestMethod]
        public void ShowsUnlockedContentWhenSafeIsOpen()
        {
            string modelHtmlContent = "c2VjcmV0";
            byte[] safeKey = new byte[] { 88 };
            SearchableHtmlConverter searchableConverter = new SearchableHtmlConverter();
            Mock<IRepositoryStorageService> repositoryService = new Mock<IRepositoryStorageService>();
            Mock<ISettingsService> settingsService = new Mock<ISettingsService>();
            Mock<ICryptor> cryptor = new Mock<ICryptor>();
            cryptor.
                Setup(m => m.Decrypt(It.IsAny<byte[]>(), It.Is<byte[]>(v => v == safeKey))).
                Returns(CryptoUtils.StringToBytes("not secret anymore"));

            SafeListModel safes = new SafeListModel(new[]
            {
                new SafeModel { Id = Guid.NewGuid() },
            });
            var keyService = CommonMocksAndStubs.SafeKeyService()
                .AddKey(safes[0].Id, safeKey);
            NoteModel note = new NoteModel { Id = Guid.NewGuid(), HtmlContent = modelHtmlContent, SafeId = safes[0].Id };

            var noteViewModel = new NoteViewModelReadOnly(
                note,
                searchableConverter,
                new Mock<IThemeService>().Object,
                settingsService.Object,
                keyService,
                cryptor.Object,
                safes);

            Assert.AreEqual("not secret anymore", noteViewModel.UnlockedHtmlContent);
            Assert.AreEqual("not secret anymore", noteViewModel.SearchableContent);
        }

        [TestMethod]
        public void ModifiedNoteIsEncryptedAndStored()
        {
            string modelHtmlContent = "c2VjcmV0";
            byte[] safeKey = new byte[] { 88 };
            SettingsModel settingsModel = new SettingsModel();
            SearchableHtmlConverter searchableConverter = new SearchableHtmlConverter();
            NoteRepositoryModel repositoryModel = new NoteRepositoryModel();
            Mock<IRepositoryStorageService> repositoryService = CommonMocksAndStubs.RepositoryStorageServiceMock(repositoryModel);
            Mock<ISettingsService> settingsService = new Mock<ISettingsService>();
            settingsService.
                Setup(m => m.LoadSettingsOrDefault()).Returns(settingsModel);
            Mock<ICryptor> cryptor = new Mock<ICryptor>();
            cryptor.
                Setup(m => m.Decrypt(It.IsAny<byte[]>(), It.Is<byte[]>(v => v == safeKey))).
                Returns(CryptoUtils.StringToBytes("not secret anymore"));

            SafeListModel safes = new SafeListModel(new[]
            {
                new SafeModel { Id = Guid.NewGuid() },
            });
            var keyService = CommonMocksAndStubs.SafeKeyService()
                .AddKey(safes[0].Id, safeKey);
            NoteModel note = new NoteModel { Id = Guid.NewGuid(), HtmlContent = modelHtmlContent, SafeId = safes[0].Id };
            repositoryModel.Notes.Add(note);

            NoteViewModel noteViewModel = new NoteViewModel(
                note,
                searchableConverter,
                new Mock<INavigationService>().Object,
                new Mock<ILanguageService>().Object,
                new Mock<IThemeService>().Object,
                repositoryService.Object,
                new Mock<IFeedbackService>().Object,
                settingsService.Object,
                keyService,
                new Mock<IEnvironmentService>().Object,
                new Mock<INativeBrowserService>().Object,
                new Mock<ISynchronizationState>().Object,
                cryptor.Object,
                safes,
                new List<string>());

            noteViewModel.UnlockedHtmlContent = "something new";
            noteViewModel.OnStoringUnsavedData(new StoreUnsavedDataMessage());

            cryptor.Verify(m => m.Encrypt(
                It.Is<byte[]>(p => p.SequenceEqual(CryptoUtils.StringToBytes("something new"))),
                It.Is<byte[]>(p => p.SequenceEqual(safeKey)),
                It.IsAny<string>(),
                It.Is<string>(p => p == null)),
                Times.Once);
            repositoryService.Verify(m => m.TrySaveRepository(It.IsAny<NoteRepositoryModel>()), Times.Once);
        }

        [TestMethod]
        public void AddTag_IsCaseInsensitive()
        {
            NoteModel note = new NoteModel { Id = Guid.NewGuid(), HtmlContent = "secret" };

            NoteViewModel noteViewModel = new NoteViewModel(
                note,
                new SearchableHtmlConverter(),
                new Mock<INavigationService>().Object,
                new Mock<ILanguageService>().Object,
                new Mock<IThemeService>().Object,
                new Mock<IRepositoryStorageService>().Object,
                new Mock<IFeedbackService>().Object,
                new Mock<ISettingsService>().Object,
                CommonMocksAndStubs.SafeKeyService(),
                new Mock<IEnvironmentService>().Object,
                new Mock<INativeBrowserService>().Object,
                new Mock<ISynchronizationState>().Object,
                new Mock<ICryptor>().Object,
                new SafeListModel(),
                new List<string>());

            noteViewModel.AddTagCommand.Execute("Paddington");
            Assert.AreEqual(1, noteViewModel.Tags.Count);

            noteViewModel.AddTagCommand.Execute("paddington");
            Assert.AreEqual(1, noteViewModel.Tags.Count);

            noteViewModel.AddTagCommand.Execute("Sälüë");
            Assert.AreEqual(2, noteViewModel.Tags.Count);

            noteViewModel.AddTagCommand.Execute("SÄlÜË");
            Assert.AreEqual(2, noteViewModel.Tags.Count);
        }

        [TestMethod]
        public void AddTag_IsAlwaysSorted()
        {
            NoteModel note = new NoteModel { Id = Guid.NewGuid(), HtmlContent = "secret" };

            NoteViewModel noteViewModel = new NoteViewModel(
                note,
                new SearchableHtmlConverter(),
                new Mock<INavigationService>().Object,
                new Mock<ILanguageService>().Object,
                new Mock<IThemeService>().Object,
                new Mock<IRepositoryStorageService>().Object,
                new Mock<IFeedbackService>().Object,
                new Mock<ISettingsService>().Object,
                CommonMocksAndStubs.SafeKeyService(),
                new Mock<IEnvironmentService>().Object,
                new Mock<INativeBrowserService>().Object,
                new Mock<ISynchronizationState>().Object,
                new Mock<ICryptor>().Object,
                new SafeListModel(),
                new List<string>());

            noteViewModel.AddTagCommand.Execute("abc");
            noteViewModel.AddTagCommand.Execute("def");
            noteViewModel.AddTagCommand.Execute("äbc");
            noteViewModel.AddTagCommand.Execute("Bcde");
            noteViewModel.AddTagCommand.Execute("abcd");
            noteViewModel.AddTagCommand.Execute("ç");
            noteViewModel.AddTagCommand.Execute("E");
            noteViewModel.AddTagCommand.Execute("é");
            noteViewModel.AddTagCommand.Execute("f");
            Assert.AreEqual("abc", noteViewModel.Tags[0]);
            Assert.AreEqual("äbc", noteViewModel.Tags[1]);
            Assert.AreEqual("abcd", noteViewModel.Tags[2]);
            Assert.AreEqual("Bcde", noteViewModel.Tags[3]);
            Assert.AreEqual("ç", noteViewModel.Tags[4]);
            Assert.AreEqual("def", noteViewModel.Tags[5]);
            Assert.AreEqual("E", noteViewModel.Tags[6]);
            Assert.AreEqual("é", noteViewModel.Tags[7]);
            Assert.AreEqual("f", noteViewModel.Tags[8]);
        }

        [TestMethod]
        public void TagSuggestions_ExcludeTagsOfOwnNote()
        {
            NoteModel note = new NoteModel { Id = Guid.NewGuid() };
            note.Tags.Add("Aaa");
            NoteViewModel noteViewModel = new NoteViewModel(
                note,
                new SearchableHtmlConverter(),
                new Mock<INavigationService>().Object,
                new Mock<ILanguageService>().Object,
                new Mock<IThemeService>().Object,
                new Mock<IRepositoryStorageService>().Object,
                new Mock<IFeedbackService>().Object,
                new Mock<ISettingsService>().Object,
                CommonMocksAndStubs.SafeKeyService(),
                new Mock<IEnvironmentService>().Object,
                new Mock<INativeBrowserService>().Object,
                new Mock<ISynchronizationState>().Object,
                new Mock<ICryptor>().Object,
                new SafeListModel(),
                new string[] { "aaa", "bbb" });

            List<string> suggestions = noteViewModel.TagSuggestions.ToList();
            Assert.AreEqual(1, suggestions.Count);
            Assert.AreEqual("bbb", suggestions[0]);
        }

        [TestMethod]
        public void TogglePinned_ChangesPinState()
        {
            NoteModel note = new NoteModel { Id = Guid.NewGuid() };
            NoteViewModel noteViewModel = CreateMockedNoteViewModel(note);

            Assert.IsFalse(note.IsPinned);
            noteViewModel.TogglePinnedCommand.Execute(null);
            Assert.IsTrue(note.IsPinned);
            noteViewModel.TogglePinnedCommand.Execute(null);
            Assert.IsFalse(note.IsPinned);
        }

        [TestMethod]
        public void TogglePinned_CalledTwice_DoesNotChangeNotePosition()
        {
            NoteRepositoryModel repository = CreateTestRepository();
            NoteModel noteToBePinned = repository.Notes[1];
            NoteViewModel noteViewModel = CreateMockedNoteViewModel(noteToBePinned, repository);

            noteViewModel.TogglePinnedCommand.Execute(null);
            noteViewModel.TogglePinnedCommand.Execute(null);
            noteViewModel.OnStoringUnsavedData(new StoreUnsavedDataMessage());

            Assert.AreSame(noteToBePinned, repository.Notes[1]); // No modification, no change of position
            Assert.IsFalse(noteToBePinned.IsPinned);
        }

        [TestMethod]
        public void TogglePinned_CalledTwice_DoesNotStoreRepository()
        {
            NoteModel noteToBePinned = new NoteModel { Id = new Guid("11111111-1111-1111-1111-111111111111") };
            Mock<IRepositoryStorageService> repositoryStorageService = new Mock<IRepositoryStorageService>();
            NoteViewModel noteViewModel = CreateMockedNoteViewModel(noteToBePinned, repositoryStorageService.Object);

            noteViewModel.TogglePinnedCommand.Execute(null);
            noteViewModel.TogglePinnedCommand.Execute(null);
            noteViewModel.OnStoringUnsavedData(new StoreUnsavedDataMessage());

            repositoryStorageService.Verify(m => m.TrySaveRepository(It.IsAny<NoteRepositoryModel>()), Times.Never);
        }

        [TestMethod]
        public void IsPinned_SetToTrue_MovesNotePositionToTop()
        {
            NoteRepositoryModel repository = CreateTestRepository();
            NoteModel noteToBePinned = repository.Notes[1];
            NoteViewModel noteViewModel = CreateMockedNoteViewModel(noteToBePinned, repository);

            noteViewModel.IsPinned = true;
            noteViewModel.OnStoringUnsavedData(new StoreUnsavedDataMessage());

            Assert.AreSame(noteToBePinned, repository.Notes[0]); // Now on first position
            Assert.IsTrue(noteToBePinned.IsPinned);
        }

        [TestMethod]
        public void IsPinned_SetToFalse_MovesNotePositionPastLastPinned()
        {
            NoteRepositoryModel repository = CreateTestRepository();
            repository.Notes[0].IsPinned = true;
            repository.Notes[1].IsPinned = true;
            NoteModel noteToBeUnpinned = repository.Notes[0];
            NoteViewModel noteViewModel = CreateMockedNoteViewModel(noteToBeUnpinned, repository);

            noteViewModel.IsPinned = false;
            noteViewModel.OnStoringUnsavedData(new StoreUnsavedDataMessage());

            Assert.AreSame(noteToBeUnpinned, repository.Notes[1]); // Now on middle position
            Assert.IsFalse(noteToBeUnpinned.IsPinned);
        }

        [TestMethod]
        public void IsPinned_SetToFalse_WorksWithAllNotesPinned()
        {
            NoteRepositoryModel repository = CreateTestRepository();
            repository.Notes[0].IsPinned = true;
            repository.Notes[1].IsPinned = true;
            repository.Notes[2].IsPinned = true;
            NoteModel noteToBeUnpinned = repository.Notes[0];
            NoteViewModel noteViewModel = CreateMockedNoteViewModel(noteToBeUnpinned, repository);

            noteViewModel.IsPinned = false;
            noteViewModel.OnStoringUnsavedData(new StoreUnsavedDataMessage());

            Assert.AreSame(noteToBeUnpinned, repository.Notes[2]); // Now on last position
            Assert.IsFalse(noteToBeUnpinned.IsPinned);
        }

        [TestMethod]
        public void IsPinned_PositionChange_StoresPosition()
        {
            NoteRepositoryModel repository = CreateTestRepository();
            var testNote = repository.Notes[2];
            NoteViewModel noteViewModel = CreateMockedNoteViewModel(testNote, repository);

            var original = repository.OrderModifiedAt;

            testNote.IsPinned = true; //should be moved to index 0 and refresh OrderModifiedAt

            noteViewModel.OnStoringUnsavedData(new StoreUnsavedDataMessage());

            Assert.IsTrue(repository.OrderModifiedAt != original);
        }

        [TestMethod]
        public void IsPinned_NoPositionChange_NOTStoresPosition()
        {
            NoteRepositoryModel repository = CreateTestRepository();
            var testNote = repository.Notes[0];
            NoteViewModel noteViewModel = CreateMockedNoteViewModel(testNote, repository);

            var original = repository.OrderModifiedAt;

            testNote.IsPinned = true; //stays on indey 0 and does NOT refresh OrderModifiedAt

            noteViewModel.OnStoringUnsavedData(new StoreUnsavedDataMessage());

            Assert.IsTrue(repository.OrderModifiedAt == original);
        }

        [TestMethod]
        public void IsPinned_SetToFalse_WorksWithOnlySingleNote()
        {
            NoteRepositoryModel repository = new NoteRepositoryModel { Id = new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa") };
            repository.Notes.Add(new NoteModel { Id = new Guid("11111111-1111-1111-1111-111111111111") });

            NoteModel noteToBeUnpinned = repository.Notes[0];
            NoteViewModel noteViewModel = CreateMockedNoteViewModel(noteToBeUnpinned, repository);

            noteViewModel.IsPinned = false;
            noteViewModel.OnStoringUnsavedData(new StoreUnsavedDataMessage());

            Assert.AreSame(noteToBeUnpinned, repository.Notes[0]); // Now on last position
        }

        private static NoteViewModel CreateMockedNoteViewModel(NoteModel note)
        {
            Mock<IRepositoryStorageService> repositoryStorageService = new Mock<IRepositoryStorageService>();
            return CreateMockedNoteViewModel(note, repositoryStorageService.Object);
        }

        private static NoteViewModel CreateMockedNoteViewModel(NoteModel note, NoteRepositoryModel repository)
        {
            Mock<IRepositoryStorageService> repositoryStorageService = new Mock<IRepositoryStorageService>();
            repositoryStorageService.
                Setup(m => m.LoadRepositoryOrDefault(out repository));
            return CreateMockedNoteViewModel(note, repositoryStorageService.Object);
        }

        private static NoteViewModel CreateMockedNoteViewModel(NoteModel note, IRepositoryStorageService repositoryStorageService)
        {
            return new NoteViewModel(
                note,
                new SearchableHtmlConverter(),
                new Mock<INavigationService>().Object,
                new Mock<ILanguageService>().Object,
                new Mock<IThemeService>().Object,
                repositoryStorageService,
                new Mock<IFeedbackService>().Object,
                new Mock<ISettingsService>().Object,
                CommonMocksAndStubs.SafeKeyService(),
                new Mock<IEnvironmentService>().Object,
                new Mock<INativeBrowserService>().Object,
                new Mock<ISynchronizationState>().Object,
                new Mock<ICryptor>().Object,
                new SafeListModel(),
                new string[] { });
        }

        private static NoteRepositoryModel CreateTestRepository()
        {
            NoteRepositoryModel model = new NoteRepositoryModel { Id = new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa") };
            model.Notes.Add(new NoteModel { Id = new Guid("11111111-1111-1111-1111-111111111111") });
            model.Notes.Add(new NoteModel { Id = new Guid("22222222-2222-2222-2222-222222222222") });
            model.Notes.Add(new NoteModel { Id = new Guid("33333333-3333-3333-3333-333333333333") });
            return model;
        }
    }
}