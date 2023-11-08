using System;
using System.Security;
using Moq;
using NUnit.Framework;
using SilentNotes;
using SilentNotes.Models;
using SilentNotes.Services;
using SilentNotes.ViewModels;
using VanillaCloudStorageClient;

namespace SilentNotesTest.ViewModels
{
    [TestFixture]
    public class OpenSafeViewModelTest
    {
        [Test]
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

            Mock<INavigationService> navigationService = new Mock<INavigationService>();
            Mock<IRepositoryStorageService> repositoryStorageService = new Mock<IRepositoryStorageService>();
            repositoryStorageService.
                Setup(m => m.LoadRepositoryOrDefault(out repository));

            OpenSafeViewModel viewModel = CreateMockedOpenSafeViewModelViewModel(
                repositoryStorageService.Object, navigationService.Object);

            viewModel.ResetSafeCommand.Execute(null);

            // Note is deleted and added to the deleted list
            Assert.AreEqual(1, repository.Notes.Count);
            Assert.AreEqual(note2Id, repository.Notes[0].Id);
            Assert.AreEqual(1, repository.DeletedNotes.Count);
            Assert.AreEqual(note1Id, repository.DeletedNotes[0]);

            // Safes are removed
            Assert.AreEqual(0, repository.Safes.Count);

            // Is marked as modified and stored before navigating away
            repositoryStorageService.Verify(m => m.TrySaveRepository(It.IsAny<NoteRepositoryModel>()));
            navigationService.Verify(m => m.NavigateTo(It.Is<string>(route => route == Routes.OpenSafe), It.Is<bool>(remove => remove == true)));
        }

        [Test]
        public void Ok_CreatesNewSafeAndMarksRepositoryAsModified()
        {
            Guid note1Id = new Guid("10000000000000000000000000000000");
            NoteRepositoryModel repository = new NoteRepositoryModel();
            repository.Notes.Add(new NoteModel { Id = note1Id });

            Mock<INavigationService> navigationService = new Mock<INavigationService>();
            Mock<IRepositoryStorageService> repositoryStorageService = new Mock<IRepositoryStorageService>();
            repositoryStorageService.
                Setup(m => m.LoadRepositoryOrDefault(out repository));

            OpenSafeViewModel viewModel = CreateMockedOpenSafeViewModelViewModel(
                repositoryStorageService.Object, navigationService.Object);
            viewModel.Password = SecureStringExtensions.StringToSecureString("somethingvalid");
            viewModel.PasswordConfirmation = SecureStringExtensions.StringToSecureString("somethingvalid");

            Assert.IsFalse(viewModel.Modifications.IsModified());
            viewModel.OkCommand.Execute(null);
            Assert.IsTrue(viewModel.Modifications.IsModified());
            navigationService.Verify(m => m.NavigateHome());
        }

        private static OpenSafeViewModel CreateMockedOpenSafeViewModelViewModel(
            IRepositoryStorageService repositoryStorageService, INavigationService navigationService = null)
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
                null);
        }
    }
}
