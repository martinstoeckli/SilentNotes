using Moq;
using NUnit.Framework;
using SilentNotes.Models;
using SilentNotes.Services;
//using SilentNotes.StoryBoards;
//using SilentNotes.StoryBoards.SynchronizationStory;
using VanillaCloudStorageClient;

namespace SilentNotesTest.StoryBoards.SynchronizationStory
{
    [TestFixture]
    public class HandleOAuthRedirectStepTest
    {
        // todo: reactivate tests
        //[Test]
        //public void AccountIsStoredAfterGettingNewRefreshToken()
        //{
        //    SerializeableCloudStorageCredentials credentialsFromSession = new SerializeableCloudStorageCredentials { CloudStorageId = CloudStorageClientFactory.CloudStorageIdOneDrive };
        //    string dummy = "dummy";
        //    SettingsModel settingsModel = new SettingsModel { Credentials = new SerializeableCloudStorageCredentials { CloudStorageId = CloudStorageClientFactory.CloudStorageIdOneDrive } };

        //    Mock<IStoryBoard> storyBoard = new Mock<IStoryBoard>();
        //    storyBoard.
        //        Setup(m => m.Session.TryLoad(It.Is<SynchronizationStorySessionKey>(p => p == SynchronizationStorySessionKey.CloudStorageCredentials), out credentialsFromSession)).
        //        Returns(true);
        //    storyBoard.
        //        Setup(m => m.Session.TryLoad(It.IsAny<SynchronizationStorySessionKey>(), out dummy)).
        //        Returns(true);

        //    Mock<ISettingsService> settingsService = new Mock<ISettingsService>();
        //    settingsService.
        //        Setup(m => m.LoadSettingsOrDefault()).Returns(settingsModel);

        //    CloudStorageToken newToken = new CloudStorageToken { AccessToken = "at", RefreshToken = "rt" };
        //    Mock<ICloudStorageClient> cloudStorageClient = new Mock<ICloudStorageClient>();
        //    Mock<IOAuth2CloudStorageClient> oauthCloudStorageClient = cloudStorageClient.As<IOAuth2CloudStorageClient>();
        //    oauthCloudStorageClient.
        //        Setup(m => m.FetchTokenAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).
        //        ReturnsAsync(newToken);

        //    // Run step
        //    var step = new HandleOAuthRedirectStep(
        //        SynchronizationStoryStepId.HandleOAuthRedirect,
        //        storyBoard.Object,
        //        CommonMocksAndStubs.LanguageService(),
        //        CommonMocksAndStubs.FeedbackService(),
        //        settingsService.Object,
        //        CommonMocksAndStubs.CloudStorageClientFactory(cloudStorageClient.Object));
        //    Assert.DoesNotThrowAsync(step.Run);

        //    // New token is stored
        //    settingsService.Verify(m => m.TrySaveSettingsToLocalDevice(It.Is<SettingsModel>(s => s.Credentials.Token.RefreshToken == "rt")), Times.Once);
        //}
    }
}
