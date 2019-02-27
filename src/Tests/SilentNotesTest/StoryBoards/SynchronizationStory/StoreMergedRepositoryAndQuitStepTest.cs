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
            Mock<ILanguageService> languageService = new Mock<ILanguageService>();
            Mock<IFeedbackService> feedbackService = new Mock<IFeedbackService>();
            Mock<ISettingsService> settingsService = new Mock<ISettingsService>();
            Mock<IRepositoryStorageService> repositoryStorageService = new Mock<IRepositoryStorageService>();
            repositoryStorageService.
                Setup(m => m.LoadRepositoryOrDefault(out repositoryModel)); // same as from storyBoard
            Mock<ICloudStorageService> cloudStorageService = new Mock<ICloudStorageService>();
            Mock<ICloudStorageServiceFactory> cloudStorageServiceFactory = new Mock<ICloudStorageServiceFactory>();

            // Run step
            var step = new StoreMergedRepositoryAndQuitStep(
                SynchronizationStoryStepId.StoreLocalRepositoryToCloudAndQuit.ToInt(), storyBoard.Object, languageService.Object, feedbackService.Object, settingsService.Object, new RandomSource4UnitTest(), repositoryStorageService.Object, cloudStorageServiceFactory.Object);
            Assert.DoesNotThrowAsync(step.Run);

            // repository is not stored to the local device
            repositoryStorageService.Verify(m => m.TrySaveRepository(It.IsAny<NoteRepositoryModel>()), Times.Never);

            // repository is not stored to the cloud
            cloudStorageService.Verify(m => m.UploadRepositoryAsync(It.IsAny<byte[]>()), Times.Never);

            // Next step is called
            storyBoard.Verify(m => m.ContinueWith(It.Is<int>(x => x == SynchronizationStoryStepId.StopAndShowRepository.ToInt())), Times.Once);
        }

        [Test]
        public void StoreMergedRepositoryWhenDifferent()
        {
            const string transferCode = "abcdefgh";
            var settingsModel = CreateSettingsModel(transferCode);
            NoteRepositoryModel repositoryModelLocal = new NoteRepositoryModel();
            repositoryModelLocal.Notes.Add(new NoteModel());
            NoteRepositoryModel repositoryModelCloud = new NoteRepositoryModel();
            repositoryModelCloud.Notes.Add(new NoteModel());

            Mock<IStoryBoard> storyBoard = new Mock<IStoryBoard>();
            storyBoard.
                Setup(m => m.LoadFromSession<NoteRepositoryModel>(It.Is<int>(p => p == SynchronizationStorySessionKey.CloudRepository.ToInt()))).
                Returns(repositoryModelCloud); // same as from repositoryStorageService
            Mock<ILanguageService> languageService = new Mock<ILanguageService>();
            Mock<IFeedbackService> feedbackService = new Mock<IFeedbackService>();
            Mock<ISettingsService> settingsService = new Mock<ISettingsService>();
            settingsService.
                Setup(m => m.LoadSettingsOrDefault()).Returns(settingsModel);
            Mock<IRepositoryStorageService> repositoryStorageService = new Mock<IRepositoryStorageService>();
            repositoryStorageService.
                Setup(m => m.LoadRepositoryOrDefault(out repositoryModelLocal)); // same as from storyBoard
            Mock<ICloudStorageService> cloudStorageService = new Mock<ICloudStorageService>();
            Mock<ICloudStorageServiceFactory> cloudStorageServiceFactory = new Mock<ICloudStorageServiceFactory>();
            cloudStorageServiceFactory.
                Setup(m => m.Create(It.IsAny<CloudStorageAccount>())).
                Returns(cloudStorageService.Object);

            // Run step
            var step = new StoreMergedRepositoryAndQuitStep(
                SynchronizationStoryStepId.StoreLocalRepositoryToCloudAndQuit.ToInt(), storyBoard.Object, languageService.Object, feedbackService.Object, settingsService.Object, new RandomSource4UnitTest(), repositoryStorageService.Object, cloudStorageServiceFactory.Object);
            Assert.DoesNotThrowAsync(step.Run);

            // repository is stored to the local device
            repositoryStorageService.Verify(m => m.TrySaveRepository(It.IsAny<NoteRepositoryModel>()), Times.Once);

            // repository is stored to the cloud
            cloudStorageService.Verify(m => m.UploadRepositoryAsync(It.IsAny<byte[]>()), Times.Once);

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
