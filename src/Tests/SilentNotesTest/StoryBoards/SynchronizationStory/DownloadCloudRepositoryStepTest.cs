using Moq;
using NUnit.Framework;
using SilentNotes.Services;
using SilentNotes.StoryBoards;
using SilentNotes.StoryBoards.SynchronizationStory;
using VanillaCloudStorageClient;

namespace SilentNotesTest.StoryBoards.SynchronizationStory
{
    [TestFixture]
    public class DownloadCloudRepositoryStepTest
    {
        [Test]
        public void SuccessfulFlowEndsInNextStep()
        {
            SerializeableCloudStorageCredentials credentialsFromSession = new SerializeableCloudStorageCredentials();
            byte[] repositoryFromSession = null;
            byte[] repository = new byte[8];

            Mock<IStoryBoard> storyBoard = new Mock<IStoryBoard>();
            storyBoard.
                Setup(m => m.Session.Load<SerializeableCloudStorageCredentials>(It.Is<SynchronizationStorySessionKey>(p => p == SynchronizationStorySessionKey.CloudStorageCredentials))).
                Returns(credentialsFromSession);
            storyBoard.
                Setup(m => m.Session.TryLoad(It.Is<SynchronizationStorySessionKey>(p => p == SynchronizationStorySessionKey.BinaryCloudRepository), out repositoryFromSession)).
                Returns(false);
            Mock<ICloudStorageClient> cloudStorageClient = new Mock<ICloudStorageClient>();
            cloudStorageClient.
                Setup(m => m.DownloadFileAsync(It.IsAny<string>(), It.IsAny<CloudStorageCredentials>())).
                ReturnsAsync(repository);

            // Run step
            var step = new DownloadCloudRepositoryStep(
                SynchronizationStoryStepId.DownloadCloudRepository,
                storyBoard.Object,
                CommonMocksAndStubs.LanguageService(),
                CommonMocksAndStubs.FeedbackService(),
                CommonMocksAndStubs.CloudStorageClientFactory(cloudStorageClient.Object));
            Assert.DoesNotThrowAsync(step.Run);

            // Repository was stored in session
            storyBoard.Verify(m => m.Session.Store(It.Is<SynchronizationStorySessionKey>(p => p == SynchronizationStorySessionKey.BinaryCloudRepository), It.Is<object>(p => p == repository)), Times.Once);

            // Next step is called
            storyBoard.Verify(m => m.ContinueWith(It.Is<SynchronizationStoryStepId>(x => x == SynchronizationStoryStepId.ExistsTransferCode)), Times.Once);
        }

        [Test]
        public void ErrorMessageIsShownInCaseOfException()
        {
            SerializeableCloudStorageCredentials credentialsFromSession = new SerializeableCloudStorageCredentials();
            byte[] repositoryFromSession = null;
            byte[] repository = new byte[8];

            Mock<IStoryBoard> storyBoard = new Mock<IStoryBoard>();
            storyBoard.
                Setup(m => m.Session.Load<SerializeableCloudStorageCredentials>(It.Is<SynchronizationStorySessionKey>(p => p == SynchronizationStorySessionKey.CloudStorageCredentials))).
                Returns(credentialsFromSession);
            storyBoard.
                Setup(m => m.Session.TryLoad(It.Is<SynchronizationStorySessionKey>(p => p == SynchronizationStorySessionKey.BinaryCloudRepository), out repositoryFromSession)).
                Returns(false);
            Mock<IFeedbackService> feedbackService = new Mock<IFeedbackService>();
            Mock<ICloudStorageClient> cloudStorageClient = new Mock<ICloudStorageClient>();
            cloudStorageClient.
                Setup(m => m.DownloadFileAsync(It.IsAny<string>(), It.IsAny<CloudStorageCredentials>())).
                Throws<ConnectionFailedException>();

            // Run step
            var step = new DownloadCloudRepositoryStep(
                SynchronizationStoryStepId.DownloadCloudRepository,
                storyBoard.Object,
                CommonMocksAndStubs.LanguageService(),
                feedbackService.Object,
                CommonMocksAndStubs.CloudStorageClientFactory(cloudStorageClient.Object));
            Assert.DoesNotThrowAsync(step.Run);

            // Error message was shown
            feedbackService.Verify(m => m.ShowToast(It.IsAny<string>()), Times.Once);

            // Next step is called
            storyBoard.Verify(m => m.ContinueWith(It.Is<SynchronizationStoryStepId>(x => x == SynchronizationStoryStepId.ExistsTransferCode)), Times.Never);
        }
    }
}
