using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using SilentNotes.Crypto;
using SilentNotes.Models;
using SilentNotes.Services;
using SilentNotes.ViewModels;
using SilentNotes.Workers;

namespace SilentNotesTest.ViewModels
{
    [TestFixture]
    public class NoteViewModelTest
    {
        [Test]
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
            NoteModel note = new NoteModel { Id = Guid.NewGuid(), HtmlContent = "secret", SafeId = safes[0].Id };

            NoteViewModel noteViewModel = new NoteViewModel(
                new Mock<INavigationService>().Object,
                new Mock<ILanguageService>().Object,
                new Mock<ISvgIconService>().Object,
                new Mock<IThemeService>().Object,
                new Mock<IBaseUrlService>().Object,
                searchableConverter,
                repositoryService.Object,
                new Mock<IFeedbackService>().Object,
                settingsService.Object,
                cryptor.Object,
                safes,
                new List<string>(),
                note);

            Assert.IsNull(noteViewModel.UnlockedHtmlContent);
            Assert.IsNull(noteViewModel.SearchableContent);
        }

        [Test]
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
                new SafeModel { Id = Guid.NewGuid(), Key = safeKey },
            });
            NoteModel note = new NoteModel { Id = Guid.NewGuid(), HtmlContent = modelHtmlContent, SafeId = safes[0].Id };

            NoteViewModel noteViewModel = new NoteViewModel(
                new Mock<INavigationService>().Object,
                new Mock<ILanguageService>().Object,
                new Mock<ISvgIconService>().Object,
                new Mock<IThemeService>().Object,
                new Mock<IBaseUrlService>().Object,
                searchableConverter,
                repositoryService.Object,
                new Mock<IFeedbackService>().Object,
                settingsService.Object,
                cryptor.Object,
                safes,
                new List<string>(),
                note);

            Assert.AreEqual("not secret anymore", noteViewModel.UnlockedHtmlContent);
            Assert.AreEqual("not secret anymore", noteViewModel.SearchableContent);
        }

        [Test]
        public void ModifiedNoteIsEncryptedAndStored()
        {
            string modelHtmlContent = "c2VjcmV0";
            byte[] safeKey = new byte[] { 88 };
            SettingsModel settingsModel = new SettingsModel();
            SearchableHtmlConverter searchableConverter = new SearchableHtmlConverter();
            Mock<IRepositoryStorageService> repositoryService = new Mock<IRepositoryStorageService>();
            Mock<ISettingsService> settingsService = new Mock<ISettingsService>();
            settingsService.
                Setup(m => m.LoadSettingsOrDefault()).Returns(settingsModel);
            Mock<ICryptor> cryptor = new Mock<ICryptor>();
            cryptor.
                Setup(m => m.Decrypt(It.IsAny<byte[]>(), It.Is<byte[]>(v => v == safeKey))).
                Returns(CryptoUtils.StringToBytes("not secret anymore"));

            SafeListModel safes = new SafeListModel(new[]
            {
                new SafeModel { Id = Guid.NewGuid(), Key = safeKey },
            });
            NoteModel note = new NoteModel { Id = Guid.NewGuid(), HtmlContent = modelHtmlContent, SafeId = safes[0].Id };

            NoteViewModel noteViewModel = new NoteViewModel(
                new Mock<INavigationService>().Object,
                new Mock<ILanguageService>().Object,
                new Mock<ISvgIconService>().Object,
                new Mock<IThemeService>().Object,
                new Mock<IBaseUrlService>().Object,
                searchableConverter,
                repositoryService.Object,
                new Mock<IFeedbackService>().Object,
                settingsService.Object,
                cryptor.Object,
                safes,
                new List<string>(),
                note);

            noteViewModel.UnlockedHtmlContent = "something new";
            noteViewModel.OnStoringUnsavedData();

            cryptor.Verify(m => m.Encrypt(
                It.Is<byte[]>(p => p.SequenceEqual(CryptoUtils.StringToBytes("something new"))),
                It.Is<byte[]>(p => p.SequenceEqual(safeKey)),
                It.IsAny<string>(),
                It.Is<string>(p => p == null)),
                Times.Once);
            repositoryService.Verify(m => m.TrySaveRepository(It.IsAny<NoteRepositoryModel>()), Times.Once);
        }

        [Test]
        public void AddTag_IsCaseInsensitive()
        {
            NoteModel note = new NoteModel { Id = Guid.NewGuid(), HtmlContent = "secret" };

            NoteViewModel noteViewModel = new NoteViewModel(
                new Mock<INavigationService>().Object,
                new Mock<ILanguageService>().Object,
                new Mock<ISvgIconService>().Object,
                new Mock<IThemeService>().Object,
                new Mock<IBaseUrlService>().Object,
                new SearchableHtmlConverter(),
                new Mock<IRepositoryStorageService>().Object,
                new Mock<IFeedbackService>().Object,
                new Mock<ISettingsService>().Object,
                new Mock<ICryptor>().Object,
                new SafeListModel(),
                new List<string>(),
                note);

            noteViewModel.AddTagCommand.Execute("Paddington");
            Assert.AreEqual(1, noteViewModel.Tags.Count);

            noteViewModel.AddTagCommand.Execute("paddington");
            Assert.AreEqual(1, noteViewModel.Tags.Count);

            noteViewModel.AddTagCommand.Execute("Sälüë");
            Assert.AreEqual(2, noteViewModel.Tags.Count);

            noteViewModel.AddTagCommand.Execute("SÄlÜË");
            Assert.AreEqual(2, noteViewModel.Tags.Count);
        }

        [Test]
        public void AddTag_IsAlwaysSorted()
        {
            NoteModel note = new NoteModel { Id = Guid.NewGuid(), HtmlContent = "secret" };

            NoteViewModel noteViewModel = new NoteViewModel(
                new Mock<INavigationService>().Object,
                new Mock<ILanguageService>().Object,
                new Mock<ISvgIconService>().Object,
                new Mock<IThemeService>().Object,
                new Mock<IBaseUrlService>().Object,
                new SearchableHtmlConverter(),
                new Mock<IRepositoryStorageService>().Object,
                new Mock<IFeedbackService>().Object,
                new Mock<ISettingsService>().Object,
                new Mock<ICryptor>().Object,
                new SafeListModel(),
                new List<string>(),
                note);

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

        [Test]
        public void TagSuggestions_ExcludeTagsOfOwnNote()
        {
            NoteModel note = new NoteModel { Id = Guid.NewGuid() };
            note.Tags.Add("Aaa");
            NoteViewModel noteViewModel = new NoteViewModel(
                new Mock<INavigationService>().Object,
                new Mock<ILanguageService>().Object,
                new Mock<ISvgIconService>().Object,
                new Mock<IThemeService>().Object,
                new Mock<IBaseUrlService>().Object,
                new SearchableHtmlConverter(),
                new Mock<IRepositoryStorageService>().Object,
                new Mock<IFeedbackService>().Object,
                new Mock<ISettingsService>().Object,
                new Mock<ICryptor>().Object,
                new SafeListModel(),
                new string[] {"aaa", "bbb"},
                note);

            List<string> suggestions = noteViewModel.TagSuggestions.ToList();
            Assert.AreEqual(1, suggestions.Count);
            Assert.AreEqual("bbb", suggestions[0]);
        }
    }
}
