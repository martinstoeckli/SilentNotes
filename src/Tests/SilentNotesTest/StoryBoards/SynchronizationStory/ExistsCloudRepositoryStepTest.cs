using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using SilentNotes.Models;
using SilentNotes.Services;
using SilentNotes.Stories;
using SilentNotes.Stories.SynchronizationStory;
using VanillaCloudStorageClient;

namespace SilentNotesTest.Stories.SynchronizationStory
{
    [TestFixture]
    public class ExistsCloudRepositoryStepTest
    {
        [Test]
        public async Task AccountIsStoredWhenDifferent()
        {
            SerializeableCloudStorageCredentials credentialsFromSession = new SerializeableCloudStorageCredentials { CloudStorageId = CloudStorageClientFactory.CloudStorageIdDropbox };
            SettingsModel settingsModel = new SettingsModel { Credentials = new SerializeableCloudStorageCredentials { CloudStorageId = CloudStorageClientFactory.CloudStorageIdFtp } };
            var model = new SynchronizationStoryModel
            {
                StoryMode = StoryMode.Silent,
                Credentials = credentialsFromSession,
            };

            Mock<ISettingsService> settingsService = new Mock<ISettingsService>();
            settingsService.
                Setup(m => m.LoadSettingsOrDefault()).Returns(settingsModel);
            Mock<ICloudStorageClient> cloudStorageClient = new Mock<ICloudStorageClient>();
            cloudStorageClient.
                Setup(m => m.ExistsFileAsync(It.IsAny<string>(), It.IsAny<CloudStorageCredentials>())).
                ReturnsAsync(true);

            var serviceCollection = new ServiceCollection();
            serviceCollection
                .AddSingleton<ISettingsService>(settingsService.Object)
                .AddSingleton<ILanguageService>(CommonMocksAndStubs.LanguageService())
                .AddSingleton<ICloudStorageClientFactory>(CommonMocksAndStubs.CloudStorageClientFactory(cloudStorageClient.Object));

            // Run step
            var step = new ExistsCloudRepositoryStep();
            var result = await step.RunStep(model, serviceCollection.BuildServiceProvider(), model.StoryMode);

            // Settings are stored with account from session
            settingsService.Verify(m => m.TrySaveSettingsToLocalDevice(It.Is<SettingsModel>(s => s.Credentials == credentialsFromSession)), Times.Once);

            // Next step is called
            Assert.IsInstanceOf<DownloadCloudRepositoryStep>(result.NextStep);
        }

        [Test]
        public async Task CorrectNextStepWhenNoCloudRepositoryExists()
        {
            SerializeableCloudStorageCredentials credentials = new SerializeableCloudStorageCredentials { CloudStorageId = CloudStorageClientFactory.CloudStorageIdDropbox };
            SettingsModel settingsModel = new SettingsModel { Credentials = credentials };
            var model = new SynchronizationStoryModel
            {
                StoryMode = StoryMode.Silent,
                Credentials = credentials,
            };

            Mock<ISettingsService> settingsService = new Mock<ISettingsService>();
            settingsService.
                Setup(m => m.LoadSettingsOrDefault()).Returns(settingsModel);
            Mock<ICloudStorageClient> cloudStorageClient = new Mock<ICloudStorageClient>();
            cloudStorageClient.
                Setup(m => m.ExistsFileAsync(It.IsAny<string>(), It.IsAny<CloudStorageCredentials>())).
                ReturnsAsync(false);

            var serviceCollection = new ServiceCollection();
            serviceCollection
                .AddSingleton<ISettingsService>(settingsService.Object)
                .AddSingleton<ILanguageService>(CommonMocksAndStubs.LanguageService())
                .AddSingleton<ICloudStorageClientFactory>(CommonMocksAndStubs.CloudStorageClientFactory(cloudStorageClient.Object));

            // Run step
            var step = new ExistsCloudRepositoryStep();
            var result = await step.RunStep(model, serviceCollection.BuildServiceProvider(), model.StoryMode);

            // Settings are not stored because they are equal
            settingsService.Verify(m => m.TrySaveSettingsToLocalDevice(It.IsAny<SettingsModel>()), Times.Never);

            // Next step is called
            Assert.IsInstanceOf<StoreLocalRepositoryToCloudAndQuitStep>(result.NextStep);
        }
    }
}
