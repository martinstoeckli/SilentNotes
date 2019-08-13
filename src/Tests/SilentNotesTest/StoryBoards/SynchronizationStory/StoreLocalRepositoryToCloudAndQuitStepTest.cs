using Moq;
using NUnit.Framework;
using SilentNotes.Crypto.SymmetricEncryption;
using SilentNotes.Models;
using SilentNotes.Services;
using SilentNotes.StoryBoards;
using SilentNotes.StoryBoards.SynchronizationStory;
using VanillaCloudStorageClient;

namespace SilentNotesTest.StoryBoards.SynchronizationStory
{
    [TestFixture]
    public class StoreLocalRepositoryToCloudAndQuitStepTest
    {
        [Test]
        public void GenerateAndStoreNewTransfercode()
        {
            SerializeableCloudStorageCredentials credentialsFromSession = new SerializeableCloudStorageCredentials();
            var settingsModel = CreateSettingsModel(null); // Transfercode does not yet exist
            NoteRepositoryModel repositoryModel = new NoteRepositoryModel();

            Mock<IStoryBoard> storyBoard = new Mock<IStoryBoard>();
            storyBoard.
                Setup(m => m.LoadFromSession<SerializeableCloudStorageCredentials>(It.Is<int>(p => p == SynchronizationStorySessionKey.CloudStorageCredentials.ToInt()))).
                Returns(credentialsFromSession);
            Mock<ISettingsService> settingsService = new Mock<ISettingsService>();
            settingsService.
                Setup(m => m.LoadSettingsOrDefault()).Returns(settingsModel);
            Mock<IRepositoryStorageService> repositoryStorageService = new Mock<IRepositoryStorageService>();
            repositoryStorageService.
                Setup(m => m.LoadRepositoryOrDefault(out repositoryModel));

            // Run step
            var step = new StoreLocalRepositoryToCloudAndQuitStep(
                SynchronizationStoryStepId.StoreLocalRepositoryToCloudAndQuit.ToInt(),
                storyBoard.Object,
                CommonMocksAndStubs.LanguageService(),
                CommonMocksAndStubs.FeedbackService(),
                settingsService.Object,
                CommonMocksAndStubs.CryptoRandomService(),
                repositoryStorageService.Object,
                CommonMocksAndStubs.CloudStorageClientFactory());
            Assert.DoesNotThrowAsync(step.Run);

            // Settings are stored with new transfer code
            settingsService.Verify(m => m.TrySaveSettingsToLocalDevice(It.Is<SettingsModel>(s => !string.IsNullOrEmpty(s.TransferCode))), Times.Once);

            // Next step is called
            storyBoard.Verify(m => m.ContinueWith(It.Is<int>(x => x == SynchronizationStoryStepId.StopAndShowRepository.ToInt())), Times.Once);
        }

        [Test]
        public void KeepExistingTransfercode()
        {
            SerializeableCloudStorageCredentials credentialsFromSession = new SerializeableCloudStorageCredentials();
            var settingsModel = CreateSettingsModel("abcdefgh"); // Transfercode exists
            NoteRepositoryModel repositoryModel = new NoteRepositoryModel();

            Mock<IStoryBoard> storyBoard = new Mock<IStoryBoard>();
            storyBoard.
                Setup(m => m.LoadFromSession<SerializeableCloudStorageCredentials>(It.Is<int>(p => p == SynchronizationStorySessionKey.CloudStorageCredentials.ToInt()))).
                Returns(credentialsFromSession);
            Mock<ISettingsService> settingsService = new Mock<ISettingsService>();
            settingsService.
                Setup(m => m.LoadSettingsOrDefault()).Returns(settingsModel);
            Mock<IRepositoryStorageService> repositoryStorageService = new Mock<IRepositoryStorageService>();
            repositoryStorageService.
                Setup(m => m.LoadRepositoryOrDefault(out repositoryModel));

            // Run step
            var step = new StoreLocalRepositoryToCloudAndQuitStep(
                SynchronizationStoryStepId.StoreLocalRepositoryToCloudAndQuit.ToInt(),
                storyBoard.Object,
                CommonMocksAndStubs.LanguageService(),
                CommonMocksAndStubs.FeedbackService(),
                settingsService.Object,
                CommonMocksAndStubs.CryptoRandomService(),
                repositoryStorageService.Object,
                CommonMocksAndStubs.CloudStorageClientFactory());
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
