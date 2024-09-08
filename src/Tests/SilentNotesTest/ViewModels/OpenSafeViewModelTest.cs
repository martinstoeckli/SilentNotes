using System;
using System.Security;
using Moq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SilentNotes;
using SilentNotes.Models;
using SilentNotes.Services;
using SilentNotes.ViewModels;
using VanillaCloudStorageClient;

namespace SilentNotesTest.ViewModels
{
    [TestClass]
    public class OpenSafeViewModelTest
    {
        [TestMethod]
        public void ResetSafe_RemovesNotesAndSafes()
        {
            Guid note1Id = new Guid("10000000000000000000000000000000");
            Guid note2Id = new Guid("20000000000000000000000000000000");
            Guid safeAId = new Guid("A0000000000000000000000000000000");
            Guid safeBId = new Guid("B0000000000000000000000000000000");
            NoteRepositoryModel repository = new NoteRepositoryModel();
            repository.Notes.Add(new NoteModel { Id = note1Id, SafeId = safeAId }); // locked, should be deleted
            repository.Notes.Add(new NoteModel { Id = note2Id, SafeId = null }); // unlocked, should be kept
            repository.Safes.Add(new SafeModel { Id = safeAId });
            repository.Safes.Add(new SafeModel { Id = safeBId });
            var keyService = CommonMocksAndStubs.SafeKeyService()
                .AddKey(safeAId, new byte[0])
                .AddKey(safeBId, new byte[0]);

            Mock<INavigationService> navigationService = new Mock<INavigationService>();
            Mock<IRepositoryStorageService> repositoryStorageService = new Mock<IRepositoryStorageService>();
            repositoryStorageService.
                Setup(m => m.LoadRepositoryOrDefault(out repository));

            OpenSafeViewModel viewModel = CreateMockedOpenSafeViewModel(
                repositoryStorageService.Object, keyService, navigationService.Object);

            viewModel.ResetSafeCommand.Execute(null);

            // Note is deleted and added to the deleted list
            Assert.AreEqual(1, repository.Notes.Count);
            Assert.AreEqual(note2Id, repository.Notes[0].Id);
            Assert.AreEqual(1, repository.DeletedNotes.Count);
            Assert.AreEqual(note1Id, repository.DeletedNotes[0]);

            // Safes are removed
            Assert.AreEqual(0, repository.Safes.Count);
            Assert.AreEqual(0, keyService.Count);

            // Is marked as modified and stored before navigating away
            repositoryStorageService.Verify(m => m.TrySaveRepository(It.IsAny<NoteRepositoryModel>()));
            navigationService.Verify(m => m.NavigateReload(), Times.Once);
        }

        [TestMethod]
        public void Ok_CreatesNewSafeAndMarksRepositoryAsModified()
        {
            Guid note1Id = new Guid("10000000000000000000000000000000");
            NoteRepositoryModel repository = new NoteRepositoryModel();
            repository.Notes.Add(new NoteModel { Id = note1Id });

            Mock<INavigationService> navigationService = new Mock<INavigationService>();
            Mock<IRepositoryStorageService> repositoryStorageService = new Mock<IRepositoryStorageService>();
            repositoryStorageService.
                Setup(m => m.LoadRepositoryOrDefault(out repository));
            var keyService = CommonMocksAndStubs.SafeKeyService();

            OpenSafeViewModel viewModel = CreateMockedOpenSafeViewModel(
                repositoryStorageService.Object, keyService, navigationService.Object);
            viewModel.Password = SecureStringExtensions.StringToSecureString("somethingvalid");
            viewModel.PasswordConfirmation = SecureStringExtensions.StringToSecureString("somethingvalid");

            Assert.IsFalse(viewModel.Modifications.IsModified());
            viewModel.OkCommand.Execute(null);
            Assert.IsTrue(viewModel.Modifications.IsModified());
            Assert.AreEqual(1, keyService.Count);
            navigationService.Verify(m => m.NavigateTo(It.Is<string>(r => r == RouteNames.NoteRepository), It.IsAny<bool>()), Times.Once);
        }

        private static OpenSafeViewModel CreateMockedOpenSafeViewModel(
            IRepositoryStorageService repositoryStorageService, ISafeKeyService keyService, INavigationService navigationService = null)
        {
            if (navigationService == null)
                navigationService = new Mock<INavigationService>().Object;
            SettingsModel settingsModel = new SettingsModel();
            Mock<ISettingsService> settingsService = new Mock<ISettingsService>();
            settingsService.
                Setup(m => m.LoadSettingsOrDefault()).Returns(settingsModel);

            return new OpenSafeViewModel(
                CommonMocksAndStubs.LanguageService(),
                navigationService,
                CommonMocksAndStubs.FeedbackService(),
                CommonMocksAndStubs.CryptoRandomService(),
                settingsService.Object,
                repositoryStorageService,
                keyService,
                null);
        }
    }
}
