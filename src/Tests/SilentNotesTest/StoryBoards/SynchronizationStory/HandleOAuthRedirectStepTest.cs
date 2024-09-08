using Microsoft.Extensions.DependencyInjection;
using Moq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SilentNotes.Models;
using SilentNotes.Services;
using SilentNotes.Stories;
using SilentNotes.Stories.SynchronizationStory;
using VanillaCloudStorageClient;

namespace SilentNotesTest.Stories.SynchronizationStory
{
    [TestClass]
    public class HandleOAuthRedirectStepTest
    {
        [TestMethod]
        public async Task AccountIsStoredAfterGettingNewRefreshToken()
        {
            SerializeableCloudStorageCredentials credentialsFromSession = new SerializeableCloudStorageCredentials { CloudStorageId = CloudStorageClientFactory.CloudStorageIdOneDrive };
            SettingsModel settingsModel = new SettingsModel { Credentials = new SerializeableCloudStorageCredentials { CloudStorageId = CloudStorageClientFactory.CloudStorageIdOneDrive } };
            var model = new SynchronizationStoryModel
            {
                StoryMode = StoryMode.Silent,
                Credentials = credentialsFromSession,
                OauthState = "dummy",
                OauthCodeVerifier = "dummy",
                OauthRedirectUrl = "dummy",
            };

            Mock<ISettingsService> settingsService = new Mock<ISettingsService>();
            settingsService.
                Setup(m => m.LoadSettingsOrDefault()).Returns(settingsModel);

            CloudStorageToken newToken = new CloudStorageToken { AccessToken = "at", RefreshToken = "rt" };
            Mock<ICloudStorageClient> cloudStorageClient = new Mock<ICloudStorageClient>();
            Mock<IOAuth2CloudStorageClient> oauthCloudStorageClient = cloudStorageClient.As<IOAuth2CloudStorageClient>();
            oauthCloudStorageClient.
                Setup(m => m.FetchTokenAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).
                ReturnsAsync(newToken);

            var serviceCollection = new ServiceCollection();
            serviceCollection
                .AddSingleton<ISettingsService>(settingsService.Object)
                .AddSingleton<ILanguageService>(CommonMocksAndStubs.LanguageService())
                .AddSingleton<ICloudStorageClientFactory>(CommonMocksAndStubs.CloudStorageClientFactory(cloudStorageClient.Object));

            // Run step
            var step = new HandleOAuthRedirectStep();
            var result = await step.RunStep(model, serviceCollection.BuildServiceProvider(), model.StoryMode);

            // New token is stored
            settingsService.Verify(m => m.TrySaveSettingsToLocalDevice(It.Is<SettingsModel>(s => s.Credentials.Token.RefreshToken == "rt")), Times.Once);

            // Next step is called
            Assert.IsInstanceOfType<ExistsCloudRepositoryStep>(result.NextStep);
        }

        [TestMethod]
        public async Task AbortStoryWithoutNewRefreshToken()
        {
            SerializeableCloudStorageCredentials credentialsFromSession = new SerializeableCloudStorageCredentials { CloudStorageId = CloudStorageClientFactory.CloudStorageIdOneDrive };
            SettingsModel settingsModel = new SettingsModel { Credentials = new SerializeableCloudStorageCredentials { CloudStorageId = CloudStorageClientFactory.CloudStorageIdOneDrive } };
            var model = new SynchronizationStoryModel
            {
                StoryMode = StoryMode.Silent,
                Credentials = credentialsFromSession,
                OauthState = "dummy",
                OauthCodeVerifier = "dummy",
                OauthRedirectUrl = "dummy",
            };

            Mock<ISettingsService> settingsService = new Mock<ISettingsService>();
            settingsService.
                Setup(m => m.LoadSettingsOrDefault()).Returns(settingsModel);

            Mock<ICloudStorageClient> cloudStorageClient = new Mock<ICloudStorageClient>();
            Mock<IOAuth2CloudStorageClient> oauthCloudStorageClient = cloudStorageClient.As<IOAuth2CloudStorageClient>();

            var serviceCollection = new ServiceCollection();
            serviceCollection
                .AddSingleton<ISettingsService>(settingsService.Object)
                .AddSingleton<ILanguageService>(CommonMocksAndStubs.LanguageService())
                .AddSingleton<ICloudStorageClientFactory>(CommonMocksAndStubs.CloudStorageClientFactory(cloudStorageClient.Object));

            // Run step
            var step = new HandleOAuthRedirectStep();
            var result = await step.RunStep(model, serviceCollection.BuildServiceProvider(), model.StoryMode);

            // No token is stored
            settingsService.Verify(m => m.TrySaveSettingsToLocalDevice(It.IsAny<SettingsModel>()), Times.Never);

            // Next step is end of story
            Assert.IsInstanceOfType<StopAndShowRepositoryStep>(result.NextStep);
        }
    }
}
