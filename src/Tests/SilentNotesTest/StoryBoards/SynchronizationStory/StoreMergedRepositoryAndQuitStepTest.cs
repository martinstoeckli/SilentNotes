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
    public class StoreMergedRepositoryAndQuitStepTest
    {
        [Test]
        public void DoNotStoreAnythingWhenRepositoriesAreSame()
        {
            NoteRepositoryModel repositoryModel = new NoteRepositoryModel();
            repositoryModel.Revision = NoteRepositoryModel.NewestSupportedRevision;

            Mock<IStoryBoard> storyBoard = new Mock<IStoryBoard>();
            storyBoard.
                Setup(m => m.LoadFromSession<NoteRepositoryModel>(It.Is<int>(p => p == SynchronizationStorySessionKey.CloudRepository.ToInt()))).
                Returns(repositoryModel); // same as from repositoryStorageService
            Mock<ISettingsService> settingsService = new Mock<ISettingsService>();
            Mock<IRepositoryStorageService> repositoryStorageService = new Mock<IRepositoryStorageService>();
            repositoryStorageService.
                Setup(m => m.LoadRepositoryOrDefault(out repositoryModel)); // same as from storyBoard
            Mock<ICloudStorageClient> cloudStorageClient = new Mock<ICloudStorageClient>();

            // Run step
            var step = new StoreMergedRepositoryAndQuitStep(
                SynchronizationStoryStepId.StoreLocalRepositoryToCloudAndQuit,
                storyBoard.Object,
                CommonMocksAndStubs.LanguageService(),
                CommonMocksAndStubs.FeedbackService(),
                settingsService.Object,
                CommonMocksAndStubs.CryptoRandomService(),
                repositoryStorageService.Object,
                CommonMocksAndStubs.CloudStorageClientFactory(cloudStorageClient.Object));
            Assert.DoesNotThrowAsync(step.Run);

            // repository is not stored to the local device
            repositoryStorageService.Verify(m => m.TrySaveRepository(It.IsAny<NoteRepositoryModel>()), Times.Never);

            // repository is not stored to the cloud
            cloudStorageClient.Verify(m => m.UploadFileAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<CloudStorageCredentials>()), Times.Never);

            // Next step is called
            storyBoard.Verify(m => m.ContinueWith(It.Is<SynchronizationStoryStepId>(x => x == SynchronizationStoryStepId.StopAndShowRepository)), Times.Once);
        }

        [Test]
        public void StoreMergedRepositoryWhenDifferent()
        {
            const string transferCode = "abcdefgh";
            SerializeableCloudStorageCredentials credentialsFromSession = new SerializeableCloudStorageCredentials();
            var settingsModel = CreateSettingsModel(transferCode);
            NoteRepositoryModel repositoryModelLocal = new NoteRepositoryModel();
            repositoryModelLocal.Notes.Add(new NoteModel());
            NoteRepositoryModel repositoryModelCloud = new NoteRepositoryModel();
            repositoryModelCloud.Notes.Add(new NoteModel());

            Mock<IStoryBoard> storyBoard = new Mock<IStoryBoard>();
            storyBoard.
                Setup(m => m.LoadFromSession<SerializeableCloudStorageCredentials>(It.Is<int>(p => p == SynchronizationStorySessionKey.CloudStorageCredentials.ToInt()))).
                Returns(credentialsFromSession);
            storyBoard.
                Setup(m => m.LoadFromSession<NoteRepositoryModel>(It.Is<int>(p => p == SynchronizationStorySessionKey.CloudRepository.ToInt()))).
                Returns(repositoryModelCloud); // same as from repositoryStorageService
            Mock<ISettingsService> settingsService = new Mock<ISettingsService>();
            settingsService.
                Setup(m => m.LoadSettingsOrDefault()).Returns(settingsModel);
            Mock<IRepositoryStorageService> repositoryStorageService = new Mock<IRepositoryStorageService>();
            repositoryStorageService.
                Setup(m => m.LoadRepositoryOrDefault(out repositoryModelLocal)); // same as from storyBoard
            Mock<ICloudStorageClient> cloudStorageClient = new Mock<ICloudStorageClient>();

            // Run step
            var step = new StoreMergedRepositoryAndQuitStep(
                SynchronizationStoryStepId.StoreLocalRepositoryToCloudAndQuit,
                storyBoard.Object,
                CommonMocksAndStubs.LanguageService(),
                CommonMocksAndStubs.FeedbackService(),
                settingsService.Object,
                CommonMocksAndStubs.CryptoRandomService(),
                repositoryStorageService.Object,
                CommonMocksAndStubs.CloudStorageClientFactory(cloudStorageClient.Object));
            Assert.DoesNotThrowAsync(step.Run);

            // repository is stored to the local device
            repositoryStorageService.Verify(m => m.TrySaveRepository(It.IsAny<NoteRepositoryModel>()), Times.Once);

            // repository is stored to the cloud
            cloudStorageClient.Verify(m => m.UploadFileAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<CloudStorageCredentials>()), Times.Once);

            // Next step is called
            storyBoard.Verify(m => m.ContinueWith(It.Is<SynchronizationStoryStepId>(x => x == SynchronizationStoryStepId.StopAndShowRepository)), Times.Once);
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
