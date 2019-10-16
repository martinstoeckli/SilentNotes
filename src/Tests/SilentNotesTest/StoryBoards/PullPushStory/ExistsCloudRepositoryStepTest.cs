using Moq;
using NUnit.Framework;
using SilentNotes.Models;
using SilentNotes.Services;
using SilentNotes.StoryBoards;
using SilentNotes.StoryBoards.PullPushStory;
using VanillaCloudStorageClient;

namespace SilentNotesTest.StoryBoards.PullPushStory
{
    [TestFixture]
    public class ExistsCloudRepositoryStepTest
    {
        [Test]
        public void CorrectNextStepWhenCloudRepositoryExists()
        {
            SerializeableCloudStorageCredentials credentials = new SerializeableCloudStorageCredentials { CloudStorageId = CloudStorageClientFactory.CloudStorageIdDropbox };
            SettingsModel settingsModel = new SettingsModel { Credentials = credentials, TransferCode = "abc" };

            Mock<IStoryBoard> storyBoard = new Mock<IStoryBoard>();
            Mock<ISettingsService> settingsService = new Mock<ISettingsService>();
            settingsService.
                Setup(m => m.LoadSettingsOrDefault()).Returns(settingsModel);
            Mock<ICloudStorageClient> cloudStorageClient = new Mock<ICloudStorageClient>();
            cloudStorageClient.
                Setup(m => m.ExistsFileAsync(It.IsAny<string>(), It.IsAny<CloudStorageCredentials>())).
                ReturnsAsync(true);

            // Run step
            var step = new ExistsCloudRepositoryStep(
                PullPushStoryStepId.ExistsCloudRepository,
                storyBoard.Object,
                CommonMocksAndStubs.LanguageService(),
                CommonMocksAndStubs.FeedbackService(),
                settingsService.Object,
                CommonMocksAndStubs.CloudStorageClientFactory(cloudStorageClient.Object));
            Assert.DoesNotThrowAsync(step.Run);

            // Settings are not stored because no token needs to be refreshed
            settingsService.Verify(m => m.TrySaveSettingsToLocalDevice(It.IsAny<SettingsModel>()), Times.Never);

            // Next step is called
            storyBoard.Verify(m => m.ContinueWith(It.Is<PullPushStoryStepId>(x => x == PullPushStoryStepId.DownloadCloudRepository)), Times.Once);
        }

        [Test]
        public void QuitWhenMissingClientOrTransfercode()
        {
            SerializeableCloudStorageCredentials credentials = new SerializeableCloudStorageCredentials { CloudStorageId = CloudStorageClientFactory.CloudStorageIdDropbox };
            SettingsModel settingsModel = new SettingsModel { Credentials = credentials };

            Mock<IStoryBoard> storyBoard = new Mock<IStoryBoard>();
            Mock<ISettingsService> settingsService = new Mock<ISettingsService>();
            settingsService.
                Setup(m => m.LoadSettingsOrDefault()).Returns(settingsModel);
            Mock<ICloudStorageClient> cloudStorageClient = new Mock<ICloudStorageClient>();
            cloudStorageClient.
                Setup(m => m.ExistsFileAsync(It.IsAny<string>(), It.IsAny<CloudStorageCredentials>())).
                ReturnsAsync(true);

            // Run step with missing transfercode
            var step = new ExistsCloudRepositoryStep(
                PullPushStoryStepId.ExistsCloudRepository,
                storyBoard.Object,
                CommonMocksAndStubs.LanguageService(),
                CommonMocksAndStubs.FeedbackService(),
                settingsService.Object,
                CommonMocksAndStubs.CloudStorageClientFactory(cloudStorageClient.Object));
            Assert.DoesNotThrowAsync(step.Run);

            // Next step is called
            storyBoard.Verify(m => m.ContinueWith(It.Is<PullPushStoryStepId>(x => x == PullPushStoryStepId.DownloadCloudRepository)), Times.Never);

            // Run step with missing storage client
            settingsModel.TransferCode = "abc";
            settingsModel.Credentials.CloudStorageId = null;
            Assert.DoesNotThrowAsync(step.Run);

            // Next step is called
            storyBoard.Verify(m => m.ContinueWith(It.Is<PullPushStoryStepId>(x => x == PullPushStoryStepId.DownloadCloudRepository)), Times.Never);
        }
    }
}
