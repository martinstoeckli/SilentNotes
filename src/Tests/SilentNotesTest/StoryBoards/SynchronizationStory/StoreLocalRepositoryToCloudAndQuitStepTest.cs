using Moq;
using NUnit.Framework;
using SilentNotes.Crypto.SymmetricEncryption;
using SilentNotes.Models;
using SilentNotes.Services;
using SilentNotes.Services.CloudStorageServices;
using SilentNotes.StoryBoards;
using SilentNotes.StoryBoards.SynchronizationStory;

namespace SilentNotesTest.StoryBoards.SynchronizationStory
{
    [TestFixture]
    public class StoreLocalRepositoryToCloudAndQuitStepTest
    {
        [Test]
        public void GenerateAndStoreNewTransfercode()
        {
            var settingsModel = CreateSettingsModel(null); // Transfercode does not yet exist
            NoteRepositoryModel repositoryModel = new NoteRepositoryModel();

            Mock<IStoryBoard> storyBoard = new Mock<IStoryBoard>();
            Mock<ILanguageService> languageService = new Mock<ILanguageService>();
            Mock<IFeedbackService> feedbackService = new Mock<IFeedbackService>();
            Mock<ISettingsService> settingsService = new Mock<ISettingsService>();
            settingsService.
                Setup(m => m.LoadSettingsOrDefault()).Returns(settingsModel);
            Mock<IRepositoryStorageService> repositoryStorageService = new Mock<IRepositoryStorageService>();
            repositoryStorageService.
                Setup(m => m.LoadRepositoryOrDefault(out repositoryModel));
            Mock<ICloudStorageService> cloudStorageService = new Mock<ICloudStorageService>();
            Mock<ICloudStorageServiceFactory> cloudStorageServiceFactory = new Mock<ICloudStorageServiceFactory>();
            cloudStorageServiceFactory.
                Setup(m => m.Create(It.IsAny<CloudStorageAccount>())).
                Returns(cloudStorageService.Object);

            // Run step
            var step = new StoreLocalRepositoryToCloudAndQuitStep(
                SynchronizationStoryStepId.StoreLocalRepositoryToCloudAndQuit.ToInt(), storyBoard.Object, languageService.Object, feedbackService.Object, settingsService.Object, new RandomSource4UnitTest(), repositoryStorageService.Object, cloudStorageServiceFactory.Object);
            Assert.DoesNotThrowAsync(step.Run);

            // Settings are stored with new transfer code
            settingsService.Verify(m => m.TrySaveSettingsToLocalDevice(It.Is<SettingsModel>(s => !string.IsNullOrEmpty(s.TransferCode))), Times.Once);

            // Next step is called
            storyBoard.Verify(m => m.ContinueWith(It.Is<int>(x => x == SynchronizationStoryStepId.StopAndShowRepository.ToInt())), Times.Once);
        }

        [Test]
        public void KeepExistingTransfercode()
        {
            var settingsModel = CreateSettingsModel("abcdefgh"); // Transfercode exists
            NoteRepositoryModel repositoryModel = new NoteRepositoryModel();

            Mock<IStoryBoard> storyBoard = new Mock<IStoryBoard>();
            Mock<ILanguageService> languageService = new Mock<ILanguageService>();
            Mock<IFeedbackService> feedbackService = new Mock<IFeedbackService>();
            Mock<ISettingsService> settingsService = new Mock<ISettingsService>();
            settingsService.
                Setup(m => m.LoadSettingsOrDefault()).Returns(settingsModel);
            Mock<IRepositoryStorageService> repositoryStorageService = new Mock<IRepositoryStorageService>();
            repositoryStorageService.
                Setup(m => m.LoadRepositoryOrDefault(out repositoryModel));
            Mock<ICloudStorageService> cloudStorageService = new Mock<ICloudStorageService>();
            Mock<ICloudStorageServiceFactory> cloudStorageServiceFactory = new Mock<ICloudStorageServiceFactory>();
            cloudStorageServiceFactory.
                Setup(m => m.Create(It.IsAny<CloudStorageAccount>())).
                Returns(cloudStorageService.Object);

            // Run step
            var step = new StoreLocalRepositoryToCloudAndQuitStep(
                SynchronizationStoryStepId.StoreLocalRepositoryToCloudAndQuit.ToInt(), storyBoard.Object, languageService.Object, feedbackService.Object, settingsService.Object, new RandomSource4UnitTest(), repositoryStorageService.Object, cloudStorageServiceFactory.Object);
            Assert.DoesNotThrowAsync(step.Run);

            // No settings are stored
            settingsService.Verify(m => m.TrySaveSettingsToLocalDevice(It.IsAny<SettingsModel>()), Times.Never);

            // Next step is called
            storyBoard.Verify(m => m.ContinueWith(It.Is<int>(x => x == SynchronizationStoryStepId.StopAndShowRepository.ToInt())), Times.Once);
        }

        private static SettingsModel CreateSettingsModel(string transferCode)
        {
            return new SettingsModel
            {
                TransferCode = transferCode,
                SelectedEncryptionAlgorithm = BouncyCastleTwofishGcm.CryptoAlgorithmName,
            };
        }
    }
}
