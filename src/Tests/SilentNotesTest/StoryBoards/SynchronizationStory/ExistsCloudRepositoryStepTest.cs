using Moq;
using NUnit.Framework;
using SilentNotes.Models;
using SilentNotes.Services;
using SilentNotes.Services.CloudStorageServices;
using SilentNotes.StoryBoards;
using SilentNotes.StoryBoards.SynchronizationStory;

namespace SilentNotesTest.StoryBoards.SynchronizationStory
{
    [TestFixture]
    public class ExistsCloudRepositoryStepTest
    {
        [Test]
        public void AccountIsStoredWhenDifferent()
        {
            CloudStorageAccount accountFromSession = new CloudStorageAccount { CloudType = CloudStorageType.Dropbox };
            SettingsModel settingsModel = new SettingsModel { CloudStorageAccount = new CloudStorageAccount { CloudType = CloudStorageType.FTP } };

            Mock<IStoryBoard> storyBoard = new Mock<IStoryBoard>();
            storyBoard.
                Setup(m => m.LoadFromSession<CloudStorageAccount>(It.Is<int>(p => p == SynchronizationStorySessionKey.CloudStorageAccount.ToInt()))).
                Returns(accountFromSession);
            Mock<ILanguageService> languageService = new Mock<ILanguageService>();
            Mock<IFeedbackService> feedbackService = new Mock<IFeedbackService>();
            Mock<ISettingsService> settingsService = new Mock<ISettingsService>();
            settingsService.
                Setup(m => m.LoadSettingsOrDefault()).Returns(settingsModel);
            Mock<ICloudStorageService> cloudStorageService = new Mock<ICloudStorageService>();
            cloudStorageService.
                Setup(m => m.ExistsRepositoryAsync()).
                ReturnsAsync(true);
            Mock<ICloudStorageServiceFactory> cloudStorageServiceFactory = new Mock<ICloudStorageServiceFactory>();
            cloudStorageServiceFactory.
                Setup(m => m.Create(It.IsAny<CloudStorageAccount>())).
                Returns(cloudStorageService.Object);

            // Run step
            var step = new ExistsCloudRepositoryStep(
                SynchronizationStoryStepId.ExistsCloudRepository.ToInt(), storyBoard.Object, languageService.Object, feedbackService.Object, settingsService.Object, cloudStorageServiceFactory.Object);
            Assert.DoesNotThrowAsync(step.Run);

            // Settings are stored with account from session
            settingsService.Verify(m => m.TrySaveSettingsToLocalDevice(It.Is<SettingsModel>(s => s.CloudStorageAccount == accountFromSession)), Times.Once);

            // Next step is called
            storyBoard.Verify(m => m.ContinueWith(It.Is<int>(x => x == SynchronizationStoryStepId.DownloadCloudRepository.ToInt())), Times.Once);
        }

        [Test]
        public void CorrectNextStepWhenNoCloudRepositoryExists()
        {
            CloudStorageAccount account = new CloudStorageAccount { CloudType = CloudStorageType.Dropbox };
            SettingsModel settingsModel = new SettingsModel { CloudStorageAccount = account };

            Mock<IStoryBoard> storyBoard = new Mock<IStoryBoard>();
            storyBoard.
                Setup(m => m.LoadFromSession<CloudStorageAccount>(It.Is<int>(p => p == SynchronizationStorySessionKey.CloudStorageAccount.ToInt()))).
                Returns(account);
            Mock<ILanguageService> languageService = new Mock<ILanguageService>();
            Mock<IFeedbackService> feedbackService = new Mock<IFeedbackService>();
            Mock<ISettingsService> settingsService = new Mock<ISettingsService>();
            settingsService.
                Setup(m => m.LoadSettingsOrDefault()).Returns(settingsModel);
            Mock<ICloudStorageService> cloudStorageService = new Mock<ICloudStorageService>();
            cloudStorageService.
                Setup(m => m.ExistsRepositoryAsync()).
                ReturnsAsync(false);
            Mock<ICloudStorageServiceFactory> cloudStorageServiceFactory = new Mock<ICloudStorageServiceFactory>();
            cloudStorageServiceFactory.
                Setup(m => m.Create(It.IsAny<CloudStorageAccount>())).
                Returns(cloudStorageService.Object);

            // Run step
            var step = new ExistsCloudRepositoryStep(
                SynchronizationStoryStepId.ExistsCloudRepository.ToInt(), storyBoard.Object, languageService.Object, feedbackService.Object, settingsService.Object, cloudStorageServiceFactory.Object);
            Assert.DoesNotThrowAsync(step.Run);

            // Settings are not stored because they are equal
            settingsService.Verify(m => m.TrySaveSettingsToLocalDevice(It.IsAny<SettingsModel>()), Times.Never);

            // Next step is called
            storyBoard.Verify(m => m.ContinueWith(It.Is<int>(x => x == SynchronizationStoryStepId.StoreLocalRepositoryToCloudAndQuit.ToInt())), Times.Once);
        }
    }
}
