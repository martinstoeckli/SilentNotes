using Moq;
using NUnit.Framework;
using SilentNotes.Services;
using SilentNotes.Services.CloudStorageServices;
using SilentNotes.StoryBoards;
using SilentNotes.StoryBoards.SynchronizationStory;

namespace SilentNotesTest.StoryBoards.SynchronizationStory
{
    [TestFixture]
    public class DownloadCloudRepositoryStepTest
    {
        [Test]
        public void SuccessfulFlowEndsInNextStep()
        {
            CloudStorageAccount account = null;
            byte[] repository = new byte[8];

            Mock<IStoryBoard> storyBoard = new Mock<IStoryBoard>();
            storyBoard.
                Setup(m => m.TryLoadFromSession(It.Is<int>(p => p == SynchronizationStorySessionKey.BinaryCloudRepository.ToInt()), out account)).
                Returns(false);
            Mock<ILanguageService> languageService = new Mock<ILanguageService>();
            Mock<IFeedbackService> feedbackService = new Mock<IFeedbackService>();
            Mock<ICloudStorageService> cloudStorageService = new Mock<ICloudStorageService>();
            cloudStorageService.
                Setup(m => m.DownloadRepositoryAsync()).
                ReturnsAsync(repository);
            Mock<ICloudStorageServiceFactory> cloudStorageServiceFactory = new Mock<ICloudStorageServiceFactory>();
            cloudStorageServiceFactory.
                Setup(m => m.Create(It.IsAny<CloudStorageAccount>())).
                Returns(cloudStorageService.Object);

            // Run step
            var step = new DownloadCloudRepositoryStep(SynchronizationStoryStepId.DownloadCloudRepository.ToInt(), storyBoard.Object, languageService.Object, feedbackService.Object, cloudStorageServiceFactory.Object);
            Assert.DoesNotThrowAsync(step.Run);

            // Repository was stored in session
            storyBoard.Verify(m => m.StoreToSession(It.Is<int>(p => p == SynchronizationStorySessionKey.BinaryCloudRepository.ToInt()), It.Is<object>(p => p == repository)), Times.Once);

            // Next step is called
            storyBoard.Verify(m => m.ContinueWith(It.Is<int>(x => x == SynchronizationStoryStepId.ExistsTransferCode.ToInt())), Times.Once);
        }

        [Test]
        public void ErrorMessageIsShownInCaseOfException()
        {
            CloudStorageAccount account = null;
            byte[] repository = new byte[8];

            Mock<IStoryBoard> storyBoard = new Mock<IStoryBoard>();
            storyBoard.
                Setup(m => m.TryLoadFromSession(It.Is<int>(p => p == SynchronizationStorySessionKey.BinaryCloudRepository.ToInt()), out account)).
                Returns(false);
            Mock<ILanguageService> languageService = new Mock<ILanguageService>();
            Mock<IFeedbackService> feedbackService = new Mock<IFeedbackService>();
            Mock<ICloudStorageService> cloudStorageService = new Mock<ICloudStorageService>();
            cloudStorageService.
                Setup(m => m.DownloadRepositoryAsync()).
                Throws<CloudStorageConnectionException>();
            Mock<ICloudStorageServiceFactory> cloudStorageServiceFactory = new Mock<ICloudStorageServiceFactory>();
            cloudStorageServiceFactory.
                Setup(m => m.Create(It.IsAny<CloudStorageAccount>())).
                Returns(cloudStorageService.Object);

            // Run step
            var step = new DownloadCloudRepositoryStep(SynchronizationStoryStepId.DownloadCloudRepository.ToInt(), storyBoard.Object, languageService.Object, feedbackService.Object, cloudStorageServiceFactory.Object);
            Assert.DoesNotThrowAsync(step.Run);

            // Error message was shown
            feedbackService.Verify(m => m.ShowToast(It.IsAny<string>()), Times.Once);

            // Next step is called
            storyBoard.Verify(m => m.ContinueWith(It.Is<int>(x => x == SynchronizationStoryStepId.ExistsTransferCode.ToInt())), Times.Never);
        }
    }
}
