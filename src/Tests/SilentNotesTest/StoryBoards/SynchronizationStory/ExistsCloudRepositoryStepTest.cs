using Moq;
using NUnit.Framework;
using SilentNotes.Models;
using SilentNotes.Services;
using SilentNotes.StoryBoards;
using SilentNotes.StoryBoards.SynchronizationStory;
using VanillaCloudStorageClient;

namespace SilentNotesTest.StoryBoards.SynchronizationStory
{
    [TestFixture]
    public class ExistsCloudRepositoryStepTest
    {
        [Test]
        public void AccountIsStoredWhenDifferent()
        {
            SerializeableCloudStorageCredentials credentialsFromSession = new SerializeableCloudStorageCredentials { CloudStorageId = CloudStorageClientFactory.CloudStorageIdDropbox };
            SettingsModel settingsModel = new SettingsModel { Credentials = new SerializeableCloudStorageCredentials { CloudStorageId = CloudStorageClientFactory.CloudStorageIdFtp } };

            Mock<IStoryBoard> storyBoard = new Mock<IStoryBoard>();
            storyBoard.
                Setup(m => m.LoadFromSession<SerializeableCloudStorageCredentials>(It.Is<SynchronizationStorySessionKey>(p => p == SynchronizationStorySessionKey.CloudStorageCredentials))).
                Returns(credentialsFromSession);
            Mock<ISettingsService> settingsService = new Mock<ISettingsService>();
            settingsService.
                Setup(m => m.LoadSettingsOrDefault()).Returns(settingsModel);
            Mock<ICloudStorageClient> cloudStorageClient = new Mock<ICloudStorageClient>();
            cloudStorageClient.
                Setup(m => m.ExistsFileAsync(It.IsAny<string>(), It.IsAny<CloudStorageCredentials>())).
                ReturnsAsync(true);

            // Run step
            var step = new ExistsCloudRepositoryStep(
                SynchronizationStoryStepId.ExistsCloudRepository,
                storyBoard.Object,
                CommonMocksAndStubs.LanguageService(),
                CommonMocksAndStubs.FeedbackService(),
                settingsService.Object,
                CommonMocksAndStubs.CloudStorageClientFactory(cloudStorageClient.Object));
            Assert.DoesNotThrowAsync(step.Run);

            // Settings are stored with account from session
            settingsService.Verify(m => m.TrySaveSettingsToLocalDevice(It.Is<SettingsModel>(s => s.Credentials == credentialsFromSession)), Times.Once);

            // Next step is called
            storyBoard.Verify(m => m.ContinueWith(It.Is<SynchronizationStoryStepId>(x => x == SynchronizationStoryStepId.DownloadCloudRepository)), Times.Once);
        }

        [Test]
        public void CorrectNextStepWhenNoCloudRepositoryExists()
        {
            SerializeableCloudStorageCredentials credentials = new SerializeableCloudStorageCredentials { CloudStorageId = CloudStorageClientFactory.CloudStorageIdDropbox };
            SettingsModel settingsModel = new SettingsModel { Credentials = credentials };

            Mock<IStoryBoard> storyBoard = new Mock<IStoryBoard>();
            storyBoard.
                Setup(m => m.LoadFromSession<SerializeableCloudStorageCredentials>(It.Is<SynchronizationStorySessionKey>(p => p == SynchronizationStorySessionKey.CloudStorageCredentials))).
                Returns(credentials);
            Mock<ISettingsService> settingsService = new Mock<ISettingsService>();
            settingsService.
                Setup(m => m.LoadSettingsOrDefault()).Returns(settingsModel);
            Mock<ICloudStorageClient> cloudStorageClient = new Mock<ICloudStorageClient>();
            cloudStorageClient.
                Setup(m => m.ExistsFileAsync(It.IsAny<string>(), It.IsAny<CloudStorageCredentials>())).
                ReturnsAsync(false);

            // Run step
            var step = new ExistsCloudRepositoryStep(
                SynchronizationStoryStepId.ExistsCloudRepository,
                storyBoard.Object,
                CommonMocksAndStubs.LanguageService(),
                CommonMocksAndStubs.FeedbackService(),
                settingsService.Object,
                CommonMocksAndStubs.CloudStorageClientFactory(cloudStorageClient.Object));
            Assert.DoesNotThrowAsync(step.Run);

            // Settings are not stored because they are equal
            settingsService.Verify(m => m.TrySaveSettingsToLocalDevice(It.IsAny<SettingsModel>()), Times.Never);

            // Next step is called
            storyBoard.Verify(m => m.ContinueWith(It.Is<SynchronizationStoryStepId>(x => x == SynchronizationStoryStepId.StoreLocalRepositoryToCloudAndQuit)), Times.Once);
        }
    }
}
