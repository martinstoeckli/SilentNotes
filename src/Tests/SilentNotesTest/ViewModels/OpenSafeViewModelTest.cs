using System;
using Moq;
using NUnit.Framework;
using SilentNotes;
using SilentNotes.Controllers;
using SilentNotes.Models;
using SilentNotes.Services;
using SilentNotes.ViewModels;

namespace SilentNotesTest.ViewModels
{
    [TestFixture]
    public class OpenSafeViewModelTest
    {
        [Test]
        public void ResetSafeRemovesNotesAndSafes()
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
            Mock<IFeedbackService> feedbackService = new Mock<IFeedbackService>();
            feedbackService.
                Setup(m => m.ShowMessageAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButtons>(), It.IsAny<bool>()))
                .ReturnsAsync(MessageBoxResult.Continue);

            OpenSafeViewModel viewModel = new OpenSafeViewModel(
                navigationService.Object,
                CommonMocksAndStubs.LanguageService(),
                new Mock<ISvgIconService>().Object,
                new Mock<IThemeService>().Object,
                new Mock<IBaseUrlService>().Object,
                feedbackService.Object,
                CommonMocksAndStubs.CryptoRandomService(),
                new Mock<ISettingsService>().Object,
                repositoryStorageService.Object,
                null);

            viewModel.ResetSafeCommand.Execute(null);

            // Note is deleted and added to the deleted list
            Assert.AreEqual(1, repository.Notes.Count);
            Assert.AreEqual(note2Id, repository.Notes[0].Id);
            Assert.AreEqual(1, repository.DeletedNotes.Count);
            Assert.AreEqual(note1Id, repository.DeletedNotes[0]);

            // Safes are removed
            Assert.AreEqual(0, repository.Safes.Count);

            // Is marked as modified and navigated away, so it will be stored.
            Assert.IsTrue(viewModel.Modified);
            navigationService.Verify(m => m.Navigate(It.Is<Navigation>(v => v.ControllerId == ControllerNames.OpenSafe)));
            feedbackService.Verify(m => m.ShowMessageAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButtons>(), It.Is<bool>(v => v == true)), Times.Once);
        }
    }
}
